import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { queryDb } from "../db.js";
import { validateSqlQuery, checkSelectStarSensitiveColumns, sanitizeResponse } from "../utils/security.js";

export const registerQueryTools = (server: McpServer) => {
  server.registerTool(
    "read_query",
    {
      description: "Execute a read-only SQL query (SELECT/WITH) on the SupplyCoreERP PostgreSQL database.",
      inputSchema: z.object({
        sql: z.string().describe("SQL SELECT or WITH query. Use $1, $2 for parameters."),
        params: z.array(z.string()).optional().describe("Parameter values for $1, $2 placeholders.")
      }),
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ sql, params }) => {
      try {
        // 1. Validate SQL
        const validation = await validateSqlQuery(sql);
        if (!validation.isValid) {
          return {
            isError: true,
            content: [{ type: "text", text: `SQL Validation Error: ${validation.errorReason}` }]
          };
        }

        // 2. Check SELECT * for sensitive columns
        if (validation.hasStar && validation.tables.length > 0) {
          const starCheck = await checkSelectStarSensitiveColumns(validation.tables);
          if (!starCheck.allowed) {
            return {
              isError: true,
              content: [{ type: "text", text: `SQL Validation Error: ${starCheck.reason}` }]
            };
          }
        }

        // 3. Execute query
        const rawRows = await queryDb(sql, params || []);

        // 4. Sanitize response
        const sanitizedRows = sanitizeResponse(rawRows);

        if (sanitizedRows.length === 0) {
          return { content: [{ type: "text", text: "No rows returned from the query." }] };
        }

        return {
          content: [{
            type: "text",
            text: JSON.stringify(sanitizedRows, null, 2)
          }]
        };
      } catch (error: any) {
        const errorDetails = [
          `PostgreSQL Error [${error.code || 'UNKNOWN'}]: ${error.message}`,
          error.hint ? `Hint: ${error.hint}` : null
        ].filter(Boolean).join('\n');

        return {
          isError: true,
          content: [{ type: "text", text: errorDetails }]
        };
      }
    }
  );
};
