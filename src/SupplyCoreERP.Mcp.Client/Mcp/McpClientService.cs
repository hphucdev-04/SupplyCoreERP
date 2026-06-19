using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using SupplyCoreERP.Mcp.Dtos;
using SupplyCoreERP.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SupplyCoreERP.Mcp.Client.Mcp;

public class McpClientService : IMcpClientService, ISingletonDependency, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HttpClient _sseHttpClient; // HttpClient riêng cho SSE — không bị timeout
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger<McpClientService> _logger;
    private string _mcpBaseUrl = string.Empty;

    // Quản lý trạng thái kết nối Streamable HTTP
    private string? _sessionId;
    private bool _isConnected;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    // Cache tĩnh lưu danh sách tools vô hạn
    private static List<McpToolDto>? _cachedTools;
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);
    private CancellationTokenSource? _sseCts;
    private static List<McpResourceDto>? _cachedResources;
    private static readonly SemaphoreSlim _resourceCacheLock = new(1, 1);
    private string? _serverInstructions;

    public McpClientService(
        HttpClient httpClient,
        ISettingProvider settingProvider,
        ILogger<McpClientService> logger)
    {
        _httpClient = httpClient;
        _settingProvider = settingProvider;
        _logger = logger;

        // HttpClient cho JSON-RPC request ngắn (timeout 30 giây)
        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        // HttpClient riêng cho SSE — cần long-lived connection, không được timeout
        _sseHttpClient = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
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

        try
        {
            JsonNode? resultNode = JsonNode.Parse(responseContent);
            JsonNode? resultObj = resultNode?["result"];
            JsonNode? errorObj = resultNode?["error"];

            // 1. Nếu là yêu cầu Elicitation (Form Mode) từ Server
            if (resultObj != null && resultObj["elicitation"] != null)
            {
                return responseContent;
            }

            // 2. Nếu có lỗi giao thức JSON-RPC
            if (errorObj != null)
            {
                if (errorObj["code"]?.GetValue<int>() == -32042)
                {
                    return responseContent;
                }

                return $"[PROTOCOL ERROR] {errorObj["message"]?.ToString() ?? "Unknown protocol error"}";
            }

            // 3. Nếu là kết quả thực thi công cụ bình thường
            if (resultObj != null)
            {
                bool isToolError = resultObj["isError"]?.GetValue<bool>() ?? false;
                JsonArray? contentArray = resultObj["content"]?.AsArray();
                string processedResult;

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
                    return $"[TOOL ERROR] {processedResult}";
                }

                return processedResult;
            }
        }
        catch
        {
            // Bỏ qua lỗi parsing, trả về kết quả thô
        }

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

            _mcpBaseUrl = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.McpServerBaseUrl)
                ?? throw new InvalidOperationException("Chưa cấu hình MCP Server Base URL trong cài đặt hệ thống.");
            string mcpUrl = $"{_mcpBaseUrl.TrimEnd('/')}/mcp";

            // BƯỚC 1: Gửi request POST initialize để bắt đầu handshake
            var initPayload = new
            {
                jsonrpc = "2.0",
                id = "init-1",
                method = "initialize",
                @params = new
                {
                    protocolVersion = "2025-06-18",
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

            try
            {
                JsonNode? initNode = JsonNode.Parse(initResponseContent);
                _serverInstructions = initNode?["result"]?["instructions"]?.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "EnsureConnectedAsync: Lỗi trích xuất instructions từ initialize response.");
            }

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
            string mcpUrl = $"{_mcpBaseUrl.TrimEnd('/')}/mcp";

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
        string mcpUrl = $"{_mcpBaseUrl.TrimEnd('/')}/mcp";

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
                    // Đọc _sessionId động: nếu session đã bị reset ở nơi khác thì dừng loop này
                    string? currentSessionId = _sessionId;
                    if (string.IsNullOrEmpty(currentSessionId))
                    {
                        _logger.LogInformation("StartSseListenerAsync: Session đã bị reset, dừng SSE listener.");
                        break;
                    }

                    string mcpUrl = $"{_mcpBaseUrl.TrimEnd('/')}/mcp";

                    using HttpRequestMessage request = new(HttpMethod.Get, mcpUrl);
                    request.Headers.Add("mcp-session-id", currentSessionId);
                    request.Headers.Add("Accept", "text/event-stream");

                    using HttpResponseMessage response = await _sseHttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("StartSseListenerAsync: Kết nối GET SSE thất bại ({StatusCode}). Reset connection state và dừng.", response.StatusCode);

                        // Session không còn tồn tại trên server (ví dụ server vừa restart)
                        // → Reset để EnsureConnectedAsync sẽ tạo session mới ở request tiếp theo
                        _isConnected = false;
                        _sessionId = null;
                        _cachedTools = null;
                        break;
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
                            _logger.LogInformation("StartSseListenerAsync: Nhận sự kiện thay đổi resources từ Server! Xóa cache resources...");
                            _cachedResources = null;
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

    public async Task<List<McpResourceDto>> GetResourcesAsync()
    {
        _logger.LogDebug("GetResourcesAsync: Bắt đầu lấy danh sách resources.");
        if (_cachedResources != null)
        {
            _logger.LogDebug("GetResourcesAsync: Trả về resources từ Cache.");
            return _cachedResources;
        }

        await _resourceCacheLock.WaitAsync();
        try
        {
            if (_cachedResources != null)
            {
                return _cachedResources;
            }

            string requestId = Guid.NewGuid().ToString();
            var payload = new
            {
                jsonrpc = "2.0",
                id = requestId,
                method = "resources/list"
            };

            string responseContent = await SendMcpRequestAsync(payload, requestId);

            JsonNode? jsonNode = JsonNode.Parse(responseContent);
            JsonArray? resourcesArray = jsonNode?["result"]?["resources"]?.AsArray();

            List<McpResourceDto> mcpResources = new();
            if (resourcesArray != null)
            {
                foreach (JsonNode? node in resourcesArray)
                {
                    if (node == null)
                    {
                        continue;
                    }

                    string uri = node["uri"]?.ToString() ?? "";
                    string name = node["name"]?.ToString() ?? "";
                    string desc = node["description"]?.ToString() ?? "";
                    string mimeType = node["mimeType"]?.ToString() ?? "";

                    mcpResources.Add(new McpResourceDto
                    {
                        Uri = uri,
                        Name = name,
                        Description = desc,
                        MimeType = mimeType
                    });
                }
            }

            _cachedResources = mcpResources;
            _logger.LogInformation("GetResourcesAsync: Lấy thành công {Count} resources và cập nhật Cache.", mcpResources.Count);
            return mcpResources;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetResourcesAsync: Lỗi khi lấy danh sách resources: {Message}", ex.Message);
            return new List<McpResourceDto>();
        }
        finally
        {
            _resourceCacheLock.Release();
        }
    }

    public Task<string> GetServerInstructionsAsync()
    {
        return Task.FromResult(_serverInstructions ?? string.Empty);
    }

    public void Dispose()
    {
        _logger.LogInformation("McpClientService: Bắt đầu Dispose dọn dẹp tài nguyên.");

        _sseCts?.Cancel();
        _sseCts?.Dispose();
        _sseHttpClient.Dispose();
        _connectionLock?.Dispose();
        _cacheLock?.Dispose();

        if (!string.IsNullOrEmpty(_sessionId))
        {
            try
            {
                string mcpUrl = $"{_mcpBaseUrl.TrimEnd('/')}/mcp";

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
