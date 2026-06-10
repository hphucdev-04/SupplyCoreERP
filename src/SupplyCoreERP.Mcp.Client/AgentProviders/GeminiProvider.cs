using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;
using SupplyCoreERP.Mcp.Dtos;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Mcp.Client.AgentProviders;

[ExposeServices(typeof(IAgentProvider))]
public class GeminiProvider : IAgentProvider, ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<AgentResponseDto> GenerateContentAsync(List<AgentChatMessageDto> chatHistory, List<McpToolDto> tools)
    {
        string? geminiApiKey = _configuration["Gemini:ApiKey"];
        string geminiModel = _configuration["Gemini:Model"] ?? "gemini-1.5-pro";

        if (string.IsNullOrEmpty(geminiApiKey))
        {
            throw new Exception("Chưa cấu hình Gemini API Key (Gemini:ApiKey) trong file appsettings.json!");
        }

        string geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={geminiApiKey}";

        List<object> geminiContents = MapHistoryToGeminiFormat(chatHistory);
        List<object> geminiTools = MapMcpToolsToGemini(tools);

        Dictionary<string, object> geminiPayload = new()
        {
            { "contents", geminiContents }
        };

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

    private List<object> MapHistoryToGeminiFormat(List<AgentChatMessageDto> chatHistory)
    {
        List<object> contents = new();
        bool hasStartedWithUser = false;

        foreach (AgentChatMessageDto msg in chatHistory)
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
                foreach (AgentToolCallDto toolCall in msg.ToolCalls)
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

            if (msg.ToolResponses != null && msg.ToolResponses.Count > 0)
            {
                foreach (AgentToolResponseDto response in msg.ToolResponses)
                {
                    JsonNode? parsedContent = null;
                    try
                    {
                        parsedContent = JsonNode.Parse(response.Content);
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

    private List<object> MapMcpToolsToGemini(List<McpToolDto> tools)
    {
        List<object> list = new();
        if (tools == null)
        {
            return list;
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

                        props.Add(propName, new { type = propType, description = propDesc });
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
        JsonNode? geminiJson = JsonNode.Parse(responseContent);
        JsonNode? candidate = geminiJson?["candidates"]?[0];
        JsonNode? contentPart = candidate?["content"]?["parts"]?[0];

        AgentResponseDto responseDto = new();

        if (contentPart == null)
        {
            responseDto.Text = "Không nhận được phản hồi từ trợ lý AI.";
            return responseDto;
        }

        if (contentPart["functionCall"] != null)
        {
            JsonNode? functionCall = contentPart["functionCall"];
            string? toolName = functionCall["name"]?.ToString();
            JsonObject? toolArgs = functionCall["args"]?.AsObject();

            if (!string.IsNullOrEmpty(toolName))
            {
                responseDto.ToolCalls.Add(new AgentToolCallDto
                {
                    Name = toolName,
                    Arguments = toolArgs ?? new JsonObject()
                });
            }
        }
        else
        {
            responseDto.Text = contentPart["text"]?.ToString() ?? "";
        }

        return responseDto;
    }
}
