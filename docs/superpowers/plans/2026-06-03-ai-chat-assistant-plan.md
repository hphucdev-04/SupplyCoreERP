# Kế hoạch Triển khai: Trợ lý AI Chat Assistant (RAG & Node.js MCP Server)

Kế hoạch này chia nhỏ quá trình tích hợp Trợ lý AI với Node.js MCP Server và Gemini API thành các task tuyến tính để thực hiện từng bước.

---

## GIAI ĐOẠN 1: XÂY DỰNG DỰ ÁN NODE.JS MCP SERVER (TYPESCRIPT)

### Task 1: Khởi tạo cấu trúc dự án `mcp-server-node`
*   **Mục tiêu**: Tạo khung dự án Node.js TypeScript và cài đặt các thư viện cần thiết.
*   **Các file sẽ tạo mới**:
    *   `mcp-server-node/package.json`
    *   `mcp-server-node/tsconfig.json`
*   **Nội dung công việc**:
    1. Tạo thư mục `mcp-server-node` ở thư mục gốc của workspace.
    2. Cấu hình `package.json` với các package `@modelcontextprotocol/sdk`, `express`, `pg`, `dotenv` và typescript compiler.
    3. Cấu hình `tsconfig.json` cho TypeScript ESModules.

### Task 2: Cấu hình kết nối Database PostgreSQL (`src/db.ts`)
*   **Mục tiêu**: Thiết lập kết nối an toàn đến database PostgreSQL (Neon Cloud) bằng thư viện `pg`.
*   **Các file sẽ tạo mới**:
    *   `mcp-server-node/src/db.ts`
    *   `mcp-server-node/.env` (Lưu connection string local)
*   **Nội dung công việc**:
    1. Đọc biến môi trường `DATABASE_URL`.
    2. Khởi tạo một `Pool` kết nối từ thư viện `pg`.
    3. Viết hàm helper `queryDb(text: string, params?: any[])` để thực thi câu lệnh SQL.

### Task 3: Định nghĩa và thực thi các MCP Tools (`src/tools.ts`)
*   **Mục tiêu**: Định nghĩa schema và logic truy vấn cho các Tools tra cứu tồn kho sản phẩm.
*   **Các file sẽ tạo mới**:
    *   `mcp-server-node/src/tools.ts`
*   **Nội dung công việc**:
    1. Viết hàm `getToolsDefinition()` trả về mô tả metadata của tool `get_inventory_balance` theo chuẩn JSON Schema của MCP.
    2. Viết hàm `executeTool(name: string, args: any)` thực thi câu lệnh SQL lấy số lượng tồn kho theo sản phẩm/kho từ bảng `AppInventoryBalances` và trả về kết quả định dạng text cho AI.

### Task 4: Dựng Express Server với SSE Transport (`src/index.ts`)
*   **Mục tiêu**: Khởi chạy ứng dụng Express, expose endpoint SSE cho MCP Client kết nối.
*   **Các file sẽ tạo mới**:
    *   `mcp-server-node/src/index.ts`
*   **Nội dung công việc**:
    1. Khởi tạo MCP `Server` instance từ SDK.
    2. Thiết lập endpoint `GET /sse` để khởi tạo kết nối Server-Sent Events (SSE).
    3. Thiết lập endpoint `POST /messages` để nhận các yêu cầu JSON-RPC từ client.
    4. Lắng nghe trên cổng `3000` và chạy thử nghiệm.

---

## GIAI ĐOẠN 2: TÍCH HỢP MCP CLIENT VÀO BACKEND C# (.NET 10 ERP)

### Task 5: Tạo DTOs và Interface ứng dụng (`IAiChatAppService`)
*   **Mục tiêu**: Định nghĩa các cấu trúc dữ liệu truyền nhận tin nhắn giữa Angular và Backend C#.
*   **Các file sẽ tạo mới**:
    *   `src/SupplyCoreERP.Application.Contracts/AiChats/Dtos/ChatMessageDto.cs`
    *   `src/SupplyCoreERP.Application.Contracts/AiChats/Dtos/ChatRequestInputDto.cs`
    *   `src/SupplyCoreERP.Application.Contracts/AiChats/Dtos/ChatResponseOutputDto.cs`
    *   `src/SupplyCoreERP.Application.Contracts/AiChats/IAiChatAppService.cs`
*   **Nội dung công việc**: Định nghĩa các DTOs lưu tin nhắn, lịch sử chat và interface ứng dụng kế thừa `IApplicationService`.

### Task 6: Xây dựng lớp dịch vụ kết nối MCP Client (`McpClientService`)
*   **Mục tiêu**: Viết logic kết nối HTTP/SSE tới Node.js MCP Server và thực thi hội thoại qua Gemini API.
*   **Các file sẽ tạo mới**:
    *   `src/SupplyCoreERP.Application/AiChats/Mcp/IMcpClientService.cs`
    *   `src/SupplyCoreERP.Application/AiChats/Mcp/McpClientService.cs`
*   **Nội dung công việc**:
    1. Cấu hình HttpClient gọi sang Node.js MCP Server lấy danh sách tools.
    2. Cấu hình gửi request chat tới Gemini API (sử dụng API Key).
    3. Xử lý vòng lặp gọi hàm: Nếu Gemini yêu cầu gọi tool, C# Backend gửi HTTP request thực thi tool sang Node.js MCP Server, nhận kết quả và gửi lại cho Gemini.

### Task 7: Triển khai ứng dụng service `AiChatAppService`
*   **Mục tiêu**: Thực thi logic API nhận tin nhắn từ Angular, bắt buộc Authorization.
*   **Các file sẽ tạo mới**:
    *   `src/SupplyCoreERP.Application/AiChats/AiChatAppService.cs`
*   **Nội dung công việc**: Triển khai `AiChatAppService` kế thừa `SupplyCoreERPAppService`, gắn attribute `[Authorize]` và gọi `McpClientService` để trả lời tin nhắn.

---

## GIAI ĐOẠN 3: CẬP NHẬT ANGULAR UI & TEST E2E

### Task 8: Tích hợp API thực tế vào UI Angular
*   **Mục tiêu**: Cập nhật chat widget gửi tin nhắn lên API backend C# và truyền kèm JWT token.
*   **Các file sẽ thay đổi**:
    *   `angular/src/app/shared/components/ai-chat.component/ai-chat.component.ts`
*   **Nội dung công việc**:
    1. Inject `OAuthService` để lấy token.
    2. Cấu hình thuộc tính `[request]` của `<deep-chat>` trỏ tới endpoint `/api/app/ai-chat/send-message`.
    3. Viết `requestInterceptor` để format tin nhắn và lịch sử chat tương thích với DTO của Backend.

### Task 9: Chạy kiểm thử toàn trình (E2E Test)
*   **Mục tiêu**: Xác minh toàn bộ luồng hoạt động chính xác từ UI đến Database.
*   **Nội dung công việc**:
    1. Khởi chạy Node.js MCP Server (`npm run dev`).
    2. Khởi chạy Backend C# (.NET).
    3. Khởi chạy Angular Client.
    4. Chat thử nghiệm tra cứu tồn kho sản phẩm, kiểm tra kết quả hiển thị trên UI và log gọi tool ở MCP Server.

### Task 10: Viết cấu hình Deploy lên Railway
*   **Mục tiêu**: Thiết lập Dockerfile để deploy Node.js MCP Server lên Railway độc lập.
*   **Các file sẽ tạo mới**:
    *   `mcp-server-node/Dockerfile`
*   **Nội dung công việc**: Viết Dockerfile tối giản (sử dụng Alpine Node image) để build và chạy ứng dụng Express TypeScript trên Railway.
