using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SupplyCoreERP.Agent.Dtos;
using SupplyCoreERP.Enums.Agent;
using SupplyCoreERP.Mcp;
using SupplyCoreERP.Settings;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace SupplyCoreERP.Agent;

[Authorize]
public class AgentAppService : SupplyCore, IAgentAppService
{
    private readonly IAgent _agent;
    private readonly IMcpClientService _mcpClientService;
    private readonly IRepository<AgentSession, Guid> _sessionRepository;
    private readonly AgentManager _agentManager;
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public AgentAppService(
        IAgent agent,
        IMcpClientService mcpClientService,
        IRepository<AgentSession, Guid> sessionRepository,
        AgentManager agentManager,
        ISettingProvider settingProvider,
        ISettingManager settingManager)
    {
        _agent = agent;
        _mcpClientService = mcpClientService;
        _sessionRepository = sessionRepository;
        _agentManager = agentManager;
        _settingProvider = settingProvider;
        _settingManager = settingManager;
    }

    public async Task<object> SendMessageAsync(AgentRequestInputDto input)
    {
        // 1. Phục hồi hoặc khởi tạo phiên làm việc thông qua AgentManager
        string defaultHistoryJson = input.History != null
            ? JsonSerializer.Serialize(input.History, _jsonOptions)
            : "[]";

        AgentSession session = await _agentManager.GetOrCreateSessionAsync(
            input.SessionId,
            CurrentUser.Id ?? Guid.Empty,
            defaultHistoryJson
        );

        AgentContext context = new()
        {
            Steps = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson, _jsonOptions)
                    ?? new List<AgentMessageDto>()
        };

        context.Steps.Add(new AgentMessageDto
        {
            Role = "user",
            Text = input.Text
        });

        // 2. Chạy Agent
        AgentResultDto result = await _agent.RunAsync(context);

        // 3. Cập nhật lịch sử cuộc trò chuyện vào session
        await _agentManager.UpdateSessionHistoryAsync(session, JsonSerializer.Serialize(context.Steps, _jsonOptions));

        // 4. Xử lý kết quả trả về từ Agent
        if (result.RequiresApproval)
        {
            AgentToolCallMessageDto pendingToolCall = new()
            {
                Name = result.PendingToolName!,
                Arguments = result.PendingToolArguments != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                    : new JsonObject()
            };

            // Tạo AgentTask loại Approval ở trạng thái Pending thông qua AgentManager
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Approval,
                formJson: null,
                suspendedDataJson: JsonSerializer.Serialize(pendingToolCall, _jsonOptions)
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
            AgentToolCallMessageDto pendingToolCall = new()
            {
                Name = result.PendingToolName!,
                Arguments = result.PendingToolArguments != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                    : new JsonObject()
            };

            // Tạo AgentTask loại Elicitation ở trạng thái Pending thông qua AgentManager
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Elicitation,
                formJson: result.ElicitationFormJson,
                suspendedDataJson: JsonSerializer.Serialize(pendingToolCall, _jsonOptions)
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

    public async Task<object> ApproveAsync(AgentSessionInputDto input)
    {
        // 1. Phục hồi phiên làm việc từ Database
        AgentSession session = await _sessionRepository.GetAsync(input.SessionId);

        // Tìm AgentTask loại Approval đang Pending của Session này
        AgentTask? pendingTask = await _agentManager.FindPendingTaskAsync(input.SessionId, AgentTaskType.Approval);

        if (pendingTask == null || string.IsNullOrEmpty(pendingTask.SuspendedDataJson))
        {
            throw new UserFriendlyException("Không tìm thấy tác vụ phê duyệt đang chờ.");
        }

        AgentToolCallMessageDto? pendingToolCall = JsonSerializer.Deserialize<AgentToolCallMessageDto>(pendingTask.SuspendedDataJson, _jsonOptions);
        if (pendingToolCall == null)
        {
            throw new UserFriendlyException("Thông tin tác vụ chờ phê duyệt không hợp lệ.");
        }

        List<AgentMessageDto> steps = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson, _jsonOptions)
                    ?? new List<AgentMessageDto>();

        // 2. Gọi tool trên MCP Server
        string toolResult = await _mcpClientService.CallToolAsync(pendingToolCall.Name, pendingToolCall.Arguments);

        // 2.1 Kiểm tra xem kết quả có chứa yêu cầu Elicitation (Mã lỗi -32042) hay không
        try
        {
            JsonNode? resultNode = JsonNode.Parse(toolResult);
            JsonNode? errorObj = resultNode?["error"];
            if (errorObj != null && errorObj["code"]?.GetValue<int>() == -32042)
            {
                // Hoàn thành tác vụ Approval cũ
                await _agentManager.CompleteTaskAsync(pendingTask);

                // Tạo một tác vụ Elicitation mới đang chờ
                await _agentManager.CreateTaskAsync(
                    session.Id,
                    AgentTaskType.Elicitation,
                    formJson: errorObj["data"]?["requestedSchema"]?.ToJsonString(_jsonOptions),
                    suspendedDataJson: JsonSerializer.Serialize(pendingToolCall, _jsonOptions)
                );

                // Cập nhật lại lịch sử chat
                await _agentManager.UpdateSessionHistoryAsync(session, JsonSerializer.Serialize(steps, _jsonOptions));

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
        await _agentManager.CompleteTaskAsync(pendingTask);

        // Khử nhạy cảm thông tin đối số trước khi lưu vào lịch sử DB để bảo vệ an toàn thông tin (DLP)
        if (pendingToolCall.Arguments != null)
        {
            try
            {
                List<string> keys = pendingToolCall.Arguments.Select(x => x.Key).ToList();
                foreach (string key in keys)
                {
                    JsonNode? valNode = pendingToolCall.Arguments[key];
                    if (valNode != null)
                    {
                        if (valNode is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string? originalValue) && originalValue != null)
                        {
                            string redactedValue = await RedactSensitiveTextAsync(originalValue);
                            pendingToolCall.Arguments[key] = JsonValue.Create(redactedValue);
                        }
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi làm sạch
            }
        }

        // 3. Cập nhật lịch sử cuộc trò chuyện (lượt gọi tool và kết quả của tool)
        steps.Add(new AgentMessageDto
        {
            Role = "model",
            ToolCalls = new List<AgentToolCallMessageDto> { pendingToolCall }
        });

        string redactedToolResult = await RedactSensitiveTextAsync(toolResult);

        steps.Add(new AgentMessageDto
        {
            Role = "user",
            ToolResponses = new List<AgentToolResponseMessageDto>
            {
                new() { Name = pendingToolCall.Name, Content = redactedToolResult }
            }
        });

        // 4. Tiếp tục vòng lặp Agent với ngữ cảnh mới
        AgentContext context = new() { Steps = steps };
        AgentResultDto result = await _agent.RunAsync(context);

        // 5. Lưu lại lịch sử và tạo tác vụ mới (nếu có)
        await _agentManager.UpdateSessionHistoryAsync(session, JsonSerializer.Serialize(context.Steps, _jsonOptions));

        if (result.RequiresApproval)
        {
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Approval,
                formJson: null,
                suspendedDataJson: JsonSerializer.Serialize(new AgentToolCallMessageDto
                {
                    Name = result.PendingToolName!,
                    Arguments = result.PendingToolArguments != null
                        ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                        : new JsonObject()
                }, _jsonOptions)
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
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Elicitation,
                formJson: result.ElicitationFormJson,
                suspendedDataJson: JsonSerializer.Serialize(new AgentToolCallMessageDto
                {
                    Name = result.PendingToolName!,
                    Arguments = result.PendingToolArguments != null
                        ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                        : new JsonObject()
                }, _jsonOptions)
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

    public async Task<object> RejectAsync(AgentSessionInputDto input)
    {
        // 1. Phục hồi phiên làm việc từ Database
        AgentSession session = await _sessionRepository.GetAsync(input.SessionId);

        // Tìm AgentTask loại Approval đang Pending
        AgentTask? pendingTask = await _agentManager.FindPendingTaskAsync(input.SessionId, AgentTaskType.Approval);

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
        await _agentManager.CancelTaskAsync(pendingTask);

        List<AgentMessageDto> steps = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson, _jsonOptions)
                    ?? new List<AgentMessageDto>();

        // 2. Thêm thông tin từ chối của người dùng vào lịch sử
        steps.Add(new AgentMessageDto
        {
            Role = "model",
            ToolCalls = new List<AgentToolCallMessageDto> { pendingToolCall }
        });

        steps.Add(new AgentMessageDto
        {
            Role = "user",
            Text = $"User rejected the execution of tool '{pendingToolCall.Name}'."
        });

        // 3. Tiếp tục vòng lặp Agent với ngữ cảnh mới
        AgentContext context = new() { Steps = steps };
        AgentResultDto result = await _agent.RunAsync(context);

        // 4. Lưu lại lịch sử và tạo tác vụ mới (nếu có)
        await _agentManager.UpdateSessionHistoryAsync(session, JsonSerializer.Serialize(context.Steps, _jsonOptions));

        if (result.RequiresApproval)
        {
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Approval,
                formJson: null,
                suspendedDataJson: JsonSerializer.Serialize(new AgentToolCallMessageDto
                {
                    Name = result.PendingToolName!,
                    Arguments = result.PendingToolArguments != null
                        ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                        : new JsonObject()
                }, _jsonOptions)
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
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Elicitation,
                formJson: result.ElicitationFormJson,
                suspendedDataJson: JsonSerializer.Serialize(new AgentToolCallMessageDto
                {
                    Name = result.PendingToolName!,
                    Arguments = result.PendingToolArguments != null
                        ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                        : new JsonObject()
                }, _jsonOptions)
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

    public async Task<AgentHistoryDto> GetHistoryAsync(AgentSessionInputDto input)
    {
        AgentHistoryDto output = new();

        if (input.SessionId == Guid.Empty)
        {
            return output;
        }

        // A. Lấy lịch sử cuộc trò chuyện từ DB
        AgentSession? session = await _sessionRepository.FindAsync(input.SessionId);
        if (session != null && !string.IsNullOrEmpty(session.ConversationHistoryJson))
        {
            output.Steps = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson, _jsonOptions)
                           ?? new List<AgentMessageDto>();
        }

        // B. Tìm và tích hợp tác vụ pending (nếu có) để khôi phục trạng thái
        AgentTask? pendingTask = await _agentManager.FindPendingTaskAsync(input.SessionId, AgentTaskType.Approval);
        if (pendingTask == null)
        {
            pendingTask = await _agentManager.FindPendingTaskAsync(input.SessionId, AgentTaskType.Elicitation);
        }

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

        return output;
    }

    public async Task<object> SubmitElicitationAsync(AgentElicitationInputDto input)
    {
        // 1. Phục hồi phiên làm việc từ Database
        AgentSession session = await _sessionRepository.GetAsync(input.SessionId);

        // Tìm tác vụ Elicitation đang Pending
        AgentTask? pendingTask = await _agentManager.FindPendingTaskAsync(input.SessionId, AgentTaskType.Elicitation);

        if (pendingTask == null || string.IsNullOrEmpty(pendingTask.SuspendedDataJson))
        {
            throw new UserFriendlyException("Phiên làm việc này không ở trạng thái chờ cung cấp thông tin.");
        }

        AgentToolCallMessageDto? suspendedToolCall = JsonSerializer.Deserialize<AgentToolCallMessageDto>(pendingTask.SuspendedDataJson, _jsonOptions);
        if (suspendedToolCall == null)
        {
            throw new UserFriendlyException("Thông tin tác vụ bị treo không hợp lệ.");
        }

        List<AgentMessageDto> steps = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson, _jsonOptions)
                    ?? new List<AgentMessageDto>();

        // 2. Trộn dữ liệu người dùng nộp từ Form vào đối số gọi Tool
        JsonObject arguments = suspendedToolCall.Arguments ?? new JsonObject();
        foreach (KeyValuePair<string, string> item in input.FormValues)
        {
            arguments[item.Key] = JsonValue.Create(item.Value);
        }

        // Hoàn thành tác vụ Elicitation
        await _agentManager.CompleteTaskAsync(pendingTask);

        // 3. Gọi Tool trực tiếp trên MCP Server với đối số đầy đủ
        string toolResult = await _mcpClientService.CallToolAsync(suspendedToolCall.Name, arguments);

        // Phân biệt Protocol Error vs Tool Execution Error và trích xuất nội dung
        string processedResult = toolResult;
        try
        {
            JsonNode? resultNode = JsonNode.Parse(toolResult);
            JsonNode? resultObj = resultNode?["result"];
            JsonNode? errorObj = resultNode?["error"];

            if (errorObj != null)
            {
                processedResult = $"[PROTOCOL ERROR] {errorObj["message"]?.ToString() ?? "Unknown protocol error"}";
            }
            else if (resultObj != null)
            {
                bool isToolError = resultObj["isError"]?.GetValue<bool>() ?? false;
                JsonArray? contentArray = resultObj["content"]?.AsArray();

                if (contentArray != null && contentArray.Count > 0)
                {
                    IEnumerable<string?> texts = contentArray
                        .Select(c => c?["text"]?.ToString())
                        .Where(t => !string.IsNullOrEmpty(t));
                    processedResult = string.Join("\n", texts);
                }
                else
                {
                    processedResult = resultObj.ToString();
                }

                if (isToolError)
                {
                    processedResult = $"[TOOL ERROR] {processedResult}";
                }
            }
        }
        catch
        {
            processedResult = $"[RAW RESULT] {toolResult}";
        }

        const int MaxToolResultLength = 50 * 1024;
        if (processedResult.Length > MaxToolResultLength)
        {
            processedResult = processedResult[..MaxToolResultLength] + "\n[TRUNCATED: Result exceeded 50KB limit]";
        }

        // 4. Khử nhạy cảm thông tin đối số trước khi lưu vào lịch sử DB để bảo vệ an toàn thông tin (DLP)
        try
        {
            List<string> keys = arguments.Select(x => x.Key).ToList();
            foreach (string key in keys)
            {
                JsonNode? valNode = arguments[key];
                if (valNode != null)
                {
                    if (valNode is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string? originalValue) && originalValue != null)
                    {
                        string redactedValue = await RedactSensitiveTextAsync(originalValue);
                        arguments[key] = JsonValue.Create(redactedValue);
                    }
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi làm sạch
        }

        steps.Add(new AgentMessageDto
        {
            Role = "model",
            ToolCalls = new List<AgentToolCallMessageDto>
            {
                new() { Name = suspendedToolCall.Name, Arguments = arguments }
            }
        });

        string redactedProcessedResult = await RedactSensitiveTextAsync(processedResult);

        steps.Add(new AgentMessageDto
        {
            Role = "user",
            ToolResponses = new List<AgentToolResponseMessageDto>
            {
                new() { Name = suspendedToolCall.Name, Content = redactedProcessedResult }
            }
        });

        // 5. Tiếp tục chạy Agent với ngữ cảnh mới
        AgentContext context = new() { Steps = steps };
        AgentResultDto result = await _agent.RunAsync(context);

        // 6. Cập nhật lại session theo kết quả chạy tiếp theo của Agent
        await _agentManager.UpdateSessionHistoryAsync(session, JsonSerializer.Serialize(context.Steps, _jsonOptions));

        if (result.RequiresApproval)
        {
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Approval,
                formJson: null,
                suspendedDataJson: JsonSerializer.Serialize(new AgentToolCallMessageDto
                {
                    Name = result.PendingToolName!,
                    Arguments = result.PendingToolArguments != null
                        ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                        : new JsonObject()
                }, _jsonOptions)
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
            await _agentManager.CreateTaskAsync(
                session.Id,
                AgentTaskType.Elicitation,
                formJson: result.ElicitationFormJson,
                suspendedDataJson: JsonSerializer.Serialize(new AgentToolCallMessageDto
                {
                    Name = result.PendingToolName!,
                    Arguments = result.PendingToolArguments != null
                        ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                        : new JsonObject()
                }, _jsonOptions)
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

    public async Task<string> GetDlpRulesJsonAsync()
    {
        return await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.DlpRules) ?? "[]";
    }

    public async Task UpdateDlpRulesJsonAsync(string dlpRulesJson)
    {
        try
        {
            JsonSerializer.Deserialize<List<DlpRule>>(dlpRulesJson, _jsonOptions);
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException("Định dạng JSON của DLP Rules không hợp lệ: " + ex.Message);
        }

        await _settingManager.SetGlobalAsync(SupplyCoreERPSettings.DlpRules, dlpRulesJson);
    }

    private async Task<string> RedactSensitiveTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        try
        {
            string? dlpRulesJson = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.DlpRules);
            if (string.IsNullOrEmpty(dlpRulesJson))
            {
                return text;
            }

            List<DlpRule>? rules = JsonSerializer.Deserialize<List<DlpRule>>(dlpRulesJson, _jsonOptions);
            if (rules == null || !rules.Any())
            {
                return text;
            }

            foreach (DlpRule rule in rules)
            {
                if (!string.IsNullOrEmpty(rule.Pattern) && !string.IsNullOrEmpty(rule.Replacement))
                {
                    text = Regex.Replace(text, rule.Pattern, rule.Replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi trong quá trình redact
        }

        return text;
    }

}

public class DlpRule
{
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Replacement { get; set; } = string.Empty;
}
