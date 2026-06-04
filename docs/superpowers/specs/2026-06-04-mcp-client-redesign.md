# Tài liệu Đặc tả Thiết kế Tái cấu trúc C# MCP Host Client - SupplyCoreERP (Kiến trúc AI Agent Stateless)

Tài liệu này đặc tả chi tiết kiến trúc và thiết kế kỹ thuật để tái cấu trúc C# MCP Client trong dự án `SupplyCoreERP`. Thiết kế này áp dụng **Stateless Agent Pattern** theo tiêu chuẩn MCP, đóng gói toàn bộ logic Agent Loop và khả năng tích hợp AI bên trong project `SupplyCoreERP.Mcp.Client` dưới tên gọi `McpAgent` hoàn toàn không có trạng thái (Stateless). Tầng Application chịu trách nhiệm quản lý trạng thái phiên làm việc (State Persistence) và cung cấp các cổng API công khai (`Approve`, `Reject`).

---

## 1. Bối cảnh và Mục tiêu

### 1.1 Vấn đề hiện tại
- **Tight Coupling**: Phiên bản MCP Client cũ kết dính chặt chẽ cấu trúc dữ liệu LLM (Gemini) ở tầng Application.
- **Tính mở rộng (Scalability)**: Thiết kế cũ trói buộc Agent vào kịch bản Chat tương tác, tự động truy cập database lưu session làm mất khả năng tái sử dụng Agent cho các tác vụ chạy ngầm (Background Jobs) hoặc các kênh khác (Telegram, Webhook).
- **Tránh Flag Arguments**: Việc sử dụng một cờ boolean `Approved` để điều phối cả hai hành động Đồng ý và Từ chối làm mờ ngữ nghĩa của API.

### 1.2 Mục tiêu tái cấu trúc
- **Stateless Agent Engine**: `IAgent` (McpAgent) hoạt động hoàn toàn không trạng thái. Đầu vào duy nhất là `AgentContext` (danh sách các bước thực thi) và trả về `AgentResultDto`. Nó không có quyền truy cập hay phụ thuộc vào cơ sở dữ liệu.
- **State Persistence tại Application Layer**: `AgentAppService` chịu trách nhiệm đọc/ghi phiên làm việc (`AgentSession`) từ Database, chuẩn bị `AgentContext` và điều phối luồng API.
- **Explicit API (Tách biệt Approve/Reject)**: Tách biệt hoàn toàn hành động Phê duyệt (`Approve`) và Từ chối (`Reject`) thành hai phương thức API riêng biệt.
- **Human-in-the-loop (HITL)**: Hỗ trợ ngắt luồng gọi tool ghi nhạy cảm để xin phê duyệt từ giao diện thông qua `AgentSession` được quản lý bởi AppService.

---

## 2. Thiết kế Kiến trúc Tổng thể

```mermaid
graph TD
    DeepChat[Angular UI Client - DeepChat] -->|1. Message / Approve / Reject| AgentAppService[AgentAppService]
    
    subgraph Contracts [Application.Contracts Layer]
        IAgentAppService[IAgentAppService Interface]
        IAgent[IAgent Interface]
    end
    
    AgentAppService -->|2. Call IAgent.RunAsync| McpAgent[McpAgent - Stateless Engine]
    AgentAppService <-->|3. Save/Restore Session State| Database[PostgreSQL Database]
    AgentAppService -->|4. Execute Write Tool| McpClientService[McpClientService]
    
    subgraph McpClientProject [SupplyCoreERP.Mcp.Client Project]
        McpAgent -->|5. Generate Content| IAgentProvider[IAgentProvider]
        GeminiProvider[GeminiProvider] -.-> IAgentProvider
        
        McpAgent -->|6. Execute Read Tool| IMcpClientService[IMcpClientService]
        McpClientService -.-> IMcpClientService
    end
    
    McpClientService -->|7. HTTP POST /mcp| McpServer[Node.js MCP Server]
```

### 2.1 Trách nhiệm của các thành phần
- **`AgentAppService`** (Application Layer): Quản lý đọc/ghi `AgentSession` từ Database, thực thi các tool ghi nhạy cảm sau khi được duyệt, nạp kết quả hoặc từ chối vào lịch sử hội thoại, và gửi ngữ cảnh hoàn chỉnh sang Agent để tiếp tục chạy.
- **`IAgent` & `McpAgent`** (Stateless Agent Engine): Bộ não điều phối AI. Chạy vòng lặp Agent Loop (Agent Loop), gọi LLM sinh content, tự động gọi các tool MCP không nhạy cảm và trả về kết quả cuối cùng hoặc yêu cầu tạm ngắt xin phê duyệt.
- **`IAgentProvider` & `GeminiProvider`** (Nội bộ `Mcp.Client`): Nhà cung cấp dịch vụ mô hình AI (Gemini).
- **`IMcpClientService` & `McpClientService`** (Nội bộ `Mcp.Client`): Vận chuyển HTTP stateless kết nối với MCP Server.
- **`AgentSession`** (Domain Layer): Thực thể lưu trạng thái phiên làm việc của Agent.

---

## 3. Cấu trúc Cơ sở dữ liệu (Database Schema)

Thực thể `AgentSession` kế thừa từ `CreationAuditedEntity<Guid>` để duy trì ngữ cảnh Agent:

```csharp
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Ai;

public class AgentSession : CreationAuditedEntity<Guid>
{
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Chuỗi JSON lưu trữ danh sách các đối tượng AgentChatMessageDto đại diện cho lịch sử hội thoại của Agent.
    /// </summary>
    public string ConversationHistoryJson { get; set; }
    
    /// <summary>
    /// Đánh dấu phiên làm việc này có đang bị tạm dừng chờ người dùng phê duyệt hay không.
    /// </summary>
    public bool IsPendingApproval { get; set; }
    
    /// <summary>
    /// Chuỗi JSON lưu trữ thông tin của Tool Call đang chờ được thực thi sau khi duyệt.
    /// </summary>
    public string? PendingToolCallJson { get; set; }
}
```

---

## 4. Cấu trúc Interfaces & DTOs (Tầng Contracts - `SupplyCoreERP.Application.Contracts`)

### 4.1 interfaces

```csharp
// Agent/IAgentAppService.cs
using System.Threading.Tasks;
using SupplyCoreERP.Agent.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Agent;

public interface IAgentAppService : IApplicationService
{
    Task<object> SendMessageAsync(AgentRequestInputDto input);
    Task<object> ApproveAsync(AgentSessionInputDto input);
    Task<object> RejectAsync(AgentSessionInputDto input);
}

// Agent/IAgent.cs
using System.Threading.Tasks;
using SupplyCoreERP.Agent.Dtos;

namespace SupplyCoreERP.Agent;

public interface IAgent
{
    // Interface hoàn toàn stateless, chỉ nhận Context thực thi
    Task<AgentResultDto> RunAsync(AgentContext context);
}
```

### 4.2 DTOs phục vụ API và UI
```csharp
// Agent/Dtos/AgentRequestInputDto.cs
public class AgentRequestInputDto
{
    [Required]
    public string Text { get; set; }
    public List<AgentMessageDto> History { get; set; } = new();
}

// Agent/Dtos/AgentSessionInputDto.cs
public class AgentSessionInputDto
{
    [Required]
    public Guid SessionId { get; set; }
}

// Agent/Dtos/AgentMessageDto.cs
public class AgentMessageDto
{
    [Required]
    public string Role { get; set; } // "user" | "model" | "system"
    [Required]
    public string Text { get; set; }
}

// Agent/Dtos/AgentContext.cs
public class AgentContext
{
    public List<AgentMessageDto> Steps { get; set; } = new();
}

// Agent/Dtos/AgentResultDto.cs
public class AgentResultDto
{
    public string? FinalText { get; set; }
    public bool RequiresApproval { get; set; }
    public string? PendingToolName { get; set; }
    public string? PendingToolArguments { get; set; }
}
```

---

## 5. Tích hợp Giao diện UI (Angular)

Component Angular gọi API endpoints riêng biệt một cách tường minh:

- **Gửi tin nhắn**: `POST /api/app/agent/send-message`
- **Gửi phê duyệt (Đồng ý)**: `POST /api/app/agent/approve`
- **Gửi từ chối (Không đồng ý)**: `POST /api/app/agent/reject`

---

## 6. Sơ đồ Luồng hoạt động (Sequence Diagrams)

### 6.1 Luồng Agent Loop (Tự động thực thi với Read-Only Tools)
```mermaid
sequenceDiagram
    autonumber
    Angular Client->>AgentAppService: SendMessageAsync (Text, History)
    AgentAppService->>McpAgent: RunAsync (AgentContext)
    
    McpAgent->>McpClientService: GetToolsAsync
    McpClientService-->>McpAgent: Trả về danh sách McpToolDto
    
    loop Vòng lặp Agent Loop
        McpAgent->>GeminiProvider: GenerateContentAsync (Context, Tools)
        GeminiProvider-->>McpAgent: Trả về AgentResponseDto (Yêu cầu gọi "get_inventory_balance")
        
        Note over McpAgent: Phát hiện tool "get_inventory_balance" <br> RequiresApproval == false
        
        McpAgent->>McpClientService: CallToolAsync (get_inventory_balance, Arguments)
        McpClientService-->>McpAgent: Kết quả JSON thô
        
        McpAgent->>McpAgent: Thêm kết quả tool vào Lịch sử Context
    end
    
    McpAgent->>GeminiProvider: GenerateContentAsync (Lịch sử có kết quả tool)
    GeminiProvider-->>McpAgent: Câu trả lời cuối cùng (text)
    
    McpAgent-->>AgentAppService: AgentResultDto (FinalText)
    AgentAppService-->>Angular Client: Kết quả hiển thị cho user
```

### 6.2 Luồng Human Loop (Tạm ngắt phê duyệt với Write Tools)
Sơ đồ mô tả luồng tạm ngắt cuộc hội thoại khi gặp tool nhạy cảm, và hai luồng xử lý riêng biệt rẽ ngay từ đầu tương ứng với hành động Phê duyệt (Approve) hoặc Từ chối (Reject) của người dùng.

```mermaid
sequenceDiagram
    autonumber
    Angular Client->>AgentAppService: SendMessageAsync (Text, History)
    AgentAppService->>McpAgent: RunAsync (AgentContext)
    
    McpAgent->>GeminiProvider: GenerateContentAsync (Context, Tools)
    GeminiProvider-->>McpAgent: Trả về AgentResponseDto (Yêu cầu gọi "update_inventory")
    
    Note over McpAgent: Phát hiện tool "update_inventory" <br> RequiresApproval == true
    
    McpAgent-->>AgentAppService: Trả về AgentResultDto (RequiresApproval=true)
    AgentAppService->>Database: Lưu AgentSession (Lịch sử, PendingToolCall, IsPendingApproval=true)
    AgentAppService-->>Angular Client: Trả về JSON PendingApproval
    Note over Angular Client: Hiển thị giao diện "Đồng ý/Từ chối"

    alt Người dùng nhấn nút "Đồng ý" (Approve Flow)
        Angular Client->>AgentAppService: ApproveAsync (SessionId)
        AgentAppService->>Database: Tải AgentSession
        AgentAppService->>McpClientService: CallToolAsync (update_inventory, Arguments)
        McpClientService-->>AgentAppService: Kết quả JSON thành công
        AgentAppService->>AgentAppService: Nạp kết quả Tool vào Lịch sử Context
        
    else Người dùng nhấn nút "Từ chối" (Reject Flow)
        Angular Client->>AgentAppService: RejectAsync (SessionId)
        AgentAppService->>Database: Tải AgentSession
        AgentAppService->>AgentAppService: Nạp thông báo từ chối "User rejected..." vào Lịch sử Context
    end

    AgentAppService->>McpAgent: RunAsync (AgentContext đã cập nhật kết quả / từ chối)
    McpAgent->>GeminiProvider: GenerateContentAsync (Lịch sử mới)
    GeminiProvider-->>McpAgent: Trả về câu trả lời cuối cùng (text)
    McpAgent-->>AgentAppService: AgentResultDto (FinalText)
    
    AgentAppService->>Database: Cập nhật Lịch sử mới, đặt IsPendingApproval=false & Clear PendingToolCall
    AgentAppService-->>Angular Client: Trả về câu trả lời cuối cùng
```

### 6.3 Sơ đồ dòng dữ liệu và Giao thức mạng môi trường Production
Sơ đồ mô tả chi tiết dòng dữ liệu và giao thức truyền tải giữa các thành phần trên môi trường Production:

```mermaid
sequenceDiagram
    autonumber
    participant Angular as Angular Client (Vercel / Browser)
    participant C_Sharp as ABP Backend (Railway)
    participant Database as PostgreSQL (Neon Cloud - Cổng 5432)
    participant Gemini as Gemini API (HTTPS Cloud)
    participant McpServer as Node.js MCP (Railway)

    Note over Angular, C_Sharp: Giao thức: HTTPS (Internet)<br>Endpoint: https://backend-production.railway.app/api/...
    Angular->>C_Sharp: POST /api/app/agent/send-message
    
    Note over C_Sharp, Database: Giao thức: TCP/IP + SSL/TLS (Chỉ đọc/ghi Session hội thoại)<br>ConnectionString: neon.tech:5432
    C_Sharp->>Database: SELECT Session State (AgentSession)
    Database-->>C_Sharp: Trả về trạng thái Session (nếu có)
    
    Note over C_Sharp, McpServer: Giao thức: HTTP (Mạng nội bộ Railway - Private Network)<br>Endpoint: http://mcp-server.railway.internal:3000/mcp
    C_Sharp->>McpServer: POST /mcp (tools/list)
    McpServer-->>C_Sharp: HTTP 200 OK (Danh sách Schema của Tools)
    
    Note over C_Sharp, Gemini: Giao thức: HTTPS (Internet - Google Cloud API)<br>Endpoint: https://generativelanguage.googleapis.com/...
    C_Sharp->>Gemini: POST generateContent
    Gemini-->>C_Sharp: HTTP 200 OK (LlmResponseDto)

    alt Trường hợp Tool tự động (RequiresApproval == false)
        Note over C_Sharp, McpServer: Giao thức: HTTP (Mạng nội bộ Railway - Private Network)
        C_Sharp->>McpServer: POST /mcp (tools/call)
        
        Note over McpServer, Database: Giao thức: TCP/IP + SSL/TLS (Chỉ thực thi truy vấn nghiệp vụ ERP)<br>ConnectionString: neon.tech:5432
        McpServer->>Database: Thực thi câu lệnh SQL (ví dụ: SELECT sản phẩm/tồn kho)
        Database-->>McpServer: Trả về dữ liệu bảng kết quả nghiệp vụ
        
        McpServer-->>C_Sharp: HTTP 200 OK (Trả về kết quả thực thi Tool dạng JSON)
        
        C_Sharp->>Gemini: POST (Gửi lại lịch sử đã đính kèm kết quả Tool)
        Gemini-->>C_Sharp: HTTP 200 OK (Câu trả lời cuối cùng)
        C_Sharp-->>Angular: HTTP 200 OK (Trả về text kết luận hiển thị trực tiếp)
        
    else Trường hợp cần duyệt (RequiresApproval == true)
        C_Sharp->>Database: TCP/IP + SSL/TLS (Lưu Session hội thoại)
        Database-->>C_Sharp: Xác nhận lưu thành công
        C_Sharp-->>Angular: HTTP 200 OK (Trả về JSON status: "PendingApproval")
        
        Note over Angular, C_Sharp: Giao thức: HTTPS (Internet)
        alt Người dùng Đồng ý (Approve)
            Angular->>C_Sharp: POST /api/app/agent/approve
            C_Sharp->>Database: Tải Session
            C_Sharp->>McpServer: POST /mcp (tools/call)
            McpServer-->>C_Sharp: Trả về kết quả thực thi Tool dạng JSON
        else Người dùng Từ chối (Reject)
            Angular->>C_Sharp: POST /api/app/agent/reject
            C_Sharp->>Database: Tải Session
            C_Sharp->>C_Sharp: Cập nhật lịch sử với text từ chối
        end
        
        C_Sharp->>Database: TCP/IP + SSL/TLS (Cập nhật lịch sử & Xóa trạng thái chờ)
        C_Sharp->>Gemini: POST (Gửi lại lịch sử để lấy câu trả lời)
        Gemini-->>C_Sharp: HTTP 200 OK (Câu trả lời cuối cùng)
        C_Sharp-->>Angular: HTTP 200 OK (Trả về câu trả lời cuối cùng)
    end
```

---

## 7. Tiêu chuẩn Nghiệm thu và Kiểm thử
1. **Biên dịch**: Solution `SupplyCoreERP` phải biên dịch thành công 100% không lỗi cú pháp.
2. **Encapsulation**: Project `Contracts` không tham chiếu hay chứa bất kỳ định nghĩa nào liên quan đến LLM API hay Mcp Client kỹ thuật.
3. **Explicit API**: Cung cấp hai endpoint độc lập `/api/app/agent/approve` và `/api/app/agent/reject` thay vì dùng chung cờ boolean.
4. **HITL Flow**: Khi chạy, nếu tool được đánh dấu `RequiresApproval` từ Node.js server, luồng chat phải bị ngắt, lưu DB thành công, và khôi phục xử lý bình thường sau khi Client gửi request `/approve` hoặc `/reject`.
5. **Stateless Agent**: Lớp `McpAgent` hoàn toàn độc lập với các Repository và Database.
