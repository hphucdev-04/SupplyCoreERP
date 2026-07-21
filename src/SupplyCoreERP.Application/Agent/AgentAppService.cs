using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SupplyCoreERP.Agent.Dtos;
using SupplyCoreERP.Enums.Agent;
using SupplyCoreERP.Mcp;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Agent;

[Authorize]
public class AgentAppService : SupplyCore, IAgentAppService
{
    private readonly IAgent _agent;
    private readonly IMcpClientService _mcpClientService;
    private readonly IRepository<AgentSession, Guid> _sessionRepository;
    private readonly IAgentManager _agentManager;
    private readonly ILogger<AgentAppService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public AgentAppService(
        IAgent agent,
        IMcpClientService mcpClientService,
        IRepository<AgentSession, Guid> sessionRepository,
        IAgentManager agentManager,
        ILogger<AgentAppService> logger)
    {
        _agent = agent;
        _mcpClientService = mcpClientService;
        _sessionRepository = sessionRepository;
        _agentManager = agentManager;
        _logger = logger;
    }

    public async Task<object> SendMessageAsync(AgentRequestInputDto input)
    {
        _logger.LogInformation(
            "AgentAppService: Nhan request send-message. InputSessionId={InputSessionId}, CurrentUserId={CurrentUserId}, TextLength={TextLength}.",
            input.SessionId,
            CurrentUser.Id,
            input.Text?.Length ?? 0);

        // 1. Phục hồi hoặc khởi tạo phiên làm việc qua AgentManager
        AgentSession session = await _agentManager.GetOrCreateSessionAsync(input.SessionId, CurrentUser.Id ?? Guid.Empty);

        _logger.LogInformation(
            "AgentAppService: Da lay hoac tao session. SessionId={SessionId}.",
            session.Id);

        IQueryable<AgentSession> queryable = await _sessionRepository.GetQueryableAsync();
        session = await queryable
            .Include(s => s.Messages)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == session.Id)
            ?? throw new UserFriendlyException("Không thể khởi tạo phiên làm việc.");

        _logger.LogInformation(
            "AgentAppService: Da tai session tu database. SessionId={SessionId}, ExistingMessageCount={ExistingMessageCount}, ExistingTaskCount={ExistingTaskCount}.",
            session.Id,
            session.Messages.Count,
            session.Tasks.Count);

        // Lưu tin nhắn mới của người dùng (tự động chạy DLP ở tầng Domain)
        await _agentManager.AddMessageAsync(session, "user", input.Text);

        // 2. Nạp lịch sử tin nhắn đã được tối ưu hóa trực tiếp từ database
        List<AgentMessage> optimizedHistory = await _agentManager.GetOptimizedHistoryAsync(session.Id);
        List<AgentSessionMessageDto> steps = MapToSessionMessageDtos(optimizedHistory);

        _logger.LogInformation(
            "AgentAppService: Da nap optimized history. SessionId={SessionId}, OptimizedHistoryCount={OptimizedHistoryCount}, StepCount={StepCount}.",
            session.Id,
            optimizedHistory.Count,
            steps.Count);

        // 3. Chạy Agent
        _logger.LogInformation(
            "AgentAppService: Bat dau goi _agent.RunAsync. SessionId={SessionId}.",
            session.Id);

        AgentResultDto result = await _agent.RunAsync(new AgentContext { Steps = steps });

        _logger.LogInformation(
            "AgentAppService: _agent.RunAsync hoan tat. SessionId={SessionId}, NewStepCount={NewStepCount}, RequiresApproval={RequiresApproval}, FinalTextLength={FinalTextLength}.",
            session.Id,
            result.NewSteps?.Count ?? 0,
            result.RequiresApproval,
            result.FinalText?.Length ?? 0);

        // 4. Lưu các bước mới phát sinh xuống Database (tự động lọc DLP khi lưu)
        await SaveNewStepsAsync(session, result.NewSteps);

        _logger.LogInformation(
            "AgentAppService: Da luu new steps. SessionId={SessionId}.",
            session.Id);

        // 5. Xử lý các tác vụ chờ (nếu có) và trả về kết quả
        _logger.LogInformation(
            "AgentAppService: Bat dau BuildResultResponseAsync. SessionId={SessionId}.",
            session.Id);

        return await BuildResultResponseAsync(session, result);
    }

    public async Task<object> ApproveAsync(AgentSessionInputDto input)
    {
        // 1. Phục hồi phiên làm việc từ Database
        IQueryable<AgentSession> queryable = await _sessionRepository.GetQueryableAsync();
        AgentSession session = await queryable
            .Include(s => s.Messages)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == input.SessionId)
            ?? throw new UserFriendlyException("Không tìm thấy phiên làm việc.");

        // Tìm AgentTask loại Approval đang Pending của Session này
        AgentTask? pendingTask = await _agentManager.FindPendingTaskAsync(session, AgentTaskType.Approval);

        if (pendingTask == null || string.IsNullOrEmpty(pendingTask.SuspendedDataJson))
        {
            throw new UserFriendlyException("Không tìm thấy tác vụ phê duyệt đang chờ.");
        }

        AgentToolCallMessageDto? pendingToolCall = JsonSerializer.Deserialize<AgentToolCallMessageDto>(pendingTask.SuspendedDataJson, _jsonOptions);
        if (pendingToolCall == null)
        {
            throw new UserFriendlyException("Thông tin tác vụ chờ phê duyệt không hợp lệ.");
        }

        // 2. Gọi tool trên MCP Server (Gọi qua CallToolAsync để tự chuẩn hóa và bắt lỗi)
        string toolResult = await _mcpClientService.CallToolAsync(pendingToolCall.Name, pendingToolCall.Arguments ?? new JsonObject());

        // 2.1 Kiểm tra xem kết quả có chứa yêu cầu Elicitation (Mã lỗi -32042) hay không
        try
        {
            JsonNode? resultNode = JsonNode.Parse(toolResult);
            JsonNode? errorObj = resultNode?["error"];
            if (errorObj != null && errorObj["code"]?.GetValue<int>() == -32042)
            {
                await _agentManager.CompleteTaskAsync(session, pendingTask);
                await _agentManager.CreateTaskAsync(
                    session,
                    AgentTaskType.Elicitation,
                    formJson: errorObj["data"]?["requestedSchema"]?.ToJsonString(_jsonOptions),
                    suspendedDataJson: JsonSerializer.Serialize(pendingToolCall, _jsonOptions)
                );

                return new
                {
                    status = "PendingElicitation",
                    sessionId = session.Id,
                    elicitationForm = errorObj["data"]?["requestedSchema"] != null
                        ? JsonSerializer.Deserialize<JsonObject>(errorObj["data"]!["requestedSchema"]!.ToJsonString(_jsonOptions))
                        : null
                };
            }
        }
        catch
        {
            // Bỏ qua lỗi parsing, tiếp tục xử lý thông thường
        }

        // Hoàn thành tác vụ Approval cũ
        await _agentManager.CompleteTaskAsync(session, pendingTask);

        // 3. Lưu kết quả tool (được tự động chạy DLP tại Domain khi ghi)
        AgentToolResponseMessageDto toolResponse = new() { Name = pendingToolCall.Name, Content = toolResult };
        await _agentManager.AddMessageAsync(
            session,
            "user",
            text: null,
            toolCallsJson: null,
            toolResponsesJson: JsonSerializer.Serialize(new List<AgentToolResponseMessageDto> { toolResponse }, _jsonOptions)
        );

        // 4. Nạp lại lịch sử đã tối ưu và tiếp tục chạy Agent
        List<AgentMessage> optimizedHistory = await _agentManager.GetOptimizedHistoryAsync(session.Id);
        List<AgentSessionMessageDto> steps = MapToSessionMessageDtos(optimizedHistory);

        AgentResultDto result = await _agent.RunAsync(new AgentContext { Steps = steps });

        await SaveNewStepsAsync(session, result.NewSteps);

        return await BuildResultResponseAsync(session, result);
    }

    public async Task<object> RejectAsync(AgentSessionInputDto input)
    {
        // 1. Phục hồi phiên làm việc từ Database
        IQueryable<AgentSession> queryable = await _sessionRepository.GetQueryableAsync();
        AgentSession session = await queryable
            .Include(s => s.Messages)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == input.SessionId)
            ?? throw new UserFriendlyException("Không tìm thấy phiên làm việc.");

        // Tìm AgentTask loại Approval đang Pending
        AgentTask? pendingTask = await _agentManager.FindPendingTaskAsync(session, AgentTaskType.Approval);

        if (pendingTask == null || string.IsNullOrEmpty(pendingTask.SuspendedDataJson))
        {
            throw new UserFriendlyException("Không tìm thấy tác vụ phê duyệt đang chờ.");
        }

        AgentToolCallMessageDto? pendingToolCall = JsonSerializer.Deserialize<AgentToolCallMessageDto>(pendingTask.SuspendedDataJson, _jsonOptions);
        if (pendingToolCall == null)
        {
            throw new UserFriendlyException("Thông tin tác vụ chờ phê duyệt không hợp lệ.");
        }

        // Hủy tác vụ Approval cũ
        await _agentManager.CancelTaskAsync(session, pendingTask);

        // 2. Thêm thông tin từ chối của người dùng vào lịch sử (tự động chạy DLP khi lưu)
        await _agentManager.AddMessageAsync(session, "user", $"User rejected the execution of tool '{pendingToolCall.Name}'.");

        // 3. Nạp lại lịch sử đã tối ưu và tiếp tục chạy Agent
        List<AgentMessage> optimizedHistory = await _agentManager.GetOptimizedHistoryAsync(session.Id);
        List<AgentSessionMessageDto> steps = MapToSessionMessageDtos(optimizedHistory);

        AgentResultDto result = await _agent.RunAsync(new AgentContext { Steps = steps });

        await SaveNewStepsAsync(session, result.NewSteps);

        return await BuildResultResponseAsync(session, result);
    }

    public async Task<AgentHistoryDto> GetHistoryAsync(AgentSessionPagedInputDto input)
    {
        AgentHistoryDto output = new();

        if (input.SessionId == Guid.Empty)
        {
            return output;
        }

        IQueryable<AgentSession> queryable = await _sessionRepository.GetQueryableAsync();
        AgentSession? session = await queryable
            .Include(s => s.Messages)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == input.SessionId);

        if (session != null)
        {
            // Tải phân trang tin nhắn từ CSDL để hiển thị lịch sử UI
            List<AgentMessage> pagedMessages = session.Messages
                .OrderByDescending(m => m.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Reverse()
                .ToList();

            output.Steps = MapToSessionMessageDtos(pagedMessages);

            AgentTask? pendingTask = session.Tasks.FirstOrDefault(t => t.Status == AgentTaskStatus.Pending);
            if (pendingTask != null)
            {
                AgentToolCallMessageDto? suspendedToolCall = !string.IsNullOrEmpty(pendingTask.SuspendedDataJson)
                    ? JsonSerializer.Deserialize<AgentToolCallMessageDto>(pendingTask.SuspendedDataJson, _jsonOptions)
                    : null;

                output.PendingTask = new
                {
                    status = pendingTask.TaskType == AgentTaskType.Approval ? "PendingApproval" : "PendingElicitation",
                    sessionId = pendingTask.SessionId,
                    toolName = suspendedToolCall?.Name,
                    arguments = suspendedToolCall?.Arguments,
                    elicitationForm = pendingTask.FormJson != null
                        ? JsonSerializer.Deserialize<JsonObject>(pendingTask.FormJson)
                        : null
                };
            }
        }

        return output;
    }

    public async Task<object> ResetSessionAsync(AgentSessionInputDto input)
    {
        IQueryable<AgentSession> queryable = await _sessionRepository.GetQueryableAsync();
        AgentSession? session = await queryable
            .FirstOrDefaultAsync(s => s.Id == input.SessionId);

        if (session == null)
        {
            throw new UserFriendlyException("Không tìm thấy phiên hội thoại.");
        }

        // Xóa sạch lịch sử qua AgentManager
        await _agentManager.ClearSessionHistoryAsync(session);

        return new
        {
            status = "Success",
            message = "Đã dọn dẹp toàn bộ cuộc hội thoại cũ."
        };
    }

    public async Task<object> SubmitElicitationAsync(AgentElicitationInputDto input)
    {
        // 1. Phục hồi phiên làm việc từ Database
        IQueryable<AgentSession> queryable = await _sessionRepository.GetQueryableAsync();
        AgentSession session = await queryable
            .Include(s => s.Messages)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == input.SessionId)
            ?? throw new UserFriendlyException("Không tìm thấy phiên làm việc.");

        // Tìm tác vụ Elicitation đang Pending
        AgentTask? pendingTask = await _agentManager.FindPendingTaskAsync(session, AgentTaskType.Elicitation);

        if (pendingTask == null || string.IsNullOrEmpty(pendingTask.SuspendedDataJson))
        {
            throw new UserFriendlyException("Phiên làm việc này không ở trạng thái chờ cung cấp thông tin.");
        }

        AgentToolCallMessageDto? suspendedToolCall = JsonSerializer.Deserialize<AgentToolCallMessageDto>(pendingTask.SuspendedDataJson, _jsonOptions);
        if (suspendedToolCall == null)
        {
            throw new UserFriendlyException("Thông tin tác vụ bị treo không hợp lệ.");
        }

        // 2. Trộn dữ liệu người dùng nộp từ Form vào đối số gọi Tool
        JsonObject arguments = suspendedToolCall.Arguments ?? new JsonObject();
        foreach (KeyValuePair<string, string> item in input.FormValues)
        {
            arguments[item.Key] = JsonValue.Create(item.Value);
        }

        // Hoàn thành tác vụ Elicitation
        await _agentManager.CompleteTaskAsync(session, pendingTask);

        // 3. Gọi Tool trực tiếp qua CallToolAsync để tự chuẩn hóa và bắt lỗi
        string toolResult = await _mcpClientService.CallToolAsync(suspendedToolCall.Name, arguments);

        // 4. Lưu model tool call và kết quả vào lịch sử (tự động chạy DLP khi ghi)
        suspendedToolCall.Arguments = arguments;
        await _agentManager.AddMessageAsync(
            session,
            "model",
            text: null,
            toolCallsJson: JsonSerializer.Serialize(new List<AgentToolCallMessageDto> { suspendedToolCall }, _jsonOptions)
        );

        AgentToolResponseMessageDto toolResponse = new() { Name = suspendedToolCall.Name, Content = toolResult };
        await _agentManager.AddMessageAsync(
            session,
            "user",
            text: null,
            toolCallsJson: null,
            toolResponsesJson: JsonSerializer.Serialize(new List<AgentToolResponseMessageDto> { toolResponse }, _jsonOptions)
        );

        // 5. Tiếp tục chạy Agent với ngữ cảnh mới đã được tối ưu hóa
        List<AgentMessage> optimizedHistory = await _agentManager.GetOptimizedHistoryAsync(session.Id);
        List<AgentSessionMessageDto> steps = MapToSessionMessageDtos(optimizedHistory);

        AgentResultDto result = await _agent.RunAsync(new AgentContext { Steps = steps });

        await SaveNewStepsAsync(session, result.NewSteps);

        return await BuildResultResponseAsync(session, result);
    }

    private List<AgentSessionMessageDto> MapToSessionMessageDtos(ICollection<AgentMessage> messages)
    {
        return messages
            .OrderBy(m => m.CreationTime)
            .Select(m => new AgentSessionMessageDto
            {
                Role = m.Role,
                Text = m.Text,
                ToolCalls = !string.IsNullOrEmpty(m.ToolCallsJson)
                    ? JsonSerializer.Deserialize<List<AgentToolCallMessageDto>>(m.ToolCallsJson, _jsonOptions)
                    : null,
                ToolResponses = !string.IsNullOrEmpty(m.ToolResponsesJson)
                    ? JsonSerializer.Deserialize<List<AgentToolResponseMessageDto>>(m.ToolResponsesJson, _jsonOptions)
                    : null,
                CreationTime = m.CreationTime
            })
            .ToList();
    }

    private async Task SaveNewStepsAsync(AgentSession session, List<AgentSessionMessageDto> newSteps)
    {
        if (newSteps == null)
        {
            return;
        }

        foreach (AgentSessionMessageDto step in newSteps)
        {
            await _agentManager.AddMessageAsync(
                session,
                step.Role,
                step.Text,
                toolCallsJson: step.ToolCalls != null && step.ToolCalls.Any()
                    ? JsonSerializer.Serialize(step.ToolCalls, _jsonOptions)
                    : null,
                toolResponsesJson: step.ToolResponses != null && step.ToolResponses.Any()
                    ? JsonSerializer.Serialize(step.ToolResponses, _jsonOptions)
                    : null
            );
        }
    }

    private async Task<object> BuildResultResponseAsync(AgentSession session, AgentResultDto result)
    {
        if (result.RequiresApproval)
        {
            AgentToolCallMessageDto nextToolCall = new()
            {
                Name = result.PendingToolName!,
                Arguments = result.PendingToolArguments != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                    : new JsonObject(),
                ThoughtSignature = result.NewSteps?.FirstOrDefault(s => s.ToolCalls != null && s.ToolCalls.Any())?.ToolCalls?.FirstOrDefault()?.ThoughtSignature
            };

            await _agentManager.CreateTaskAsync(
                session,
                AgentTaskType.Approval,
                formJson: null,
                suspendedDataJson: JsonSerializer.Serialize(nextToolCall, _jsonOptions)
            );

            return new
            {
                status = "PendingApproval",
                sessionId = session.Id,
                toolName = result.PendingToolName,
                arguments = result.PendingToolArguments != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)
                    : null
            };
        }
        else if (result.RequiresElicitation)
        {
            AgentToolCallMessageDto nextToolCall = new()
            {
                Name = result.PendingToolName!,
                Arguments = result.PendingToolArguments != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                    : new JsonObject(),
                ThoughtSignature = result.NewSteps?.FirstOrDefault(s => s.ToolCalls != null && s.ToolCalls.Any())?.ToolCalls?.FirstOrDefault()?.ThoughtSignature
            };

            await _agentManager.CreateTaskAsync(
                session,
                AgentTaskType.Elicitation,
                formJson: result.ElicitationFormJson,
                suspendedDataJson: JsonSerializer.Serialize(nextToolCall, _jsonOptions)
            );

            return new
            {
                status = "PendingElicitation",
                sessionId = session.Id,
                elicitationForm = result.ElicitationFormJson != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.ElicitationFormJson)
                    : null
            };
        }
        else
        {
            return new AgentResponseOutputDto
            {
                Text = result.FinalText ?? "",
                SessionId = session.Id
            };
        }
    }
}
