using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;
using SupplyCoreERP.Mcp.Dtos;
using SupplyCoreERP.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SupplyCoreERP.Mcp.Client.AgentProviders;

[ExposeServices(typeof(IAgentProvider))]
public class GeminiProvider : IAgentProvider, ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly ISettingProvider _settingProvider;

    public GeminiProvider(
        HttpClient httpClient,
        ISettingProvider settingProvider)
    {
        _httpClient = httpClient;
        _settingProvider = settingProvider;
    }

    public async Task<AgentResponseDto> GenerateContentAsync(List<LlmMessageDto> chatHistory, List<McpToolDto> tools, List<McpResourceDto> resources, string? systemInstruction = null)
    {
        string? geminiApiKey = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.LlmProviderApiKey);

        string geminiModel = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.LlmProviderModel) ?? "gemini-2.5-flash";

        if (string.IsNullOrEmpty(geminiApiKey))
        {
            throw new Exception("Chưa cấu hình API Key cho LLM Provider trong phần cài đặt hệ thống!");
        }

        string geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={geminiApiKey}";

        List<object> geminiContents = MapHistoryToGeminiFormat(chatHistory);
        List<object> geminiTools = MapMcpToolsToGemini(tools, resources);

        Dictionary<string, object> geminiPayload = new()
        {
            { "contents", geminiContents }
        };

        if (!string.IsNullOrEmpty(systemInstruction))
        {
            geminiPayload.Add("systemInstruction", new
            {
                parts = new[] { new { text = systemInstruction } }
            });
        }

        if (geminiTools != null && geminiTools.Count > 0)
        {
            geminiPayload.Add("tools", new[] { new { functionDeclarations = geminiTools } });
        }

        // Tắt thinking budget để tránh lỗi thought_signature và cải thiện hiệu năng phản hồi của LLM
        geminiPayload.Add("generationConfig", new
        {
            thinkingConfig = new
            {
                thinkingBudget = 0
            }
        });

        string payloadJson = JsonSerializer.Serialize(geminiPayload);
        Console.WriteLine($"[Gemini-Payload] Gửi lên: {payloadJson}");

        using StringContent requestContent = new(payloadJson, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await _httpClient.PostAsync(geminiUrl, requestContent);

        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Lỗi gọi Gemini API ({response.StatusCode}): {responseContent}");
        }

        return ParseGeminiResponse(responseContent);
    }

    private List<object> MapHistoryToGeminiFormat(List<LlmMessageDto> chatHistory)
    {
        List<object> contents = new();
        bool hasStartedWithUser = false;

        foreach (LlmMessageDto msg in chatHistory)
        {
            if (!hasStartedWithUser && msg.Role != "user")
            {
                continue;
            }
            hasStartedWithUser = true;

            List<object> parts = new();

            if (!string.IsNullOrEmpty(msg.Text))
            {
                parts.Add(new { text = msg.Text });
            }

            if (msg.ToolCalls != null && msg.ToolCalls.Count > 0)
            {
                foreach (LlmToolCallDto toolCall in msg.ToolCalls)
                {
                    if (!string.IsNullOrEmpty(toolCall.ThoughtSignature))
                    {
                        parts.Add(new
                        {
                            functionCall = new
                            {
                                name = toolCall.Name,
                                args = toolCall.Arguments
                            },
                            thought_signature = toolCall.ThoughtSignature
                        });
                    }
                    else
                    {
                        parts.Add(new
                        {
                            functionCall = new
                            {
                                name = toolCall.Name,
                                args = toolCall.Arguments
                            }
                        });
                    }
                }
            }

            if (msg.ToolResponses != null && msg.ToolResponses.Count > 0)
            {
                foreach (LlmToolResponseDto response in msg.ToolResponses)
                {
                    JsonNode? parsedContent = null;
                    try
                    {
                        parsedContent = JsonNode.Parse(response.Content);
                        if (parsedContent is not JsonObject)
                        {
                            parsedContent = new JsonObject { ["result"] = parsedContent };
                        }
                    }
                    catch
                    {
                        parsedContent = new JsonObject { ["result"] = response.Content };
                    }

                    parts.Add(new
                    {
                        functionResponse = new
                        {
                            name = response.Name,
                            response = parsedContent
                        }
                    });
                }
            }

            contents.Add(new
            {
                role = msg.Role,
                parts = parts.ToArray()
            });
        }

        return contents;
    }

    private List<object> MapMcpToolsToGemini(List<McpToolDto> tools, List<McpResourceDto>? resources)
    {
        List<object> list = new();
        if (tools == null)
        {
            return list;
        }

        // Tạo phần mô tả chi tiết cho các tài nguyên có sẵn
        string uriDescription = string.Empty;
        if (resources != null && resources.Count > 0)
        {
            StringBuilder sb = new();
            sb.AppendLine("URI of the resource to read. Available resources on the server:");
            foreach (McpResourceDto r in resources)
            {
                sb.AppendLine($"- {r.Uri}: {r.Description}");
            }
            uriDescription = sb.ToString().TrimEnd();
        }

        foreach (McpToolDto tool in tools)
        {
            JsonObject inputSchema = tool.InputSchema;
            Dictionary<string, object> parameters = new();

            if (inputSchema != null)
            {
                string type = inputSchema["type"]?.ToString()?.ToUpper() ?? "OBJECT";
                parameters.Add("type", type);

                if (inputSchema["properties"] != null)
                {
                    Dictionary<string, object> props = new();
                    foreach (KeyValuePair<string, JsonNode?> prop in inputSchema["properties"].AsObject())
                    {
                        string propName = prop.Key;
                        JsonNode? propVal = prop.Value;
                        string propType = propVal?["type"]?.ToString()?.ToUpper() ?? "STRING";
                        string propDesc = propVal?["description"]?.ToString() ?? "";

                        // Bổ sung các tài nguyên vào mô tả tham số uri của tool read_resource
                        if (tool.Name == "read_resource" && propName == "uri" && !string.IsNullOrEmpty(uriDescription))
                        {
                            propDesc = uriDescription + "\nIf you cannot find a column or table in your previous query results, read the 'schema://database' resource to inspect the full database schema.";
                        }

                        if (propType == "ARRAY")
                        {
                            JsonNode? itemsNode = propVal?["items"];
                            string itemsType = itemsNode?["type"]?.ToString()?.ToUpper() ?? "STRING";

                            props.Add(propName, new
                            {
                                type = propType,
                                description = propDesc,
                                items = new { type = itemsType }
                            });
                        }
                        else
                        {
                            props.Add(propName, new { type = propType, description = propDesc });
                        }
                    }
                    parameters.Add("properties", props);
                }

                if (inputSchema["required"] != null)
                {
                    parameters.Add("required", inputSchema["required"].AsArray().Select(x => x.ToString()).ToList());
                }
            }

            list.Add(new
            {
                name = tool.Name,
                description = tool.Description,
                parameters = parameters
            });
        }

        return list;
    }

    private AgentResponseDto ParseGeminiResponse(string responseContent)
    {
        // Ghi log chi tiết phản hồi từ Gemini để theo dõi cấu trúc thực tế
        Console.WriteLine($"[Gemini-Response] Nhận phản hồi: {responseContent}");

        JsonNode? geminiJson = JsonNode.Parse(responseContent);
        JsonNode? candidate = geminiJson?["candidates"]?[0];
        JsonArray? partsArray = candidate?["content"]?["parts"]?.AsArray();

        AgentResponseDto responseDto = new();

        if (partsArray == null || partsArray.Count == 0)
        {
            responseDto.Text = "Không nhận được phản hồi từ trợ lý AI.";
            return responseDto;
        }

        // Duyệt qua toàn bộ các parts để tìm functionCall và thought_signature
        string? thoughtSignature = null;
        JsonNode? functionCallPart = null;
        StringBuilder textBuilder = new();

        foreach (JsonNode? part in partsArray)
        {
            if (part == null) continue;

            // Kiểm tra và lấy thought_signature/thoughtSignature ở mọi biến thể cấu trúc
            string? sig = part["thought_signature"]?.ToString() 
                       ?? part["thoughtSignature"]?.ToString()
                       ?? part["thought"]?["thought_signature"]?.ToString()
                       ?? part["thought"]?["thoughtSignature"]?.ToString();

            if (!string.IsNullOrEmpty(sig))
            {
                thoughtSignature = sig;
            }

            if (part["functionCall"] != null)
            {
                functionCallPart = part["functionCall"];
            }
            else if (part["text"] != null)
            {
                string? textVal = part["text"]?.ToString();
                if (!string.IsNullOrEmpty(textVal))
                {
                    if (textBuilder.Length > 0) textBuilder.AppendLine();
                    textBuilder.Append(textVal);
                }
            }
        }

        if (functionCallPart != null)
        {
            string? toolName = functionCallPart["name"]?.ToString();
            JsonObject? toolArgs = functionCallPart["args"]?.AsObject();

            if (!string.IsNullOrEmpty(toolName))
            {
                responseDto.ToolCalls.Add(new LlmToolCallDto
                {
                    Name = toolName,
                    Arguments = toolArgs ?? new JsonObject(),
                    ThoughtSignature = thoughtSignature
                });
            }
        }
        else
        {
            responseDto.Text = textBuilder.ToString();
        }

        return responseDto;
    }
}
