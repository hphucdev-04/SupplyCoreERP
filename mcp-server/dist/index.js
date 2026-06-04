import express from "express";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StreamableHTTPServerTransport } from "@modelcontextprotocol/sdk/server/streamableHttp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import dotenv from "dotenv";
import path from "path";
import { fileURLToPath } from "url";
// Import các hàm đăng ký tools
import { registerProductTools } from "./tools/product.js";
import { registerWarehouseTools } from "./tools/warehouse.js";
import { registerSupplierTools } from "./tools/supplier.js";
import { registerCustomerTools } from "./tools/customer.js";
import { registerBatchTools } from "./tools/batch.js";
import { registerUnitTools } from "./tools/unit.js";
import { registerBalanceTools } from "./tools/balance.js";
// Import các hàm đăng ký resources và prompts
import { registerDatabaseResources } from "./resources/dbSchema.js";
import { registerPrompts } from "./prompts/assistant.js";
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
// Load cấu hình biến môi trường bằng đường dẫn tuyệt đối
dotenv.config({ path: path.resolve(__dirname, "../.env") });
// 1. Khởi tạo McpServer instance mới
const server = new McpServer({
    name: "supplycore-mcp-server",
    version: "1.0.0"
});
// 2. Đăng ký toàn bộ Tools, Resources và Prompts vào server
registerProductTools(server);
registerWarehouseTools(server);
registerSupplierTools(server);
registerCustomerTools(server);
registerBatchTools(server);
registerUnitTools(server);
registerBalanceTools(server);
registerDatabaseResources(server);
registerPrompts(server);
// 3. Phân nhánh chế độ chạy (STDIO hoặc Streamable HTTP)
const isStdio = process.argv.includes("--stdio");
if (isStdio) {
    // Chạy chế độ STDIO (Phục vụ cho local CLI và IDE extension)
    const transport = new StdioServerTransport();
    await server.connect(transport);
    console.error("[MCP-Server] SupplyCore MCP Server is running on STDIO mode.");
}
else {
    // Chạy chế độ Streamable HTTP (Phục vụ remote connection cho C# Backend)
    const app = express();
    app.use(express.json());
    // Endpoint duy nhất POST /mcp xử lý các yêu cầu JSON-RPC từ C# Client
    app.post("/mcp", async (req, res) => {
        // Khởi tạo một transport mới cho mỗi request ở chế độ stateless
        const transport = new StreamableHTTPServerTransport({
            sessionIdGenerator: undefined, // Stateless mode
            enableJsonResponse: true // Trả về JSON body thuần túy thay vì SSE stream
        });
        try {
            console.log(`[MCP-Debug] === Nhận request ===`);
            console.log(`[MCP-Debug] Method: ${req.body?.method}`);
            console.log(`[MCP-Debug] Payload: ${JSON.stringify(req.body)}`);
            // Kết nối transport này với server
            await server.connect(transport);
            // Xử lý request
            await transport.handleRequest(req, res, req.body);
            console.log(`[MCP-Debug] Status trả về: ${res.statusCode}`);
            console.log(`[MCP-Debug] =======================`);
        }
        catch (err) {
            console.error("[MCP-Error] Lỗi khi xử lý request tại endpoint /mcp:", err);
            if (!res.headersSent) {
                res.status(500).json({
                    jsonrpc: "2.0",
                    error: {
                        code: -32603,
                        message: err instanceof Error ? err.message : String(err)
                    },
                    id: req.body?.id || null
                });
            }
        }
        finally {
            // Đóng transport để giải phóng tài nguyên sau khi hoàn tất request
            await transport.close();
        }
    });
    const port = process.env.PORT || 3000;
    app.listen(port, () => {
        console.log("=============================================================");
        console.log(`[MCP-Server] SupplyCore MCP Server is running (Streamable HTTP)`);
        console.log(`[MCP-Server] Endpoint POST: http://127.0.0.1:${port}/mcp`);
        console.log("=============================================================");
    });
}
