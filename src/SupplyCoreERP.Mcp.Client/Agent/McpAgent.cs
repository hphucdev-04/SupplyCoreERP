using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SupplyCoreERP.Agent;
using SupplyCoreERP.Agent.Dtos;
using SupplyCoreERP.Mcp.Client.AgentProviders;
using SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;
using SupplyCoreERP.Mcp.Dtos;
using SupplyCoreERP.Settings;
using SupplyCoreERP.Settings.Dtos;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SupplyCoreERP.Mcp.Client.Agent;

public class McpAgent : IAgent, ITransientDependency
{
    private readonly IMcpClientService _mcpClientService;
    private readonly IAgentProvider _agentProvider;
    private readonly ISettingProvider _settingProvider;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public McpAgent(
        IMcpClientService mcpClientService,
        IAgentProvider agentProvider,
        ISettingProvider settingProvider)
    {
        _mcpClientService = mcpClientService;
        _agentProvider = agentProvider;
        _settingProvider = settingProvider;
    }

    public async Task<AgentResultDto> RunAsync(AgentContext context)
    {
        List<AgentSessionMessageDto> newSteps = new();

        // Lấy danh sách Tools và Resources hiện có từ MCP Server
        List<McpToolDto> tools = await _mcpClientService.GetToolsAsync();
        List<McpResourceDto> resources = await _mcpClientService.GetResourcesAsync();

        // Định hình khung tư duy Agent (Chỉ dẫn hệ thống nạp động từ MCP Server)
        string systemInstruction = await _mcpClientService.GetServerInstructionsAsync();

        // Tải cấu hình DLP một lần cho toàn bộ vòng lặp
        List<DlpRuleDto> dlpRules = await LoadDlpRulesAsync();

        // Chuyển đổi lịch sử hội thoại đã tối ưu (được nạp trực tiếp từ Domain) sang cấu trúc tin nhắn LLM nội bộ
        List<LlmMessageDto> llmHistory = MapHistoryToLlmFormat(context.Steps);

        // Bắt đầu vòng lặp LLM điều phối Tool với chốt an toàn chống vòng lặp vô hạn
        const int MaxAgentIterations = 10;
        int iteration = 0;
        AgentResponseDto? lastLlmResponse = null;

        while (iteration < MaxAgentIterations)
        {
            iteration++;

            // Ép LLM tổng hợp câu trả lời ở lượt cuối cùng bằng cách ẩn toàn bộ công cụ
            List<McpToolDto> activeTools = (iteration == MaxAgentIterations)
                ? new List<McpToolDto>()
                : tools;

            // Gọi LLM sinh nội dung tiếp theo
            AgentResponseDto llmResponse = await _agentProvider.GenerateContentAsync(llmHistory, activeTools, resources, systemInstruction);
            lastLlmResponse = llmResponse;

            if (llmResponse.IsToolCall)
            {
                LlmToolCallDto toolCall = llmResponse.ToolCalls[0];

                // Tìm schema của Tool tương ứng để kiểm tra cờ requiresApproval
                McpToolDto? toolSchema = tools.Find(t => t.Name == toolCall.Name);
                bool requiresApproval = toolSchema != null && toolSchema.RequiresApproval;

                if (requiresApproval)
                {
                    // TẠM DỪNG: Cần Human-in-the-loop phê duyệt
                    newSteps.Add(new AgentSessionMessageDto
                    {
                        Role = "model",
                        ToolCalls = new List<AgentToolCallMessageDto>
                        {
                            new() { Name = toolCall.Name, Arguments = toolCall.Arguments }
                        },
                        CreationTime = DateTime.Now
                    });

                    return new AgentResultDto
                    {
                        RequiresApproval = true,
                        PendingToolName = toolCall.Name,
                        PendingToolArguments = JsonSerializer.Serialize(toolCall.Arguments),
                        NewSteps = newSteps
                    };
                }
                else
                {
                    // Chạy tự động và nhận kết quả đã được chuẩn hóa (Bắt lỗi, giới hạn nằm trong IMcpClientService)
                    string toolResult = await _mcpClientService.CallToolAsync(toolCall.Name, toolCall.Arguments ?? new JsonObject());

                    // Kiểm tra nếu kết quả là yêu cầu Elicitation (Form Mode)
                    try
                    {
                        JsonNode? resultNode = JsonNode.Parse(toolResult);
                        JsonNode? resultObj = resultNode?["result"];

                        if (resultObj != null && resultObj["elicitation"] != null)
                        {
                            newSteps.Add(new AgentSessionMessageDto
                            {
                                Role = "model",
                                ToolCalls = new List<AgentToolCallMessageDto>
                                {
                                    new() { Name = toolCall.Name, Arguments = toolCall.Arguments }
                                },
                                CreationTime = DateTime.Now
                            });

                            return new AgentResultDto
                            {
                                RequiresElicitation = true,
                                ElicitationFormJson = resultObj["elicitation"]?.ToJsonString(),
                                PendingToolName = toolCall.Name,
                                PendingToolArguments = JsonSerializer.Serialize(toolCall.Arguments),
                                NewSteps = newSteps
                            };
                        }
                    }
                    catch
                    {
                        // Bỏ qua lỗi parse, kết quả bình thường không phải Elicitation
                    }

                    // Làm sạch kết quả từ MCP Server trước khi gửi LLM và lưu lịch sử ===
                    string sanitizedResult = SanitizeText(toolResult, dlpRules);
                    JsonObject? sanitizedArguments = SanitizeArguments(toolCall.Arguments, dlpRules);

                    // Thêm vào lịch sử nội bộ để tiếp tục vòng lặp LLM (dữ liệu đã sạch)
                    llmHistory.Add(new LlmMessageDto
                    {
                        Role = "model",
                        ToolCalls = new List<LlmToolCallDto> { toolCall }
                    });

                    llmHistory.Add(new LlmMessageDto
                    {
                        Role = "user",
                        ToolResponses = new List<LlmToolResponseDto>
                        {
                            new() { Name = toolCall.Name, Content = sanitizedResult }
                        }
                    });

                    // Đồng bộ hóa vào newSteps (dữ liệu đã sạch — để AgentAppService lưu DB)
                    newSteps.Add(new AgentSessionMessageDto
                    {
                        Role = "model",
                        ToolCalls = new List<AgentToolCallMessageDto>
                        {
                            new() { Name = toolCall.Name, Arguments = sanitizedArguments }
                        },
                        CreationTime = DateTime.Now
                    });

                    newSteps.Add(new AgentSessionMessageDto
                    {
                        Role = "user",
                        ToolResponses = new List<AgentToolResponseMessageDto>
                        {
                            new() { Name = toolCall.Name, Content = sanitizedResult }
                        },
                        CreationTime = DateTime.Now
                    });

                    continue;
                }
            }

            // Nếu LLM trả về câu trả lời cuối cùng dạng văn bản
            newSteps.Add(new AgentSessionMessageDto
            {
                Role = "model",
                Text = llmResponse.Text ?? "",
                CreationTime = DateTime.Now
            });

            return new AgentResultDto
            {
                FinalText = llmResponse.Text ?? "",
                RequiresApproval = false,
                NewSteps = newSteps
            };
        }

        // Chốt chặn cú pháp để thỏa mãn compiler (Satisfy Compiler): Trả về câu trả lời cuối cùng thu thập được
        return new AgentResultDto
        {
            FinalText = lastLlmResponse?.Text ?? "Không thể hoàn thành xử lý yêu cầu.",
            RequiresApproval = false,
            NewSteps = newSteps
        };
    }

    private async Task<List<DlpRuleDto>> LoadDlpRulesAsync()
    {
        try
        {
            string? dlpRulesJson = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.DlpRules);
            if (!string.IsNullOrEmpty(dlpRulesJson))
            {
                List<DlpRuleDto>? rules = JsonSerializer.Deserialize<List<DlpRuleDto>>(dlpRulesJson, _jsonOptions);
                if (rules != null && rules.Count > 0)
                {
                    return rules;
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi tải cấu hình, dùng danh sách rỗng (không lọc)
        }

        return new List<DlpRuleDto>();
    }

    private static string SanitizeText(string text, List<DlpRuleDto> rules)
    {
        if (string.IsNullOrEmpty(text) || rules.Count == 0) return text;

        foreach (DlpRuleDto rule in rules)
        {
            if (!string.IsNullOrEmpty(rule.Pattern) && !string.IsNullOrEmpty(rule.Replacement))
            {
                try
                {
                    text = Regex.Replace(text, rule.Pattern, rule.Replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                catch
                {
                    // Bỏ qua regex không hợp lệ
                }
            }
        }

        return text;
    }

    private static JsonObject? SanitizeArguments(JsonObject? arguments, List<DlpRuleDto> rules)
    {
        if (arguments == null || rules.Count == 0) return arguments;

        try
        {
            // Clone để tránh mutation trên object gốc
            JsonObject sanitized = JsonNode.Parse(arguments.ToJsonString())!.AsObject();
            List<string> keys = sanitized.Select(x => x.Key).ToList();
            foreach (string key in keys)
            {
                JsonNode? valNode = sanitized[key];
                if (valNode is JsonValue jsonValue && jsonValue.TryGetValue<string>(out string? originalValue) && originalValue != null)
                {
                    sanitized[key] = JsonValue.Create(SanitizeText(originalValue, rules));
                }
            }
            return sanitized;
        }
        catch
        {
            return arguments;
        }
    }

    private List<LlmMessageDto> MapHistoryToLlmFormat(List<AgentSessionMessageDto> history)
    {
        List<LlmMessageDto> list = new();
        if (history == null)
        {
            return list;
        }

        foreach (AgentSessionMessageDto msg in history)
        {
            LlmMessageDto llmMsg = new()
            {
                Role = msg.Role,
                Text = msg.Text
            };

            if (msg.ToolCalls != null)
            {
                llmMsg.ToolCalls = new List<LlmToolCallDto>();
                foreach (AgentToolCallMessageDto tc in msg.ToolCalls)
                {
                    llmMsg.ToolCalls.Add(new LlmToolCallDto
                    {
                        Name = tc.Name,
                        Arguments = tc.Arguments
                    });
                }
            }

            if (msg.ToolResponses != null)
            {
                llmMsg.ToolResponses = new List<LlmToolResponseDto>();
                foreach (AgentToolResponseMessageDto tr in msg.ToolResponses)
                {
                    llmMsg.ToolResponses.Add(new LlmToolResponseDto
                    {
                        Name = tr.Name,
                        Content = tr.Content
                    });
                }
            }

            list.Add(llmMsg);
        }

        return list;
    }
}
