using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SupplyCoreERP.Agent.Dtos;
using SupplyCoreERP.Ai;
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

    public AgentAppService(
        IAgent agent,
        IMcpClientService mcpClientService,
        IRepository<AgentSession, Guid> sessionRepository)
    {
        _agent = agent;
        _mcpClientService = mcpClientService;
        _sessionRepository = sessionRepository;
    }

    public async Task<object> SendMessageAsync(AgentRequestInputDto input)
    {
        // 1. Khởi tạo AgentContext từ DB (nếu có SessionId) hoặc input.History
        AgentContext context = new()
        {
            Steps = new List<AgentMessageDto>()
        };

        if (input.SessionId.HasValue && input.SessionId.Value != Guid.Empty)
        {
            AgentSession? session = await _sessionRepository.FindAsync(input.SessionId.Value);
            if (session != null && !string.IsNullOrEmpty(session.ConversationHistoryJson))
            {
                Console.WriteLine($"[AgentAppService] Raw History JSON from DB: {session.ConversationHistoryJson}");
                List<AgentMessageDto>? dbHistory = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson);
                if (dbHistory != null)
                {
                    context.Steps.AddRange(dbHistory);
                }
            }
        }
        else if (input.History != null)
        {
            context.Steps.AddRange(input.History);
        }

        context.Steps.Add(new AgentMessageDto
        {
            Role = "user",
            Text = input.Text
        });

        Console.WriteLine($"[AgentAppService] Steps gửi cho Agent: {JsonSerializer.Serialize(context.Steps)}");

        // 2. Chạy Agent
        AgentResultDto result = await _agent.RunAsync(context);

        // 3. Xử lý kết quả trả về từ Agent và lưu lịch sử vào Database
        if (result.RequiresApproval)
        {
            AgentSession session;
            if (input.SessionId.HasValue && input.SessionId.Value != Guid.Empty)
            {
                // Nếu đã có session, lấy ra và cập nhật
                session = await _sessionRepository.GetAsync(input.SessionId.Value);
                session.IsPendingApproval = true;
                session.PendingToolCallJson = JsonSerializer.Serialize(new AgentToolCallMessageDto
                {
                    Name = result.PendingToolName!,
                    Arguments = result.PendingToolArguments != null
                        ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                        : new JsonObject()
                });
                session.ConversationHistoryJson = JsonSerializer.Serialize(context.Steps);
                await _sessionRepository.UpdateAsync(session);
            }
            else
            {
                // Nếu chưa có session, tạo session mới ở trạng thái chờ duyệt
                session = new(
                    GuidGenerator.Create(),
                    CurrentUser.Id ?? Guid.Empty,
                    JsonSerializer.Serialize(context.Steps)
                )
                {
                    IsPendingApproval = true,
                    PendingToolCallJson = JsonSerializer.Serialize(new AgentToolCallMessageDto
                    {
                        Name = result.PendingToolName!,
                        Arguments = result.PendingToolArguments != null
                            ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                            : new JsonObject()
                    })
                };
                await _sessionRepository.InsertAsync(session);
            }

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
        else
        {
            AgentSession session;
            if (input.SessionId.HasValue && input.SessionId.Value != Guid.Empty)
            {
                // Nếu đã có session, lấy ra và cập nhật
                session = await _sessionRepository.GetAsync(input.SessionId.Value);
                session.IsPendingApproval = false;
                session.PendingToolCallJson = null;
                session.ConversationHistoryJson = JsonSerializer.Serialize(context.Steps);
                await _sessionRepository.UpdateAsync(session);
            }
            else
            {
                // Nếu chưa có session, tạo session mới ở trạng thái bình thường để lưu lịch sử
                session = new(
                    GuidGenerator.Create(),
                    CurrentUser.Id ?? Guid.Empty,
                    JsonSerializer.Serialize(context.Steps)
                )
                {
                    IsPendingApproval = false,
                    PendingToolCallJson = null
                };
                await _sessionRepository.InsertAsync(session);
            }

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
        if (!session.IsPendingApproval || string.IsNullOrEmpty(session.PendingToolCallJson))
        {
            throw new UserFriendlyException("Phiên làm việc này không ở trạng thái chờ phê duyệt tác vụ.");
        }

        AgentToolCallMessageDto? pendingToolCall = JsonSerializer.Deserialize<AgentToolCallMessageDto>(session.PendingToolCallJson);
        if (pendingToolCall == null)
        {
            throw new UserFriendlyException("Thông tin tác vụ chờ phê duyệt không hợp lệ.");
        }

        List<AgentMessageDto> steps = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson)
                    ?? new List<AgentMessageDto>();

        // 2. Gọi tool trên MCP Server
        string toolResult = await _mcpClientService.CallToolAsync(pendingToolCall.Name, pendingToolCall.Arguments);

        // 3. Cập nhật lịch sử cuộc trò chuyện (lượt gọi tool và kết quả của tool)
        steps.Add(new AgentMessageDto
        {
            Role = "model",
            ToolCalls = new List<AgentToolCallMessageDto> { pendingToolCall }
        });

        steps.Add(new AgentMessageDto
        {
            Role = "user",
            ToolResponses = new List<AgentToolResponseMessageDto>
            {
                new() { Name = pendingToolCall.Name, Content = toolResult }
            }
        });

        // 4. Tiếp tục vòng lặp Agent với ngữ cảnh mới
        AgentContext context = new() { Steps = steps };
        AgentResultDto result = await _agent.RunAsync(context);

        // 5. Cập nhật trạng thái session dựa vào kết quả mới
        if (result.RequiresApproval)
        {
            session.IsPendingApproval = true;
            session.PendingToolCallJson = JsonSerializer.Serialize(new AgentToolCallMessageDto
            {
                Name = result.PendingToolName!,
                Arguments = result.PendingToolArguments != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                    : new JsonObject()
            });
            session.ConversationHistoryJson = JsonSerializer.Serialize(context.Steps);
            await _sessionRepository.UpdateAsync(session);

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
        else
        {
            session.IsPendingApproval = false;
            session.PendingToolCallJson = null;
            session.ConversationHistoryJson = JsonSerializer.Serialize(context.Steps);
            await _sessionRepository.UpdateAsync(session);

            return new AgentResponseOutputDto
            {
                Text = result.FinalText ?? ""
            };
        }
    }

    public async Task<object> RejectAsync(AgentSessionInputDto input)
    {
        // 1. Phục hồi phiên làm việc từ Database
        AgentSession session = await _sessionRepository.GetAsync(input.SessionId);
        if (!session.IsPendingApproval || string.IsNullOrEmpty(session.PendingToolCallJson))
        {
            throw new UserFriendlyException("Phiên làm việc này không ở trạng thái chờ phê duyệt tác vụ.");
        }

        AgentToolCallMessageDto? pendingToolCall = JsonSerializer.Deserialize<AgentToolCallMessageDto>(session.PendingToolCallJson);
        if (pendingToolCall == null)
        {
            throw new UserFriendlyException("Thông tin tác vụ chờ phê duyệt không hợp lệ.");
        }

        List<AgentMessageDto> steps = JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson)
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

        // 4. Cập nhật trạng thái session dựa vào kết quả mới
        if (result.RequiresApproval)
        {
            session.IsPendingApproval = true;
            session.PendingToolCallJson = JsonSerializer.Serialize(new AgentToolCallMessageDto
            {
                Name = result.PendingToolName!,
                Arguments = result.PendingToolArguments != null
                    ? JsonSerializer.Deserialize<JsonObject>(result.PendingToolArguments)!
                    : new JsonObject()
            });
            session.ConversationHistoryJson = JsonSerializer.Serialize(context.Steps);
            await _sessionRepository.UpdateAsync(session);

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
        else
        {
            session.IsPendingApproval = false;
            session.PendingToolCallJson = null;
            session.ConversationHistoryJson = JsonSerializer.Serialize(context.Steps);
            await _sessionRepository.UpdateAsync(session);

            return new AgentResponseOutputDto
            {
                Text = result.FinalText ?? ""
            };
        }
    }

    public async Task<List<AgentMessageDto>> GetHistoryAsync(AgentSessionInputDto input)
    {
        AgentSession session = await _sessionRepository.GetAsync(input.SessionId);
        if (string.IsNullOrEmpty(session.ConversationHistoryJson))
        {
            return new List<AgentMessageDto>();
        }

        return JsonSerializer.Deserialize<List<AgentMessageDto>>(session.ConversationHistoryJson)
               ?? new List<AgentMessageDto>();
    }
}
