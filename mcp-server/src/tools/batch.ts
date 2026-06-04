import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { queryDb } from "../db.js";

export const registerBatchTools = (server: McpServer) => {
  server.registerTool(
    "get_batches",
    {
      description: "Retrieve the list of product batches and lots (Product Batches) in the SupplyCoreERP system.",
      inputSchema: z.object({
        batchNumber: z.string().optional().describe("Product batch/lot number to search for (e.g., LOT123)"),
        code: z.string().optional().describe("Product batch management code to search for"),
        limit: z.number().optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
      })
    },
    async ({ batchNumber, code, limit }) => {
      let query = `SELECT "Id", "Code", "BatchNumber", "ExpiryDate", "Status" FROM "AppProductBatches" WHERE "IsDeleted" = false`;
      const params: any[] = [];

      if (batchNumber) {
        params.push(`%${batchNumber}%`);
        query += ` AND "BatchNumber" ILIKE $${params.length}`;
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
          return { content: [{ type: "text", text: "No product batches found matching the criteria." }] };
        }

        const text = rows.map(r => {
          const expiryDateStr = r.ExpiryDate ? new Date(r.ExpiryDate).toLocaleDateString('en-US') : 'N/A';
          return `Code: ${r.Code} | Batch Number: ${r.BatchNumber} | Expiry Date: ${expiryDateStr} | Status: ${r.Status || 'N/A'}`;
        }).join("\n");
        return { content: [{ type: "text", text: `Product Batch List:\n${text}` }] };
      } catch (error: any) {
        return {
          isError: true,
          content: [{ type: "text", text: `Database query error: ${error.message}` }]
        };
      }
    }
  );
};
