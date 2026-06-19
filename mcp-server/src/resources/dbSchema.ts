import { McpServer } from "@modelcontextprotocol/server";
import path from "path";
import fs from "fs/promises";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

export const registerDatabaseResources = (server: McpServer) => {
  server.registerResource(
    "db_schema",
    "schema://database",
    {
      mimeType: "text/markdown",
      description: "Database schema of all SupplyCoreERP tables, columns, data types, and foreign key relationships."
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
        console.error("[MCP-Server] Error reading db_schema.md:", error);
        return {
          contents: [{
            uri: uri.href,
            mimeType: "text/markdown",
            text: "# Error loading database schema\nFailed to read the database schema configuration file."
          }]
        };
      }
    }
  );
};
