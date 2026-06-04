import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { queryDb } from "../db.js";

export const registerUnitTools = (server: McpServer) => {
  server.registerTool(
    "get_units",
    {
      description: "Retrieve the list of base units of measure (Base Units) in the SupplyCoreERP system.",
      inputSchema: z.object({
        name: z.string().optional().describe("Unit name to search for (e.g., Box, Tablet)"),
        code: z.string().optional().describe("Unit code to search for"),
        limit: z.number().optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
      })
    },
    async ({ name, code, limit }) => {
      let query = `SELECT "Id", "Code", "Name" FROM "AppBaseUnits" WHERE "IsDeleted" = false`;
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
          return { content: [{ type: "text", text: "No units found matching the criteria." }] };
        }

        const text = rows.map(r => `Code: ${r.Code} | Name: ${r.Name} | ID: ${r.Id}`).join("\n");
        return { content: [{ type: "text", text: `Unit List:\n${text}` }] };
      } catch (error: any) {
        return {
          isError: true,
          content: [{ type: "text", text: `Database query error: ${error.message}` }]
        };
      }
    }
  );
};
