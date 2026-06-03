using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using SupplyCoreERP.AiChats.Dtos;
using SupplyCoreERP.AiChats.Mcp;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Mcp.Client;

public class McpClientService : IMcpClientService, ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public McpClientService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> ExecuteConversationAsync(string userMessage, List<ChatMessageDto> history)
    {
        string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
        string? geminiApiKey = _configuration["Gemini:ApiKey"];
        string geminiModel = _configuration["Gemini:Model"] ?? "gemini-1.5-pro";

        if (string.IsNullOrEmpty(geminiApiKey))
        {
            throw new Exception("Chưa cấu hình Gemini API Key (Gemini:ApiKey) trong file appsettings.json!");
        }

        // 1. Mở kết nối SSE tới Node.js MCP Server
        HttpRequestMessage sseRequest = new(HttpMethod.Get, $"{mcpBaseUrl}/sse");
        using HttpResponseMessage sseResponse = await _httpClient.SendAsync(sseRequest, HttpCompletionOption.ResponseHeadersRead);
        if (!sseResponse.IsSuccessStatusCode)
        {
            string errorContent = await sseResponse.Content.ReadAsStringAsync();
            throw new Exception($"Lỗi khi kết nối SSE tới MCP Server ({sseResponse.StatusCode}): {errorContent}");
        }

        using Stream stream = await sseResponse.Content.ReadAsStreamAsync();
        using StreamReader reader = new(stream);

        // Đọc SSE Stream để lấy Endpoint gửi message (POST /messages)
        string messageEndpoint = null;
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                messageEndpoint = line.Substring(6).Trim();
                break;
            }
        }

        if (string.IsNullOrEmpty(messageEndpoint))
        {
            throw new Exception("Không thể thiết lập kết nối SSE với MCP Server!");
        }

        string postUrl = messageEndpoint.StartsWith("/")
            ? $"{mcpBaseUrl}{messageEndpoint}"
            : messageEndpoint;

        // 2. Gửi request tools/list để lấy danh sách Tools của MCP Server
        string listToolsRequestId = Guid.NewGuid().ToString();
        var listToolsPayload = new
        {
            jsonrpc = "2.0",
            id = listToolsRequestId,
            method = "tools/list"
        };

        StringContent postContent = new(JsonSerializer.Serialize(listToolsPayload), Encoding.UTF8, "application/json");
        HttpResponseMessage postResponse = await _httpClient.PostAsync(postUrl, postContent);
        if (!postResponse.IsSuccessStatusCode)
        {
            string errorContent = await postResponse.Content.ReadAsStringAsync();
            throw new Exception($"Lỗi gửi request list_tools tới MCP Server ({postResponse.StatusCode}): {errorContent}");
        }

        // Đọc kết quả tools/list được trả về từ luồng SSE
        string toolsListJson = await ReadSseResponseAsync(reader, listToolsRequestId);
        JsonArray? toolsArray = JsonNode.Parse(toolsListJson)?["result"]?["tools"]?.AsArray();

        // Ánh xạ Tools sang định dạng Gemini Schema
        List<object>? geminiTools = MapMcpToolsToGemini(toolsArray);

        // 3. Chuẩn bị lịch sử gửi lên Gemini
        List<object> geminiContents = new();
        bool hasStartedWithUser = false;
        if (history != null)
        {
            foreach (ChatMessageDto msg in history)
            {
                // Gemini bắt buộc hội thoại phải bắt đầu bằng vai trò "user"
                if (!hasStartedWithUser && msg.Role != "user")
                {
                    continue;
                }
                hasStartedWithUser = true;

                geminiContents.Add(new
                {
                    role = msg.Role, // "user" hoặc "model"
                    parts = new[] { new { text = msg.Text } }
                });
            }
        }
        geminiContents.Add(new
        {
            role = "user",
            parts = new[] { new { text = userMessage } }
        });

        // 4. Vòng lặp điều phối cuộc gọi AI (Function Calling Loop)
        string geminiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={geminiApiKey}";

        while (true)
        {
            Dictionary<string, object> geminiPayload = new()
            {
                { "contents", geminiContents }
            };

            if (geminiTools != null && geminiTools.Count > 0)
            {
                geminiPayload.Add("tools", new[] { new { functionDeclarations = geminiTools } });
            }

            StringContent geminiRequest = new(JsonSerializer.Serialize(geminiPayload), Encoding.UTF8, "application/json");
            HttpResponseMessage geminiResponse = await _httpClient.PostAsync(geminiUrl, geminiRequest);
            string responseContent = await geminiResponse.Content.ReadAsStringAsync();
            if (!geminiResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Lỗi gọi Gemini API ({geminiResponse.StatusCode}): {responseContent}");
            }

            JsonNode? geminiJson = JsonNode.Parse(responseContent);
            JsonNode? candidate = geminiJson?["candidates"]?[0];
            string? finishReason = candidate?["finishReason"]?.ToString();
            JsonNode? contentPart = candidate?["content"]?["parts"]?[0];

            // Nếu Gemini yêu cầu gọi Tool
            if (contentPart?["functionCall"] != null)
            {
                JsonNode? functionCall = contentPart["functionCall"];
                string? toolName = functionCall["name"]?.ToString();
                JsonObject? toolArgs = functionCall["args"]?.AsObject();

                // Lưu lịch sử gọi tool
                geminiContents.Add(new
                {
                    role = "model",
                    parts = new[] { new { functionCall = new { name = toolName, args = toolArgs } } }
                });

                // Gọi thực thi Tool sang Node.js MCP Server
                string toolCallRequestId = Guid.NewGuid().ToString();
                var toolCallPayload = new
                {
                    jsonrpc = "2.0",
                    id = toolCallRequestId,
                    method = "tools/call",
                    @params = new
                    {
                        name = toolName,
                        arguments = toolArgs
                    }
                };

                StringContent toolCallRequest = new(JsonSerializer.Serialize(toolCallPayload), Encoding.UTF8, "application/json");
                HttpResponseMessage toolCallResponse = await _httpClient.PostAsync(postUrl, toolCallRequest);
                if (!toolCallResponse.IsSuccessStatusCode)
                {
                    string errorContent = await toolCallResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Lỗi gọi thực thi Tool tới MCP Server ({toolCallResponse.StatusCode}): {errorContent}");
                }

                // Nhận kết quả từ stream SSE
                string toolResultJson = await ReadSseResponseAsync(reader, toolCallRequestId);
                JsonNode? toolResult = JsonNode.Parse(toolResultJson)?["result"];

                // Gửi kết quả Tool về lại cho Gemini
                geminiContents.Add(new
                {
                    role = "user",
                    parts = new[] { new {
                        functionResponse = new {
                            name = toolName,
                            response = new { content = toolResult?["content"] }
                        }
                    } }
                });

                continue;
            }
            else
            {
                string finalAnswer = contentPart?["text"]?.ToString() ?? "Không nhận được phản hồi từ trợ lý AI.";
                return finalAnswer;
            }
        }
    }

    private async Task<string> ReadSseResponseAsync(StreamReader reader, string requestId)
    {
        string line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("data: "))
            {
                string dataContent = line.Substring(6).Trim();
                try
                {
                    JsonNode? jsonNode = JsonNode.Parse(dataContent);
                    string? id = jsonNode?["id"]?.ToString();

                    if (id == requestId)
                    {
                        return dataContent;
                    }
                }
                catch
                {
                    // Bỏ qua dòng JSON không hợp lệ
                }
            }
        }

        throw new Exception($"MCP Server không phản hồi kết quả cho request ID: {requestId}");
    }

    private List<object> MapMcpToolsToGemini(JsonArray toolsArray)
    {
        List<object> list = new();
        if (toolsArray == null) return list;

        foreach (JsonNode? tool in toolsArray)
        {
            string? name = tool?["name"]?.ToString();
            string? desc = tool?["description"]?.ToString();
            JsonNode? inputSchema = tool?["inputSchema"];

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
                name = name,
                description = desc,
                parameters = parameters
            });
        }

        return list;
    }
}
