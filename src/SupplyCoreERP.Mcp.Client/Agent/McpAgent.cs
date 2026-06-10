using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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

        // 2. Tối ưu hóa lịch sử hội thoại (Sliding Window) để tiết kiệm token gửi lên Gemini
        List<AgentMessageDto> optimizedSteps = OptimizeHistory(context.Steps);

        // 3. Chuyển đổi lịch sử hội thoại đã tối ưu sang cấu trúc tin nhắn LLM nội bộ
        List<AgentChatMessageDto> internalHistory = MapHistoryToAgentFormat(optimizedSteps);

        // 4. Bắt đầu vòng lặp LLM điều phối Tool với chốt an toàn chống vòng lặp vô hạn
        const int MaxAgentIterations = 10;
        int iteration = 0;

        while (iteration < MaxAgentIterations)
        {
            iteration++;

            // Điểm 4.2: Ép LLM tổng hợp câu trả lời ở lượt cuối cùng
            // Nếu đã chạm mốc giới hạn, không cung cấp danh sách tools nữa để LLM buộc phải trả về text dựa trên dữ liệu hiện tại
            List<McpToolDto> activeTools = (iteration == MaxAgentIterations)
                ? new List<McpToolDto>()
                : tools;

            // Gọi LLM sinh nội dung tiếp theo
            AgentResponseDto llmResponse = await _agentProvider.GenerateContentAsync(internalHistory, activeTools);

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
                            // Kiểm tra nếu kết quả là yêu cầu Elicitation từ Server (Form Mode)
                            if (resultObj["elicitation"] != null)
                            {
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
                                    RequiresElicitation = true,
                                    ElicitationFormJson = resultObj["elicitation"]?.ToJsonString(),
                                    PendingToolName = toolCall.Name,
                                    PendingToolArguments = JsonSerializer.Serialize(toolCall.Arguments)
                                };
                            }

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

                            // Điểm 2.3: Nếu là lỗi Tool Execution Error, đánh dấu rõ cho LLM tự sửa
                            if (isToolError)
                            {
                                processedResult = $"[TOOL ERROR] {processedResult}";
                            }
                        }
                    }
                    catch
                    {
                        // Giữ nguyên dữ liệu thô nếu không parse được JSON
                        processedResult = $"[RAW RESULT] {toolResult}";
                    }

                    // Điểm 4.1: Validate Tool Result - Cắt gọn kết quả nếu vượt quá 50KB để bảo vệ context window
                    const int MaxToolResultLength = 50 * 1024;
                    if (processedResult.Length > MaxToolResultLength)
                    {
                        processedResult = processedResult[..MaxToolResultLength] + "\n[TRUNCATED: Result exceeded 50KB limit]";
                    }

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
                            new() { Name = toolCall.Name, Content = processedResult }
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
                            new() { Name = toolCall.Name, Content = processedResult }
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

        // Trường hợp bất khả kháng nếu thoát vòng lặp (về lý thuyết LLM sẽ sinh text ở lượt cuối do không có tools)
        return new AgentResultDto
        {
            FinalText = "Không thể hoàn thành xử lý yêu cầu do đạt giới hạn số lần thực thi.",
            RequiresApproval = false
        };
    }

    private List<AgentMessageDto> OptimizeHistory(List<AgentMessageDto> history, int maxMessages = 12)
    {
        if (history == null || history.Count <= maxMessages)
        {
            return history ?? new List<AgentMessageDto>();
        }

        // Lấy maxMessages tin nhắn gần nhất từ cuối danh sách
        List<AgentMessageDto> optimized = history.Skip(history.Count - maxMessages).ToList();

        // Đảm bảo danh sách tối ưu bắt đầu bằng một tin nhắn text hợp lệ của user (không phải ToolResponse mồ côi và không phải tin nhắn model)
        while (optimized.Count < history.Count && (optimized[0].Role != "user" || string.IsNullOrEmpty(optimized[0].Text)))
        {
            int prevIndex = history.Count - optimized.Count - 1;
            optimized.Insert(0, history[prevIndex]);
        }

        return optimized;
    }

    private List<AgentChatMessageDto> MapHistoryToAgentFormat(List<AgentMessageDto> history)
    {
        List<AgentChatMessageDto> list = new();
        if (history == null)
        {
            return list;
        }

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
