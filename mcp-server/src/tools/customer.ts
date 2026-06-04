import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { queryDb } from "../db.js";

export const registerCustomerTools = (server: McpServer) => {
  server.registerTool(
    "get_customers",
    {
      description: "Retrieve the list of customers in the SupplyCoreERP system.",
      inputSchema: z.object({
        name: z.string().optional().describe("Customer name to search for"),
        phoneNumber: z.string().optional().describe("Customer phone number to search for"),
        limit: z.number().optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
      })
    },
    async ({ name, phoneNumber, limit }) => {
      let query = `SELECT "Id", "Code", "Name", "PhoneNumber" FROM "AppCustomers" WHERE "IsDeleted" = false`;
      const params: any[] = [];

      if (name) {
        params.push(`%${name}%`);
        query += ` AND "Name" ILIKE $${params.length}`;
      }
      if (phoneNumber) {
        params.push(`%${phoneNumber}%`);
        query += ` AND "PhoneNumber" ILIKE $${params.length}`;
      }

      query += ` LIMIT $${params.length + 1}`;
      params.push(Math.min(limit, 50));

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No customers found matching the criteria." }] };
        }

        const text = rows.map(r => `Code: ${r.Code} | Name: ${r.Name} | Phone: ${r.PhoneNumber || 'N/A'}`).join("\n");
        return { content: [{ type: "text", text: `Customer List:\n${text}` }] };
      } catch (error: any) {
        return {
          isError: true,
          content: [{ type: "text", text: `Database query error: ${error.message}` }]
        };
      }
    }
  );
};
