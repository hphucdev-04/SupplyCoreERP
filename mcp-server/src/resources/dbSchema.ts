import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { fileURLToPath } from "url";
import path from "path";
import fs from "fs/promises";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export const registerDatabaseResources = (server: McpServer) => {
  server.registerResource(
    "db_schema",
    "schema://database",
    {
      mimeType: "text/markdown",
      description: "Cung cấp sơ đồ cấu trúc cơ sở dữ liệu các bảng của SupplyCoreERP để AI hiểu mối quan hệ khóa ngoại."
    },
    async (uri) => {
      try {
        const filePath = path.resolve(__dirname, "../../resources/db_schema.md");
        const schemaMarkdown = await fs.readFile(filePath, "utf-8");
        return {
          contents: [{
            uri: uri.href,
            mimeType: "text/markdown",
            text: schemaMarkdown
          }]
        };
      } catch (error: any) {
        console.error("[MCP-Server] Lỗi khi đọc file db_schema.md:", error);
        return {
          contents: [{
            uri: uri.href,
            mimeType: "text/markdown",
            text: "# Lỗi nạp sơ đồ cơ sở dữ liệu\nKhông thể đọc file cấu hình sơ đồ cơ sở dữ liệu."
          }]
        };
      }
    }
  );
};

