# Kế hoạch Triển khai: MCP Server Streamable HTTP & Tái cấu trúc

Kế hoạch này mô tả các bước chi tiết để triển khai nâng cấp MCP Server và C# Client theo tài liệu thiết kế đã được phê duyệt.

---

## Danh sách các Task triển khai

### [ ] Task 1: Nâng cấp thư viện và Cấu hình package.json
*   **Mục tiêu**: Cài đặt phiên bản MCP SDK mới nhất và thư viện Zod.
*   **Các file thay đổi**:
    - [package.json](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/package.json)
*   **Hành động**:
    - Cập nhật dependencies: Thêm `"zod"`, nâng cấp `@modelcontextprotocol/sdk` (hoặc cài đặt các package con `@modelcontextprotocol/server`, `@modelcontextprotocol/express`, `@modelcontextprotocol/node` tùy thuộc vào phiên bản phát hành mới nhất).
    - Chạy lệnh `npm install` trong thư mục [mcp-server](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server).

### [ ] Task 2: Tạo các file định nghĩa Specific Tools (Zod-based)
*   **Mục tiêu**: Tạo các tool chuyên biệt cho từng danh mục thực thể thay thế cho generic query.
*   **Các file tạo mới**:
    - [product.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/tools/product.ts) (Định nghĩa `get_products`)
    - [warehouse.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/tools/warehouse.ts) (Định nghĩa `get_warehouses`)
    - [supplier.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/tools/supplier.ts) (Định nghĩa `get_suppliers`)
    - [customer.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/tools/customer.ts) (Định nghĩa `get_customers`)
    - [batch.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/tools/batch.ts) (Định nghĩa `get_batches`)
    - [unit.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/tools/unit.ts) (Định nghĩa `get_units`)
    - [balance.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/tools/balance.ts) (Định nghĩa `get_inventory_balance`)

### [ ] Task 3: Tạo Resources và Prompts cho MCP Server
*   **Mục tiêu**: Cung cấp schema database và prompt mẫu hỗ trợ AI.
*   **Các file tạo mới**:
    - [dbSchema.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/resources/dbSchema.ts) (Resource `db_schema`)
    - [assistant.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/prompts/assistant.ts) (Prompt `analyze_inventory_balance`)

### [ ] Task 4: Cập nhật Entrypoint index.ts của MCP Server
*   **Mục tiêu**: Khởi chạy `McpServer`, đăng ký toàn bộ tools/resources/prompts, hỗ trợ chạy song song STDIO và Streamable HTTP (Stateless JSON Mode).
*   **Các file thay đổi**:
    - [index.ts](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server/src/index.ts)

### [ ] Task 5: Biên dịch và Kiểm thử STDIO với Antigravity CLI
*   **Mục tiêu**: Đảm bảo chế độ chạy STDIO của MCP Server tương tác trơn tru với CLI.
*   **Hành động**:
    - Chạy `npm run build` trong thư mục [mcp-server](file:///D:/ProjectOwner/SupplyCoreERP/mcp-server).
    - Restart plugin `supplycore-mcp-server` trên Antigravity CLI và kiểm tra việc gọi các tool `get_products`, `get_warehouses`...

### [ ] Task 6: Cập nhật C# Client của Backend
*   **Mục tiêu**: Viết lại mã nguồn giao tiếp HTTP để tương thích với Stateless Streamable HTTP của server.
*   **Các file thay đổi**:
    - [McpClientService.cs](file:///D:/ProjectOwner/SupplyCoreERP/src/SupplyCoreERP.Mcp.Client/McpClientService.cs)
*   **Hành động**:
    - Thay thế luồng gọi SSE GET và các luồng đọc stream thành các request HTTP POST JSON trực tiếp đến `/mcp`.

### [ ] Task 7: Kiểm thử Toàn hệ thống (End-to-End)
*   **Mục tiêu**: Đảm bảo cả CLI và C# Backend đều kết nối và thực thi tool chính xác.
