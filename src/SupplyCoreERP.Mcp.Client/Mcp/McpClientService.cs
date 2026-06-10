using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SupplyCoreERP.Mcp.Dtos;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Mcp.Client.Mcp;

public class McpClientService : IMcpClientService, ISingletonDependency, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<McpClientService> _logger;

    // Quản lý trạng thái kết nối Streamable HTTP
    private string? _sessionId;
    private bool _isConnected;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    // Cache tĩnh lưu danh sách tools vô hạn
    private static List<McpToolDto>? _cachedTools;
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);
    private CancellationTokenSource? _sseCts;

    public McpClientService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<McpClientService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Thiết lập timeout mặc định cho HttpClient là 30 giây theo đặc tả
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<List<McpToolDto>> GetToolsAsync()
    {
        _logger.LogDebug("GetToolsAsync: Bắt đầu lấy danh sách tools.");
        if (_cachedTools != null)
        {
            _logger.LogDebug("GetToolsAsync: Trả về tools từ Cache.");
            return _cachedTools;
        }

        await _cacheLock.WaitAsync();
        try
        {
            if (_cachedTools != null)
            {
                return _cachedTools;
            }

            string requestId = Guid.NewGuid().ToString();
            var payload = new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "tools/list"
            };

            string responseContent = await SendMcpRequestAsync(payload, requestId);

            JsonNode? jsonNode = JsonNode.Parse(responseContent);
            JsonArray? toolsArray = jsonNode?["result"]?["tools"]?.AsArray();

            List<McpToolDto> mcpTools = new();
            if (toolsArray != null)
            {
                foreach (JsonNode? node in toolsArray)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    string name = node["name"]?.ToString() ?? "";
                    string desc = node["description"]?.ToString() ?? "";
                    JsonObject inputSchema = node["inputSchema"]?.AsObject() ?? new JsonObject();

                    // Xác định cờ duyệt thông qua readOnlyHint trong annotations (chuẩn MCP)
                    bool readOnly = node["annotations"]?["readOnlyHint"]?.GetValue<bool>() ?? true;
                    bool requiresApproval = !readOnly;

                    mcpTools.Add(new McpToolDto
                    {
                        Name = name,
                        Description = desc,
                        InputSchema = inputSchema,
                        RequiresApproval = requiresApproval
                    });
                }
            }

            _cachedTools = mcpTools;
            _logger.LogInformation("GetToolsAsync: Lấy thành công {Count} tools và cập nhật Cache.", mcpTools.Count);
            return mcpTools;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task<string> CallToolAsync(string toolName, JsonObject arguments)
    {
        _logger.LogInformation("CallToolAsync: Bắt đầu gọi tool '{ToolName}'.", toolName);
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

        string responseContent = await SendMcpRequestAsync(payload, requestId);
        _logger.LogInformation("CallToolAsync: Nhận phản hồi cho tool '{ToolName}'.", toolName);
        return responseContent;
    }

    private async Task EnsureConnectedAsync()
    {
        if (_isConnected && !string.IsNullOrEmpty(_sessionId))
        {
            return;
        }

        _logger.LogInformation("EnsureConnectedAsync: Bắt đầu kết nối Stateful Streamable HTTP...");
        await _connectionLock.WaitAsync();
        try
        {
            if (_isConnected && !string.IsNullOrEmpty(_sessionId))
            {
                return;
            }

            string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
            string mcpUrl = $"{mcpBaseUrl.TrimEnd('/')}/mcp";

            // BƯỚC 1: Gửi request POST initialize để bắt đầu handshake
            var initPayload = new
            {
                jsonrpc = "2.0",
                id = "init-1",
                method = "initialize",
                @params = new
                {
                    protocolVersion = "2025-06-18", // Phiên bản giao thức MCP mới nhất
                    capabilities = new
                    {
                        elicitation = new
                        {
                            form = new { }
                        }
                    },
                    clientInfo = new
                    {
                        name = "supplycore-csharp-client",
                        version = "1.0.0"
                    }
                }
            };

            _logger.LogDebug("EnsureConnectedAsync: Gửi POST initialize đến {McpUrl}.", mcpUrl);
            HttpRequestMessage initRequest = new(HttpMethod.Post, mcpUrl);
            initRequest.Content = new StringContent(JsonSerializer.Serialize(initPayload), Encoding.UTF8, "application/json");
            initRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            initRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

            using HttpResponseMessage initResponse = await _httpClient.SendAsync(initRequest);
            _logger.LogInformation("EnsureConnectedAsync: Nhận response initialize. Status: {StatusCode}.", initResponse.StatusCode);

            if (!initResponse.IsSuccessStatusCode)
            {
                string errorContent = await initResponse.Content.ReadAsStringAsync();
                throw new Exception($"Handshake initialize thất bại ({initResponse.StatusCode}): {errorContent}");
            }

            // Trích xuất Session ID từ header phản hồi
            if (initResponse.Headers.TryGetValues("mcp-session-id", out IEnumerable<string>? sessionValues))
            {
                _sessionId = sessionValues.First().Trim();
                _logger.LogInformation("EnsureConnectedAsync: Trích xuất thành công Session ID: '{SessionId}'.", _sessionId);
            }
            else
            {
                throw new Exception("MCP Server không trả về header 'mcp-session-id' trong phản hồi initialize.");
            }

            // Đọc kết quả initialize trả về trực tiếp trong response body
            string initResponseContent = await initResponse.Content.ReadAsStringAsync();
            _logger.LogDebug("EnsureConnectedAsync: Kết quả initialize: {Content}.", initResponseContent);

            // BƯỚC 2: Gửi thông báo initialized (notification) để hoàn tất bắt tay theo đặc tả MCP
            var initializedNotification = new
            {
                jsonrpc = "2.0",
                method = "notifications/initialized"
            };

            _logger.LogInformation("EnsureConnectedAsync: Gửi thông báo initialized với SessionId: '{SessionId}'.", _sessionId);
            using HttpRequestMessage notifyRequest = new(HttpMethod.Post, mcpUrl);
            notifyRequest.Content = new StringContent(JsonSerializer.Serialize(initializedNotification), Encoding.UTF8, "application/json");
            notifyRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            notifyRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
            notifyRequest.Headers.Add("mcp-session-id", _sessionId);
            notifyRequest.Headers.Add("mcp-protocol-version", "2025-06-18");

            using HttpResponseMessage notifyResponse = await _httpClient.SendAsync(notifyRequest);
            _logger.LogInformation("EnsureConnectedAsync: Kết quả gửi initialized: {StatusCode}.", notifyResponse.StatusCode);
            if (!notifyResponse.IsSuccessStatusCode)
            {
                string errContent = await notifyResponse.Content.ReadAsStringAsync();
                throw new Exception($"Gửi thông báo initialized thất bại ({notifyResponse.StatusCode}): {errContent}");
            }

            _isConnected = true;
            _logger.LogInformation("EnsureConnectedAsync: Bắt tay (Handshake) hoàn tất thành công!");

            // Xóa cache tools để đảm bảo tải lại danh sách mới nhất cho session mới
            _cachedTools = null;

            await StartSseListenerAsync(_sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "EnsureConnectedAsync: LỖI THIẾT LẬP KẾT NỐI: {Message}", ex.Message);
            _isConnected = false;
            _sessionId = null;
            throw new Exception($"Lỗi thiết lập kết nối Stateful với MCP Server: {ex.Message}", ex);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task<string> SendMcpRequestAsync(object payload, string requestId)
    {
        _logger.LogDebug("SendMcpRequestAsync: Gửi request ID '{RequestId}'.", requestId);
        await EnsureConnectedAsync();

        try
        {
            string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
            string mcpUrl = $"{mcpBaseUrl.TrimEnd('/')}/mcp";

            using HttpRequestMessage request = new(HttpMethod.Post, mcpUrl);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

            // Đính kèm các header bắt buộc của MCP Stateful HTTP
            request.Headers.Add("mcp-session-id", _sessionId);
            request.Headers.Add("mcp-protocol-version", "2025-06-18");

            _logger.LogDebug("SendMcpRequestAsync: POST gửi lên với SessionId: '{SessionId}'.", _sessionId);

            // Sử dụng CancellationTokenSource để giới hạn thời gian chạy là 30 giây
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cts.Token);

            // Điểm 3.6: Xử lý tường minh HTTP 404 — Auto-Reconnect
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("SendMcpRequestAsync: Nhận HTTP 404 - Session hết hạn hoặc không tồn tại. Đang kết nối lại...");
                _isConnected = false;
                _sessionId = null;

                await EnsureConnectedAsync();

                _logger.LogInformation("SendMcpRequestAsync: Reconnected thành công. Gửi lại request '{RequestId}'...", requestId);
                return await RetrySendAsync(payload, requestId);
            }

            string responseContent = await response.Content.ReadAsStringAsync(cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gửi request POST thất bại ({response.StatusCode}): {responseContent}");
            }

            _logger.LogDebug("SendMcpRequestAsync: Nhận phản hồi trực tiếp cho ID '{RequestId}': {Content}.", requestId, responseContent);

            // Điểm 4.3: SSE Parsing — Xử lý nhiều Events trong Stream
            if (responseContent.StartsWith("event:") || responseContent.Contains("data:"))
            {
                string[] lines = responseContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (line.StartsWith("data:"))
                    {
                        string jsonData = line.Substring(5).Trim();
                        // Trích xuất response khớp với Request ID mong muốn
                        if (jsonData.Contains($"\"id\":\"{requestId}\"") || jsonData.Contains($"\"id\": \"{requestId}\""))
                        {
                            _logger.LogDebug("SendMcpRequestAsync: Trích xuất JSON từ SSE khớp với ID: {Json}.", jsonData);
                            return jsonData;
                        }
                    }
                }
            }

            return responseContent;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning("SendMcpRequestAsync: Request ID '{RequestId}' bị timeout (quá 30 giây).", requestId);
            _isConnected = false;
            _sessionId = null;
            throw new TimeoutException($"MCP request '{requestId}' timed out.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SendMcpRequestAsync: Lỗi gửi request ID '{RequestId}': {Message}.", requestId, ex.Message);
            _isConnected = false;
            _sessionId = null;
            throw;
        }
    }

    private async Task<string> RetrySendAsync(object payload, string requestId)
    {
        string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
        string mcpUrl = $"{mcpBaseUrl.TrimEnd('/')}/mcp";

        using HttpRequestMessage request = new(HttpMethod.Post, mcpUrl);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));

        request.Headers.Add("mcp-session-id", _sessionId);
        request.Headers.Add("mcp-protocol-version", "2025-06-18");

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
        using HttpResponseMessage response = await _httpClient.SendAsync(request, cts.Token);

        string responseContent = await response.Content.ReadAsStringAsync(cts.Token);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gửi lại request POST thất bại sau khi reconnect ({response.StatusCode}): {responseContent}");
        }

        if (responseContent.StartsWith("event:") || responseContent.Contains("data:"))
        {
            string[] lines = responseContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (line.StartsWith("data:"))
                {
                    string jsonData = line.Substring(5).Trim();
                    if (jsonData.Contains($"\"id\":\"{requestId}\"") || jsonData.Contains($"\"id\": \"{requestId}\""))
                    {
                        return jsonData;
                    }
                }
            }
        }

        return responseContent;
    }

    private async Task StartSseListenerAsync(string sessionId)
    {
        _sseCts?.Cancel();
        _sseCts = new CancellationTokenSource();
        CancellationToken token = _sseCts.Token;

        _ = Task.Run(async () =>
        {
            _logger.LogInformation("StartSseListenerAsync: Bắt đầu lắng nghe SSE Stream cho Session: {SessionId}.", sessionId);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
                    string mcpUrl = $"{mcpBaseUrl.TrimEnd('/')}/mcp";

                    using HttpRequestMessage request = new(HttpMethod.Get, mcpUrl);
                    request.Headers.Add("mcp-session-id", sessionId);
                    request.Headers.Add("Accept", "text/event-stream");

                    using HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("StartSseListenerAsync: Kết nối GET SSE thất bại ({StatusCode}). Thử lại sau 5s...", response.StatusCode);
                        await Task.Delay(5000, token);
                        continue;
                    }

                    using Stream stream = await response.Content.ReadAsStreamAsync(token);
                    using StreamReader reader = new(stream);

                    while (!token.IsCancellationRequested)
                    {
                        string? line = await reader.ReadLineAsync(token);
                        if (line == null)
                        {
                            break;
                        }

                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        // Nếu nhận được sự kiện thay đổi danh sách tool
                        if (line.StartsWith("data:") && line.Contains("notifications/tools/list_changed"))
                        {
                            _logger.LogInformation("StartSseListenerAsync: Nhận sự kiện thay đổi tools từ Server! Xóa cache tools...");
                            _cachedTools = null;
                        }
                        // Nếu nhận được sự kiện thay đổi danh sách resources
                        else if (line.StartsWith("data:") && line.Contains("notifications/resources/list_changed"))
                        {
                            _logger.LogInformation("StartSseListenerAsync: Nhận sự kiện thay đổi resources từ Server!");
                            // Nơi thực hiện reset cache resources khi client hỗ trợ caching resources
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("StartSseListenerAsync: Hủy lắng nghe SSE do yêu cầu từ client.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "StartSseListenerAsync: Lỗi khi lắng nghe SSE: {Message}. Thử lại sau 5s...", ex.Message);
                    try { await Task.Delay(5000, token); } catch { break; }
                }
            }
        }, token);
    }

    public void Dispose()
    {
        _logger.LogInformation("McpClientService: Bắt đầu Dispose dọn dẹp tài nguyên.");

        _sseCts?.Cancel();
        _sseCts?.Dispose();
        _connectionLock?.Dispose();
        _cacheLock?.Dispose();

        if (!string.IsNullOrEmpty(_sessionId))
        {
            try
            {
                string mcpBaseUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:3000";
                string mcpUrl = $"{mcpBaseUrl.TrimEnd('/')}/mcp";

                using HttpRequestMessage request = new(HttpMethod.Delete, mcpUrl);
                request.Headers.Add("mcp-session-id", _sessionId);
                request.Headers.Add("mcp-protocol-version", "2025-06-18");

                // Sử dụng Send đồng bộ vì Dispose không async
                using HttpResponseMessage response = _httpClient.Send(request);
                _logger.LogInformation("McpClientService: Đã gửi DELETE session {SessionId} thành công khi shutdown.", _sessionId);
            }
            catch (Exception ex)
            {
                // Tránh ném lỗi trong quá trình Dispose để không làm hỏng tiến trình shutdown
                _logger.LogWarning("McpClientService: Lỗi khi dọn dẹp session (DELETE): {Message}", ex.Message);
            }
        }
    }
}
