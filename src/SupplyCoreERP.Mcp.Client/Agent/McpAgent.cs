using System.Text.Json;
using SupplyCoreERP.Agent;
using SupplyCoreERP.Agent.Dtos;
using SupplyCoreERP.Mcp.Client.AgentProviders;
using SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;
using SupplyCoreERP.Mcp.Dtos;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Mcp.Client.Agent;

public class McpAgent : IAgent, ITransientDependency
{
    private readonly IMcpClientService _mcpClientService;
    private readonly IAgentProvider _agentProvider;

    public McpAgent(
        IMcpClientService mcpClientService,
        IAgentProvider agentProvider)
    {
        _mcpClientService = mcpClientService;
        _agentProvider = agentProvider;
    }

    public async Task<AgentResultDto> RunAsync(AgentContext context)
    {
        // 1. Lấy danh sách Tools hiện có từ MCP Server
        List<McpToolDto> tools = await _mcpClientService.GetToolsAsync();

        // 2. Chuyển đổi lịch sử hội thoại thô sang cấu trúc tin nhắn LLM nội bộ
        List<AgentChatMessageDto> internalHistory = MapHistoryToAgentFormat(context.Steps);

        // 3. Bắt đầu vòng lặp LLM điều phối Tool
        while (true)
        {
            // Gọi LLM sinh nội dung tiếp theo
            AgentResponseDto llmResponse = await _agentProvider.GenerateContentAsync(internalHistory, tools);

            if (llmResponse.IsToolCall)
            {
                AgentToolCallDto toolCall = llmResponse.ToolCalls[0]; // Xử lý tuần tự tool call đầu tiên

                // Tìm schema của Tool tương ứng để kiểm tra cờ requiresApproval
                McpToolDto toolSchema = tools.Find(t => t.Name == toolCall.Name);
                bool requiresApproval = toolSchema != null && toolSchema.RequiresApproval;

                if (requiresApproval)
                {
                    // TẠM DỪNG: Cần Human-in-the-loop phê duyệt
                    // Đồng bộ thông tin tool call này vào context.Steps để Application Layer lưu trữ
                    context.Steps.Add(new AgentMessageDto
                    {
                        Role = "model",
                        ToolCalls = new List<AgentToolCallMessageDto>
                        {
                            new() { Name = toolCall.Name, Arguments = toolCall.Arguments }
                        }
                    });

                    return new AgentResultDto
                    {
                        RequiresApproval = true,
                        PendingToolName = toolCall.Name,
                        PendingToolArguments = JsonSerializer.Serialize(toolCall.Arguments)
                    };
                }
                else
                {
                    // Chạy tự động: Không cần phê duyệt
                    string toolResult = await _mcpClientService.CallToolAsync(toolCall.Name, toolCall.Arguments);

                    // Thêm tool call và tool response vào lịch sử nội bộ để tiếp tục vòng lặp
                    internalHistory.Add(new AgentChatMessageDto
                    {
                        Role = "model",
                        ToolCalls = new List<AgentToolCallDto> { toolCall }
                    });

                    internalHistory.Add(new AgentChatMessageDto
                    {
                        Role = "user",
                        ToolResponses = new List<AgentToolResponseDto>
                        {
                            new() { Name = toolCall.Name, Content = toolResult }
                        }
                    });

                    // Đồng bộ hóa vào context.Steps
                    context.Steps.Add(new AgentMessageDto
                    {
                        Role = "model",
                        ToolCalls = new List<AgentToolCallMessageDto>
                        {
                            new() { Name = toolCall.Name, Arguments = toolCall.Arguments }
                        }
                    });

                    context.Steps.Add(new AgentMessageDto
                    {
                        Role = "user",
                        ToolResponses = new List<AgentToolResponseMessageDto>
                        {
                            new() { Name = toolCall.Name, Content = toolResult }
                        }
                    });

                    continue;
                }
            }

            // Nếu LLM trả về câu trả lời cuối cùng dạng văn bản
            context.Steps.Add(new AgentMessageDto
            {
                Role = "model",
                Text = llmResponse.Text ?? ""
            });

            return new AgentResultDto
            {
                FinalText = llmResponse.Text ?? "",
                RequiresApproval = false
            };
        }
    }

    private List<AgentChatMessageDto> MapHistoryToAgentFormat(List<AgentMessageDto> history)
    {
        List<AgentChatMessageDto> list = new();
        if (history == null) return list;

        foreach (AgentMessageDto msg in history)
        {
            AgentChatMessageDto chatMsg = new()
            {
                Role = msg.Role,
                Text = msg.Text
            };

            if (msg.ToolCalls != null)
            {
                foreach (AgentToolCallMessageDto tc in msg.ToolCalls)
                {
                    chatMsg.ToolCalls.Add(new AgentToolCallDto
                    {
                        Name = tc.Name,
                        Arguments = tc.Arguments
                    });
                }
            }

            if (msg.ToolResponses != null)
            {
                foreach (AgentToolResponseMessageDto tr in msg.ToolResponses)
                {
                    chatMsg.ToolResponses.Add(new AgentToolResponseDto
                    {
                        Name = tr.Name,
                        Content = tr.Content
                    });
                }
            }

            list.Add(chatMsg);
        }

        return list;
    }
}
