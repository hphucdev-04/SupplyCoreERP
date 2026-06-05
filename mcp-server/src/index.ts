import express, { Request, Response, NextFunction } from "express";
import { randomUUID } from "crypto";
import { McpServer, isInitializeRequest, StdioServerTransport } from "@modelcontextprotocol/server";
import { NodeStreamableHTTPServerTransport } from "@modelcontextprotocol/node";
import dotenv from "dotenv";
import path from "path";
import { fileURLToPath } from "url";
import rateLimit from "express-rate-limit";

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

// Hàm khởi tạo và đăng ký toàn bộ nghiệp vụ cho một instance McpServer mới
const createMcpServer = () => {
  const server = new McpServer({
    name: "supplycore-mcp-server",
    version: "1.0.0"
  });

  registerProductTools(server);
  registerWarehouseTools(server);
  registerSupplierTools(server);
  registerCustomerTools(server);
  registerBatchTools(server);
  registerUnitTools(server);
  registerBalanceTools(server);
  registerDatabaseResources(server);
  registerPrompts(server);

  return server;
};

// Khởi chạy ở chế độ STDIO hoặc Streamable HTTP
const isStdio = process.argv.includes("--stdio");

if (isStdio) {
  // Chạy chế độ STDIO (dành cho CLI/IDE/MCP Inspector)
  const server = createMcpServer();
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("[MCP-Server] SupplyCore MCP Server is running on STDIO mode.");
} else {
  // Chạy chế độ Streamable HTTP (dành cho C# Client)
  const app = express();
  
  // Middleware kiểm tra Origin Header (DNS Rebinding Protection)
  const validateOrigin = (req: Request, res: Response, next: NextFunction) => {
    const origin = req.headers.origin;
    if (origin) {
      const allowedOrigins = (process.env.ALLOWED_ORIGINS || "").split(",").map(o => o.trim());
      if (!allowedOrigins.includes(origin)) {
        res.status(403).json({ error: "Forbidden: Invalid Origin" });
        return;
      }
    }
    next();
  };

  // Middleware Rate Limiting cho MCP
  const mcpLimiter = rateLimit({
    windowMs: 60 * 1000, // 1 phút
    max: 100,
    keyGenerator: (req) => (req.headers["mcp-session-id"] as string) || req.ip || "",
    message: { error: "Rate limit exceeded" },
    standardHeaders: true,
    legacyHeaders: false,
    validate: false 
  });

  app.use(express.json());
  app.use(validateOrigin);
  app.use("/mcp", mcpLimiter);

  // Map để quản lý transport của các phiên theo Session ID
  const transports: { [sessionId: string]: NodeStreamableHTTPServerTransport } = {};

  // Endpoint duy nhất tiếp nhận tất cả các HTTP POST requests (JSON-RPC)
  app.post("/mcp", async (req: Request, res: Response) => {
    const sessionId = req.headers["mcp-session-id"] as string | undefined;
    console.log(`[MCP-Server] ==> POST /mcp | Session: ${sessionId || "Khoi tao moi"} | Method: ${req.body?.method || "N/A"}`);

    try {
      let transport: NodeStreamableHTTPServerTransport;

      if (sessionId && transports[sessionId]) {
        // Tái sử dụng transport đã tồn tại cho phiên này
        transport = transports[sessionId];
      } else if (!sessionId && isInitializeRequest(req.body)) {
        // Tạo phiên mới khi nhận được initialize request
        transport = new NodeStreamableHTTPServerTransport({
          sessionIdGenerator: () => randomUUID(),
          onsessioninitialized: (sid) => {
            console.log(`[MCP-Server] Session duoc khoi tao voi ID: ${sid}`);
            transports[sid] = transport;
          }
        });

        transport.onclose = () => {
          const sid = transport.sessionId;
          if (sid && transports[sid]) {
            console.log(`[MCP-Server] Dong transport va xoa session: ${sid}`);
            delete transports[sid];
          }
        };

        // Kết nối transport mới này với một instance McpServer độc lập của phiên
        const server = createMcpServer();
        await server.connect(transport);

        await transport.handleRequest(req, res, req.body);
        return;
      } else if (sessionId) {
        res.status(404).json({
          jsonrpc: "2.0",
          error: { code: -32001, message: "Session not found or expired" },
          id: req.body?.id || null
        });
        return;
      } else {
        res.status(400).json({
          jsonrpc: "2.0",
          error: { code: -32000, message: "Bad Request: Session ID required" },
          id: req.body?.id || null
        });
        return;
      }

      // Tiếp tục xử lý request với transport hiện tại
      await transport.handleRequest(req, res, req.body);
    } catch (err) {
      console.error("[MCP-Error] Error POST /mcp:", err);
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
  });

  // Hỗ trợ GET request cho SSE Streams (phòng trường hợp client đăng ký nhận event stream)
  app.get("/mcp", async (req: Request, res: Response) => {
    const sessionId = (req.headers["mcp-session-id"] || req.query.sessionId) as string | undefined;
    console.log(`[MCP-Server] ==> GET /mcp (Mo SSE Stream) | Session: ${sessionId || "N/A"}`);

    if (!sessionId) {
      res.status(400).send("Missing session ID");
      return;
    }
    if (!transports[sessionId]) {
      res.status(404).send("Session not found");
      return;
    }

    try {
      const transport = transports[sessionId];
      await transport.handleRequest(req, res);
    } catch (err) {
      console.error("[MCP-Error] Loi khi xu ly GET /mcp:", err);
      if (!res.headersSent) {
        res.status(500).send("Error establishing SSE stream");
      }
    }
  });

  // Hỗ trợ DELETE request để đóng session theo đặc tả MCP
  app.delete("/mcp", async (req: Request, res: Response) => {
    const sessionId = req.headers["mcp-session-id"] as string | undefined;
    console.log(`[MCP-Server] ==> DELETE /mcp (Dong Session) | Session: ${sessionId || "N/A"}`);

    if (!sessionId) {
      res.status(400).send("Missing session ID");
      return;
    }
    if (!transports[sessionId]) {
      res.status(404).send("Session not found");
      return;
    }

    try {
      const transport = transports[sessionId];
      await transport.handleRequest(req, res);
    } catch (err) {
      console.error("[MCP-Error] Loi khi xoa session:", err);
      if (!res.headersSent) {
        res.status(500).send("Error deleting session");
      }
    }
  });
  // Endpoint kích hoạt thông báo thay đổi tools
  app.post("/mcp/tools/changed", async (req: Request, res: Response) => {
    try {
      console.log("[MCP-Server] Received tools change notification, broadcasting to all sessions...");
      for (const sid in transports) {
        try {
          await transports[sid].send({
            jsonrpc: "2.0",
            method: "notifications/tools/list_changed"
          });
          console.log(`[MCP-Server] Sent tools/list_changed notification to session: ${sid}`);
        } catch (err) {
          console.error(`[MCP-Error] Error sending notification to session ${sid}:`, err);
        }
      }
      res.status(200).json({ success: true, message: "Broadcasted list_changed to all sessions" });
    } catch (err) {
      console.error("[MCP-Error] Error when broadcasting list_changed:", err);
      res.status(500).json({ success: false, error: String(err) });
    }
  });

  const port = process.env.PORT || 3000;
  const host = process.env.HOST || "127.0.0.1";
  app.listen(Number(port), host, () => {
    console.log("=============================================================");
    console.log(`[MCP-Server] SupplyCore MCP Server is running (Streamable HTTP)`);
    console.log(`[MCP-Server] Endpoint POST/GET/DELETE: http://${host}:${port}/mcp`);
    console.log("=============================================================");
  });

  // Dọn dẹp tài nguyên khi tắt server
  process.on("SIGINT", async () => {
    console.log("[MCP-Server] Cleaning up active transports...");
    for (const sid in transports) {
      try {
        await transports[sid].close();
        delete transports[sid];
      } catch (err) {
        console.error(`[MCP-Error] Error closing session ${sid}:`, err);
      }
    }
    process.exit(0);
  });
}
