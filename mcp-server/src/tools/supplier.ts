import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { queryDb } from "../db.js";
import { sanitizeRows } from "../utils/sanitize.js";

export const registerSupplierTools = (server: McpServer) => {
  server.registerTool(
    "get_suppliers",
    {
      description: "Retrieve the list of suppliers in the SupplyCoreERP system.",
      inputSchema: z.object({
        name: z.string().optional().describe("Supplier name to search for"),
        code: z.string().optional().describe("Supplier code to search for"),
        limit: z.number().optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
      }),
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ name, code, limit }) => {
      let query = `SELECT "Id", "Code", "Name", "PhoneNumber", "Email" FROM "AppSuppliers" WHERE "IsDeleted" = false`;
      const params: any[] = [];

      if (name) {
        params.push(`%${name}%`);
        query += ` AND "Name" ILIKE $${params.length}`;
      }
      if (code) {
        params.push(`%${code}%`);
        query += ` AND "Code" ILIKE $${params.length}`;
      }

      query += ` LIMIT $${params.length + 1}`;
      params.push(Math.min(limit, 50));

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No suppliers found matching the criteria." }] };
        }

        const sanitizedRows = sanitizeRows(rows);
        const text = sanitizedRows.map(r => `Code: ${r.Code} | Name: ${r.Name} | Phone: ${r.PhoneNumber || 'N/A'} | Email: ${r.Email || 'N/A'}`).join("\n");
        return { content: [{ type: "text", text: `Supplier List:\n${text}` }] };
      } catch (error: any) {
        return {
          isError: true,
          content: [{ type: "text", text: `Database query error: ${error.message}` }]
        };
      }
    }
  );
};
