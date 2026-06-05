import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { queryDb } from "../db.js";
import { sanitizeRows } from "../utils/sanitize.js";

export const registerBalanceTools = (server: McpServer) => {
  server.registerTool(
    "get_inventory_balance",
    {
      description: "Query the physical stock inventory of a product/medicine by product code and warehouse code.",
      inputSchema: z.object({
        productCode: z.string().describe("Product/medicine code to query (e.g., SP001, MEDICINE002)"),
        warehouseCode: z.string().optional().describe("Warehouse code to filter by (e.g., KHO_HCM). If omitted, searches across all warehouses.")
      }),
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ productCode, warehouseCode }) => {
      let query = `
        SELECT w."Name" as "WarehouseName", b."Quantity", p."Name" as "ProductName"
        FROM "AppInventoryBalances" b
        JOIN "AppProducts" p ON b."ProductId" = p."Id"
        JOIN "AppWarehouses" w ON b."WarehouseId" = w."Id"
        WHERE p."Code" = $1 AND b."IsDeleted" = false AND p."IsDeleted" = false AND w."IsDeleted" = false
      `;
      const params = [productCode];

      if (warehouseCode) {
        params.push(warehouseCode);
        query += ` AND w."Code" = $2`;
      }

      try {
        const rows = await queryDb(query, params);
        
        if (rows.length === 0) {
          return {
            content: [{ 
              type: "text", 
              text: `No inventory balance found for product code '${productCode}'` + (warehouseCode ? ` in warehouse '${warehouseCode}'.` : ".")
            }]
          };
        }

        const sanitizedRows = sanitizeRows(rows);
        const resultText = sanitizedRows
          .map((r: any) => `Product: ${r.ProductName} | Warehouse: ${r.WarehouseName} | Quantity: ${Number(r.Quantity).toLocaleString('en-US')}`)
          .join("\n");

        return {
          content: [{ type: "text", text: resultText }]
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
