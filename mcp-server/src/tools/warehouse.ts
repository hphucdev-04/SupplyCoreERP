import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { queryDb } from "../db.js";
import { sanitizeResponse } from "../utils/security.js";

const listWarehousesSchema = z.object({
  isActive: z.boolean().optional().describe("Filter active or inactive warehouses"),
  limit: z.number().int().min(1).max(50).optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
});

export const registerWarehouseTools = (server: McpServer) => {
  server.registerTool(
    "list_warehouses",
    {
      description: "Retrieve warehouses available in the SupplyCoreERP system for planning and stock lookup.",
      inputSchema: listWarehousesSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ isActive, limit }) => {
      let query = `
        SELECT
          w."Id",
          w."Code",
          w."Name",
          w."Address",
          w."Status",
          w."IsActive"
        FROM "AppWarehouses" w
        WHERE w."IsDeleted" = false
      `;
      const params: Array<boolean | number> = [];

      if (typeof isActive === "boolean") {
        params.push(isActive);
        query += ` AND w."IsActive" = $${params.length}`;
      }

      query += ` ORDER BY w."Name" ASC LIMIT $${params.length + 1}`;
      params.push(limit);

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No warehouses found matching the criteria." }] };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          warehouseId: row.Id,
          warehouseCode: row.Code,
          warehouseName: row.Name,
          address: row.Address,
          status: row.Status,
          isActive: row.IsActive
        }));

        return {
          content: [{
            type: "text",
            text: JSON.stringify(items)
          }]
        };
      } catch (error: any) {
        return {
          isError: true,
          content: [{ type: "text", text: `Database query error: ${error.message}` }]
        };
      }
    }
  );
};
