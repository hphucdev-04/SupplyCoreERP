using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using SupplyCoreERP.Mcp.Dtos;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Mcp.Client.Mcp;

public class McpClientService : IMcpClientService, ITransientDependency
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public McpClientService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<McpToolDto>> GetToolsAsync()
    {
        string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
        string mcpUrl = $"{mcpBaseUrl}/mcp";

        string requestId = Guid.NewGuid().ToString();
        var payload = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method = "tools/list"
        };

        string responseContent = await SendMcpRequestAsync(mcpUrl, payload);

        JsonNode? jsonNode = JsonNode.Parse(responseContent);
        JsonArray? toolsArray = jsonNode?["result"]?["tools"]?.AsArray();

        List<McpToolDto> mcpTools = new();
        if (toolsArray != null)
        {
            foreach (JsonNode? node in toolsArray)
            {
                if (node == null) continue;

                string name = node["name"]?.ToString() ?? "";
                string desc = node["description"]?.ToString() ?? "";
                JsonObject inputSchema = node["inputSchema"]?.AsObject() ?? new JsonObject();
                bool requiresApproval = node["requiresApproval"]?.GetValue<bool>() ?? false;

                mcpTools.Add(new McpToolDto
                {
                    Name = name,
                    Description = desc,
                    InputSchema = inputSchema,
                    RequiresApproval = requiresApproval
                });
            }
        }

        return mcpTools;
    }

    public async Task<string> CallToolAsync(string toolName, JsonObject arguments)
    {
        string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
        string mcpUrl = $"{mcpBaseUrl}/mcp";

        string requestId = Guid.NewGuid().ToString();
        var payload = new
        {
            jsonrpc = "2.0",
            id = requestId,
            method = "tools/call",
            @params = new
            {
                name = toolName,
                arguments = arguments
            }
        };

        string responseContent = await SendMcpRequestAsync(mcpUrl, payload);

        JsonNode? jsonNode = JsonNode.Parse(responseContent);
        JsonNode? errorNode = jsonNode?["error"];
        if (errorNode != null)
        {
            string errorMessage = errorNode["message"]?.ToString() ?? "Unknown MCP RPC error";
            throw new Exception($"MCP Tool Call Error (RPC): {errorMessage}");
        }

        JsonNode? resultNode = jsonNode?["result"];
        if (resultNode == null)
        {
            throw new Exception("MCP Tool Call Error: Không nhận được 'result' trong phản hồi từ MCP Server.");
        }

        return resultNode.ToJsonString();
    }

    private async Task<string> SendMcpRequestAsync(string url, object payload)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        request.Headers.Accept.Clear();
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Accept.ParseAdd("text/event-stream");

        using HttpResponseMessage response = await _httpClient.SendAsync(request);

        string responseContent = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Lỗi kết nối tới MCP Server ({response.StatusCode}): {responseContent}");
        }

        return responseContent;
    }
}
