import { z } from "zod";
import { queryDb } from "../db.js";
import { sanitizeResponse } from "../utils/security.js";
const inventoryFilterSchema = z.object({
    warehouseIds: z.array(z.string().uuid()).max(100).optional().describe("Warehouse Ids to filter"),
    productIds: z.array(z.string().uuid()).max(200).optional().describe("Product Ids to filter")
});
export const registerInventoryTools = (server) => {
    server.registerTool("get_inventory_snapshot", {
        description: "Retrieve current inventory balances by product and warehouse, including available quantity.",
        inputSchema: inventoryFilterSchema,
        annotations: {
            readOnlyHint: true,
            destructiveHint: false,
            idempotentHint: true,
            openWorldHint: false
        }
    }, async ({ warehouseIds, productIds }) => {
        let query = `
        SELECT
          ib."WarehouseId",
          w."Code" AS "WarehouseCode",
          w."Name" AS "WarehouseName",
          ib."ProductId",
          p."Code" AS "ProductCode",
          p."Name" AS "ProductName",
          SUM(ib."Quantity") AS "OnHandQuantity",
          SUM(ib."LockedQuantity") AS "LockedQuantity",
          SUM(ib."Quantity" - ib."LockedQuantity") AS "AvailableQuantity"
        FROM "AppInventoryBalances" ib
        INNER JOIN "AppWarehouses" w ON w."Id" = ib."WarehouseId"
        INNER JOIN "AppProducts" p ON p."Id" = ib."ProductId"
        WHERE w."IsDeleted" = false
          AND p."IsDeleted" = false
      `;
        const params = [];
        if (warehouseIds && warehouseIds.length > 0) {
            params.push(warehouseIds);
            query += ` AND ib."WarehouseId" = ANY($${params.length})`;
        }
        if (productIds && productIds.length > 0) {
            params.push(productIds);
            query += ` AND ib."ProductId" = ANY($${params.length})`;
        }
        query += `
        GROUP BY
          ib."WarehouseId",
          w."Code",
          w."Name",
          ib."ProductId",
          p."Code",
          p."Name"
        ORDER BY w."Name" ASC, p."Name" ASC
      `;
        try {
            const rows = await queryDb(query, params);
            if (rows.length === 0) {
                return { content: [{ type: "text", text: "No inventory balances found matching the criteria." }] };
            }
            const sanitizedRows = sanitizeResponse(rows);
            const items = sanitizedRows.map((row) => ({
                warehouseId: row.WarehouseId,
                warehouseCode: row.WarehouseCode,
                warehouseName: row.WarehouseName,
                productId: row.ProductId,
                productCode: row.ProductCode,
                productName: row.ProductName,
                onHandQuantity: Number(row.OnHandQuantity),
                lockedQuantity: Number(row.LockedQuantity),
                availableQuantity: Number(row.AvailableQuantity)
            }));
            return {
                content: [{
                        type: "text",
                        text: JSON.stringify(items)
                    }]
            };
        }
        catch (error) {
            return {
                isError: true,
                content: [{ type: "text", text: `Database query error: ${error.message}` }]
            };
        }
    });
    server.registerTool("get_inventory_reservations_summary", {
        description: "Retrieve reservation totals by product and warehouse to explain committed stock.",
        inputSchema: inventoryFilterSchema,
        annotations: {
            readOnlyHint: true,
            destructiveHint: false,
            idempotentHint: true,
            openWorldHint: false
        }
    }, async ({ warehouseIds, productIds }) => {
        let query = `
        SELECT
          ir."WarehouseId",
          w."Code" AS "WarehouseCode",
          w."Name" AS "WarehouseName",
          ir."ProductId",
          p."Code" AS "ProductCode",
          p."Name" AS "ProductName",
          SUM(ir."ReservedQuantity") AS "ReservedQuantity"
        FROM "AppInventoryReservations" ir
        INNER JOIN "AppWarehouses" w ON w."Id" = ir."WarehouseId"
        INNER JOIN "AppProducts" p ON p."Id" = ir."ProductId"
        WHERE w."IsDeleted" = false
          AND p."IsDeleted" = false
      `;
        const params = [];
        if (warehouseIds && warehouseIds.length > 0) {
            params.push(warehouseIds);
            query += ` AND ir."WarehouseId" = ANY($${params.length})`;
        }
        if (productIds && productIds.length > 0) {
            params.push(productIds);
            query += ` AND ir."ProductId" = ANY($${params.length})`;
        }
        query += `
        GROUP BY
          ir."WarehouseId",
          w."Code",
          w."Name",
          ir."ProductId",
          p."Code",
          p."Name"
        ORDER BY w."Name" ASC, p."Name" ASC
      `;
        try {
            const rows = await queryDb(query, params);
            if (rows.length === 0) {
                return { content: [{ type: "text", text: "No inventory reservations found matching the criteria." }] };
            }
            const sanitizedRows = sanitizeResponse(rows);
            const items = sanitizedRows.map((row) => ({
                warehouseId: row.WarehouseId,
                warehouseCode: row.WarehouseCode,
                warehouseName: row.WarehouseName,
                productId: row.ProductId,
                productCode: row.ProductCode,
                productName: row.ProductName,
                reservedQuantity: Number(row.ReservedQuantity)
            }));
            return {
                content: [{
                        type: "text",
                        text: JSON.stringify(items)
                    }]
            };
        }
        catch (error) {
            return {
                isError: true,
                content: [{ type: "text", text: `Database query error: ${error.message}` }]
            };
        }
    });
};
