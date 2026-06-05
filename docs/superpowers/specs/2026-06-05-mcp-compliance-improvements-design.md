# Tài liệu Đặc tả Cải thiện MCP Compliance
## SupplyCoreERP — Nâng cấp C# Client & Node.js Server theo Đặc tả MCP Chính thức

Tài liệu này đặc tả chi tiết 15 điểm cải thiện được xác định qua đối chiếu mã nguồn hiện tại của hệ thống SupplyCoreERP với tài liệu đặc tả chính thức của Model Context Protocol (MCP) tại [modelcontextprotocol.io](https://modelcontextprotocol.io).

---

## 1. Phạm vi Ảnh hưởng

| Thành phần | Số điểm cải thiện | Mức MUST | Mức SHOULD |
| :--- | :---: | :---: | :---: |
| Node.js MCP Server (`mcp-server/src/`) | 7 | 2 | 5 |
| C# MCP Client (`SupplyCoreERP.Mcp.Client/`) | 11 | 2 | 9 |
| Liên thông cả hai (Protocol Version, isError) | 2 | 2 | 0 |

---

## 2. Nhóm 1 — Mức MUST (Bắt buộc sửa)

### 2.1 Origin Header Validation (Server)

**Tài liệu tham chiếu**: `transports.md` — Server **MUST** validate `Origin` header trên TẤT CẢ request HTTP. Nếu `Origin` tồn tại nhưng không hợp lệ → **MUST** trả về HTTP 403 Forbidden.

**Hiện trạng**: [index.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/index.ts) không có bất kỳ logic kiểm tra `Origin` nào.

**Thiết kế giải pháp**:
- Thêm Express middleware đặt **trước** tất cả các route handler.
- Đọc whitelist `ALLOWED_ORIGINS` từ biến môi trường `.env` (mặc định: `http://localhost:4200,http://localhost:3000`).
- Logic: Nếu request có header `Origin` và giá trị KHÔNG nằm trong whitelist → trả HTTP 403 ngay lập tức.
- Nếu request không có header `Origin` (ví dụ: cURL, Postman, STDIO) → cho phép đi qua.

**File thay đổi**:
- `mcp-server/src/index.ts` — Thêm middleware `validateOrigin`.
- `mcp-server/.env` — Thêm biến `ALLOWED_ORIGINS`.

**Mã nguồn minh họa**:
```typescript
// Middleware kiểm tra Origin Header (DNS Rebinding Protection)
app.use((req: Request, res: Response, next: NextFunction) => {
  const origin = req.headers.origin;
  if (origin) {
    const allowedOrigins = (process.env.ALLOWED_ORIGINS || "").split(",").map(o => o.trim());
    if (!allowedOrigins.includes(origin)) {
      res.status(403).json({ error: "Forbidden: Invalid Origin" });
      return;
    }
  }
  next();
});
```

---

### 2.2 Nâng cấp Protocol Version (Cả hai)

**Tài liệu tham chiếu**: `lifecycle.md` — Client **SHOULD** gửi phiên bản mới nhất mà mình hỗ trợ. Phiên bản mới nhất tại thời điểm viết: `2025-06-18`.

**Hiện trạng**: Cả C# Client và MCP Server đều sử dụng `protocolVersion: "2024-11-05"` — phiên bản cũ của giao thức SSE+HTTP đã bị thay thế.

**Thiết kế giải pháp**:
- Thay đổi giá trị `protocolVersion` từ `"2024-11-05"` thành `"2025-06-18"` ở cả hai phía.
- C# Client: Thay đổi tại `EnsureConnectedAsync` (dòng 156) và `SendMcpRequestAsync` (dòng 255).
- Xác nhận MCP SDK v2 phía Server tự động sử dụng phiên bản mới nhất hoặc cấu hình tường minh trong `McpServer` constructor.

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` — 2 vị trí thay chuỗi version.
- `mcp-server/src/index.ts` — Xác nhận/cấu hình version trong `McpServer`.

---

### 2.3 Phân biệt Protocol Error / Tool Execution Error (Cả hai)

**Tài liệu tham chiếu**: `tools.md` — Hai cơ chế lỗi riêng biệt:
1. **Protocol Error**: JSON-RPC `error` (unknown tool, malformed request, server error).
2. **Tool Execution Error**: `isError: true` trong `result` (API failures, validation, business logic). Client **SHOULD** cung cấp lỗi tool cho LLM để tự sửa.

**Hiện trạng**:
- **Server**: Khi tool query DB thất bại, lỗi bị ném ra dưới dạng exception Node.js — không trả về `isError: true`.
- **Client**: `CallToolAsync` trả về chuỗi JSON thô, không kiểm tra `isError`.

**Thiết kế giải pháp — Server**:
Bọc logic thực thi của từng tool trong `try/catch`. Khi gặp lỗi nghiệp vụ:
```typescript
// Trong mỗi tool handler
try {
  const rows = await queryDb(sql, params);
  return { content: [{ type: "text", text: JSON.stringify(rows) }] };
} catch (err) {
  return {
    content: [{ type: "text", text: `Tool execution error: ${err.message}` }],
    isError: true
  };
}
```

**Thiết kế giải pháp — Client**:
Sau khi `CallToolAsync` trả về, parse JSON và kiểm tra cờ `isError`:
```csharp
// Trong McpAgent.cs sau khi nhận toolResult
JsonNode? resultNode = JsonNode.Parse(toolResult);
bool isToolError = resultNode?["result"]?["isError"]?.GetValue<bool>() ?? false;

// Nếu là lỗi tool, đánh dấu rõ ràng cho LLM
string toolContent = isToolError
    ? $"[TOOL ERROR] {extractContentText(resultNode)}"
    : extractContentText(resultNode);
```

**File thay đổi**:
- `mcp-server/src/tools/*.ts` — Tất cả 7 file tool: bọc `try/catch`, trả `isError: true` khi lỗi.
- `src/SupplyCoreERP.Mcp.Client/Agent/McpAgent.cs` — Thêm logic kiểm tra `isError` sau `CallToolAsync`.

---

### 2.4 Khai báo Client Capabilities (Client)

**Tài liệu tham chiếu**: `lifecycle.md` — Client **MUST** gửi danh sách capabilities trong request `initialize`.

**Hiện trạng**: [McpClientService.cs dòng 157](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs#L157): `capabilities = new { }` — object rỗng.

**Thiết kế giải pháp**:
Khai báo tường minh rằng Client hiện tại không hỗ trợ các tính năng mở rộng. Mặc dù object rỗng về mặt kỹ thuật cũng biểu thị "không có capability", nhưng khai báo rõ ràng là best practice:

```csharp
capabilities = new
{
    // Client hiện tại không hỗ trợ sampling, elicitation, roots
    // Khai báo tường minh để Server biết rõ
}
```

Nếu tương lai cần hỗ trợ `sampling` (cho phép Server yêu cầu LLM completion qua Client):
```csharp
capabilities = new { sampling = new { } }
```

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` — Cập nhật object `capabilities` trong `initPayload`.

---

## 3. Nhóm 2 — Mức SHOULD (Độ tin cậy)

### 3.1 Binding Address 127.0.0.1 (Server)

**Tài liệu tham chiếu**: `transports.md` — Server **SHOULD** bind chỉ đến `127.0.0.1`, KHÔNG PHẢI `0.0.0.0`.

**Hiện trạng**: `app.listen(port)` mặc định bind `0.0.0.0`.

**Thiết kế giải pháp**:
```typescript
const host = process.env.HOST || "127.0.0.1";
app.listen(port, host, () => {
  console.log(`[MCP-Server] Listening on ${host}:${port}`);
});
```

**File thay đổi**:
- `mcp-server/src/index.ts` — Thay đổi lệnh `app.listen`.

---

### 3.2 Request Timeout + CancellationToken (Client)

**Tài liệu tham chiếu**: `lifecycle.md` — Client **SHOULD** thiết lập timeout cho tất cả request. Khi timeout → **SHOULD** gửi cancellation notification.

**Hiện trạng**: `HttpClient` không cấu hình `Timeout`. `SendAsync` không truyền `CancellationToken`.

**Thiết kế giải pháp**:
- Cấu hình `HttpClient.Timeout = TimeSpan.FromSeconds(30)` trong constructor.
- Tạo `CancellationTokenSource` với timeout cho mỗi request trong `SendMcpRequestAsync`.
- Khi `TaskCanceledException` xảy ra → log cảnh báo timeout.

```csharp
// Trong SendMcpRequestAsync
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
using HttpResponseMessage response = await _httpClient.SendAsync(request, cts.Token);
```

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` — Constructor (cấu hình Timeout) và `SendMcpRequestAsync` (truyền CancellationToken).

---

### 3.3 Rate Limiting (Server)

**Tài liệu tham chiếu**: `tools.md` — Server **SHOULD** rate limit tool invocations.

**Hiện trạng**: Không có cơ chế rate limiting.

**Thiết kế giải pháp**:
- Cài đặt package `express-rate-limit`.
- Áp dụng middleware với giới hạn 100 request/phút cho endpoint `/mcp`.
- Key function dựa trên `mcp-session-id` header (nếu có) hoặc IP.

```typescript
import rateLimit from "express-rate-limit";

const mcpLimiter = rateLimit({
  windowMs: 60 * 1000,  // 1 phút
  max: 100,
  keyGenerator: (req) => req.headers["mcp-session-id"] as string || req.ip,
  message: { error: "Rate limit exceeded" }
});

app.use("/mcp", mcpLimiter);
```

**File thay đổi**:
- `mcp-server/package.json` — Thêm dependency `express-rate-limit`.
- `mcp-server/src/index.ts` — Import và áp dụng middleware.

---

### 3.4 Sanitize Tool Output (Server)

**Tài liệu tham chiếu**: `tools.md` — Server **MUST** sanitize tool outputs.

**Hiện trạng**: Kết quả DB trả về trực tiếp `JSON.stringify(rows)` không qua lọc. Có nguy cơ rò rỉ các cột hệ thống nhạy cảm của ABP Framework.

**Thiết kế giải pháp**:
- Tạo hàm tiện ích `sanitizeRows(rows)` trong file mới `src/utils/sanitize.ts`.
- Loại bỏ các cột hệ thống: `CreatorId`, `LastModifierId`, `DeleterId`, `IsDeleted`, `DeletionTime`, `ExtraProperties`, `ConcurrencyStamp`, `TenantId`.
- Gọi `sanitizeRows` trước khi `JSON.stringify` trong tất cả tool handlers.

```typescript
const SENSITIVE_COLUMNS = [
  "CreatorId", "LastModifierId", "DeleterId",
  "IsDeleted", "DeletionTime",
  "ExtraProperties", "ConcurrencyStamp", "TenantId"
];

export function sanitizeRows(rows: any[]): any[] {
  return rows.map(row => {
    const clean = { ...row };
    for (const col of SENSITIVE_COLUMNS) {
      delete clean[col];
    }
    return clean;
  });
}
```

**File thay đổi**:
- `mcp-server/src/utils/sanitize.ts` — File mới.
- `mcp-server/src/tools/*.ts` — Tất cả 7 file tool: gọi `sanitizeRows` trước khi trả kết quả.

---

### 3.5 DELETE on Shutdown (Client)

**Tài liệu tham chiếu**: `transports.md` — Client rời khỏi phiên **SHOULD** gửi HTTP DELETE với `MCP-Session-Id` để giải phóng tài nguyên trên Server.

**Hiện trạng**: `McpClientService` không implement `IDisposable`. Không có logic gọi `DELETE /mcp`.

**Thiết kế giải pháp**:
- Implement `IDisposable` trên `McpClientService`.
- Trong `Dispose()`: hủy SSE listener (`_sseCts.Cancel()`), gửi `DELETE /mcp` với header `mcp-session-id`.
- ABP Framework tự gọi `Dispose` cho Singleton dependency khi ứng dụng shutdown.

```csharp
public class McpClientService : IMcpClientService, ISingletonDependency, IDisposable
{
    public void Dispose()
    {
        _sseCts?.Cancel();
        if (!string.IsNullOrEmpty(_sessionId))
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, $"{mcpUrl}");
                request.Headers.Add("mcp-session-id", _sessionId);
                _httpClient.Send(request); // Đồng bộ vì Dispose không async
            }
            catch { /* Best effort cleanup */ }
        }
    }
}
```

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` — Implement `IDisposable`, thêm `Dispose()`.

---

### 3.6 Xử lý tường minh HTTP 404 — Auto-Reconnect (Client)

**Tài liệu tham chiếu**: `transports.md` — Client nhận 404 → **MUST** khởi tạo phiên mới bằng `InitializeRequest` mà không gửi kèm session ID.

**Hiện trạng**: Khi nhận lỗi HTTP, chỉ ném `Exception` chung. Không phân biệt 404 với các lỗi khác.

**Thiết kế giải pháp**:
Trong `SendMcpRequestAsync`, kiểm tra tường minh mã 404:
```csharp
if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
{
    _logger.LogWarning("MCP Session expired (404). Reconnecting...");
    _isConnected = false;
    _sessionId = null;
    await EnsureConnectedAsync();
    // Retry request 1 lần duy nhất
    return await RetrySendAsync(payload, requestId);
}
```
- Giới hạn retry tối đa 1 lần để tránh vòng lặp vô hạn khi Server liên tục trả 404.

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` — Thêm logic kiểm tra 404 và retry.

---

### 3.7 Tool Annotations (Server)

**Tài liệu tham chiếu**: `tools.md` — Tools có thể khai báo `annotations` metadata giúp LLM hiểu bản chất của tool.

**Hiện trạng**: Không tool nào khai báo annotations.

**Thiết kế giải pháp**:
Thêm `annotations` cho tất cả 7 tools hiện tại. Vì tất cả đều chỉ đọc DB:
```typescript
server.registerTool("get_products", {
  description: "...",
  inputSchema: z.object({...}),
  annotations: {
    readOnlyHint: true,
    destructiveHint: false,
    idempotentHint: true,
    openWorldHint: false
  }
}, async (args) => { ... });
```

**File thay đổi**:
- `mcp-server/src/tools/*.ts` — Tất cả 7 file tool: thêm `annotations`.

---

## 4. Nhóm 3 — Mức SHOULD (Best Practices)

### 4.1 Validate Tool Result trước khi đưa cho LLM (Client)

**Tài liệu tham chiếu**: `client-best-practices.md` — Client **SHOULD** validate tool results before passing to LLM.

**Hiện trạng**: `toolResult` là chuỗi raw JSON từ MCP Server, được đưa thẳng vào lịch sử hội thoại.

**Thiết kế giải pháp**:
- Kiểm tra kích thước result. Nếu vượt 50KB → cắt gọn và ghi chú `[TRUNCATED]` cho LLM biết.
- Kiểm tra JSON hợp lệ. Nếu không parse được → gói lại thành text thông thường.

```csharp
const int MaxToolResultLength = 50 * 1024; // 50KB

string validatedResult = toolResult;
if (toolResult.Length > MaxToolResultLength)
{
    validatedResult = toolResult[..MaxToolResultLength] + "\n[TRUNCATED: Result exceeded 50KB limit]";
}
```

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Agent/McpAgent.cs` — Thêm logic validate/truncate sau `CallToolAsync`.

---

### 4.2 Agent Loop Max Iterations Guard (Client)

**Tài liệu tham chiếu**: Best practice — Client **SHOULD** implement safeguards against infinite loops.

**Hiện trạng**: [McpAgent.cs dòng 40](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Mcp.Client/Agent/McpAgent.cs#L40): `while (true)` — không có giới hạn.

**Thiết kế giải pháp**:
```csharp
const int MaxAgentIterations = 10;
int iteration = 0;

while (iteration < MaxAgentIterations)
{
    iteration++;
    // ... existing loop body ...
}

// Nếu thoát loop vì vượt ngưỡng
return new AgentResultDto
{
    FinalText = "Agent đã vượt quá giới hạn 10 vòng lặp xử lý. Vui lòng thử lại với yêu cầu đơn giản hơn.",
    RequiresApproval = false
};
```

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Agent/McpAgent.cs` — Thay `while (true)` bằng `while (iteration < MaxAgentIterations)`.

---

### 4.3 SSE Parsing — Xử lý nhiều Events trong Stream (Client)

**Tài liệu tham chiếu**: `transports.md` — Server **MAY** gửi nhiều request/notification TRƯỚC response trên cùng một SSE stream.

**Hiện trạng**: `SendMcpRequestAsync` chỉ lấy dòng `data:` **đầu tiên** tìm thấy. Nếu Server gửi notification trước response, Client nhầm notification đó là response.

**Thiết kế giải pháp**:
Thay vì lấy dòng `data:` đầu tiên, tìm dòng `data:` chứa JSON-RPC response có trường `id` khớp với `requestId`:

```csharp
if (responseContent.StartsWith("event:") || responseContent.Contains("data:"))
{
    string[] lines = responseContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    foreach (string line in lines)
    {
        if (line.StartsWith("data:"))
        {
            string jsonData = line.Substring(5).Trim();
            // Chỉ trả về nếu JSON-RPC response có id khớp requestId
            if (jsonData.Contains($"\"id\":\"{requestId}\"") || jsonData.Contains($"\"id\":\"{requestId}\""))
            {
                return jsonData;
            }
        }
    }
}
```

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` — Sửa logic bóc tách SSE trong `SendMcpRequestAsync`.

---

### 4.4 Thay Hardcoded Debug Log bằng ILogger (Client)

**Tài liệu tham chiếu**: Best practice — Logging nên sử dụng framework logging chuẩn.

**Hiện trạng**: `File.AppendAllText("D:\\ProjectOwner\\SupplyCoreERP\\mcp-client-debug.log", ...)` — đường dẫn tuyệt đối hardcoded.

**Thiết kế giải pháp**:
- Inject `ILogger<McpClientService>` qua constructor.
- Thay toàn bộ lời gọi `LogDebug(message)` bằng `_logger.LogDebug(message)`.
- Xóa hàm `LogDebug` thủ công và logic ghi file trong constructor.

```csharp
public class McpClientService : IMcpClientService, ISingletonDependency, IDisposable
{
    private readonly ILogger<McpClientService> _logger;

    public McpClientService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<McpClientService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }
}
```

**File thay đổi**:
- `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` — Inject `ILogger`, thay toàn bộ `LogDebug`, xóa hàm `LogDebug` và logic ghi file.

---

## 5. Tổng hợp File Thay đổi

### Các file cần sửa đổi:
| File | Các điểm liên quan |
| :--- | :--- |
| `mcp-server/src/index.ts` | 2.1 (Origin), 2.2 (Version), 3.1 (Binding), 3.3 (Rate Limit) |
| `mcp-server/src/tools/product.ts` | 2.3 (isError), 3.4 (Sanitize), 3.7 (Annotations) |
| `mcp-server/src/tools/warehouse.ts` | 2.3, 3.4, 3.7 |
| `mcp-server/src/tools/supplier.ts` | 2.3, 3.4, 3.7 |
| `mcp-server/src/tools/customer.ts` | 2.3, 3.4, 3.7 |
| `mcp-server/src/tools/batch.ts` | 2.3, 3.4, 3.7 |
| `mcp-server/src/tools/unit.ts` | 2.3, 3.4, 3.7 |
| `mcp-server/src/tools/balance.ts` | 2.3, 3.4, 3.7 |
| `mcp-server/.env` | 2.1 (ALLOWED_ORIGINS) |
| `mcp-server/package.json` | 3.3 (express-rate-limit) |
| `src/SupplyCoreERP.Mcp.Client/Mcp/McpClientService.cs` | 2.2, 2.4, 3.2, 3.5, 3.6, 4.3, 4.4 |
| `src/SupplyCoreERP.Mcp.Client/Agent/McpAgent.cs` | 2.3 (isError check), 4.1 (validate), 4.2 (max iterations) |

### Các file mới:
| File | Mục đích |
| :--- | :--- |
| `mcp-server/src/utils/sanitize.ts` | 3.4 — Hàm loại bỏ cột nhạy cảm từ kết quả DB |

---

## 6. Tiêu chuẩn Nghiệm thu

1. **Biên dịch**: `npm run build` (MCP Server) và `dotnet build` (Backend) đều thành công không lỗi.
2. **Origin Validation**: Request có `Origin` header không hợp lệ bị trả về HTTP 403.
3. **Protocol Version**: Handshake sử dụng phiên bản `2025-06-18`.
4. **isError Handling**: Khi tool query DB thất bại, response chứa `isError: true` và LLM nhận được thông báo lỗi rõ ràng.
5. **Timeout**: Request vượt 30 giây bị cancel với log cảnh báo.
6. **Rate Limit**: Request thứ 101 trong 1 phút bị trả về lỗi rate limit.
7. **Sanitize**: Kết quả tool không chứa các cột hệ thống ABP (`CreatorId`, `IsDeleted`, ...).
8. **Annotations**: Tất cả 7 tools khai báo `readOnlyHint: true`.
9. **DELETE**: Khi ứng dụng .NET shutdown, log ghi nhận đã gửi DELETE request đến Server.
10. **404 Reconnect**: Khi Server trả 404, Client tự động reconnect và retry thành công.
11. **Agent Guard**: Vòng lặp Agent dừng sau tối đa 10 vòng lặp.
12. **SSE Parsing**: Client lọc đúng JSON-RPC response theo `requestId`, bỏ qua notification trước đó.
13. **ILogger**: Không còn debug log hardcoded, tất cả log đi qua `ILogger`.
