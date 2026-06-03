import express from "express";
import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { SSEServerTransport } from "@modelcontextprotocol/sdk/server/sse.js";
import { CallToolRequestSchema, ListToolsRequestSchema } from "@modelcontextprotocol/sdk/types.js";
import { getToolsDefinition, executeTool } from "./tools.js";
import dotenv from "dotenv";
dotenv.config();
const app = express();
const port = process.env.PORT || 3000;
// 1. Khởi tạo MCP Server với mô tả metadata
const server = new Server({
    name: "supplycore-erp-mcp-server",
    version: "1.0.0",
}, {
    capabilities: {
        tools: {}, // Báo cho client biết server có khả năng cung cấp tools
    },
});
// 2. Đăng ký API Handler cho yêu cầu lấy danh sách Tools
server.setRequestHandler(ListToolsRequestSchema, async () => {
    console.log("[MCP Server] Received request: list tools");
    return {
        tools: getToolsDefinition(),
    };
});
// 3. Đăng ký API Handler cho yêu cầu thực thi Tool cụ thể
server.setRequestHandler(CallToolRequestSchema, async (request) => {
    const { name, arguments: args } = request.params;
    console.log(`[MCP Server] Received request: call tool -> ${name}`, { args });
    try {
        const result = await executeTool(name, args);
        return result;
    }
    catch (error) {
        console.error(`[MCP Server] Error executing tool '${name}':`, error);
        return {
            isError: true,
            content: [{ type: "text", text: `Error: ${error.message}` }],
        };
    }
});
// 4. Quản lý các kết nối SSE (Server-Sent Events) từ client
let transport = null;
// Endpoint HTTP GET /sse: Thiết lập đường ống SSE
app.get("/sse", async (req, res) => {
    console.log("[Express] Client connecting via SSE...");
    transport = new SSEServerTransport("/messages", res);
    await server.connect(transport);
    req.on("close", () => {
        console.log("[Express] SSE connection closed by client");
    });
});
// Endpoint HTTP POST /messages: Nhận các tin nhắn điều khiển từ client
app.post("/messages", async (req, res) => {
    if (!transport) {
        res.status(400).send("No active SSE connection found. Initialize connection on /sse first.");
        return;
    }
    await transport.handlePostMessage(req, res);
});
// 5. Khởi chạy server lắng nghe kết nối
app.listen(port, () => {
    console.log("=============================================================");
    console.log(`[MCP-Server] SupplyCore MCP Server is running!`);
    console.log(`[MCP-Server] Local Base URL: http://localhost:${port}`);
    console.log(`[MCP-Server] SSE Connection Endpoint: http://localhost:${port}/sse`);
    console.log(`[MCP-Server] Messages POST Endpoint: http://localhost:${port}/messages`);
    console.log("=============================================================");
});
