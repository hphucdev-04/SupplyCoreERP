import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { queryDb } from "../db.js";
import { sanitizeResponse } from "../utils/security.js";

const demandHistorySchema = z.object({
  warehouseIds: z.array(z.string().uuid()).max(100).optional().describe("Warehouse Ids to filter"),
  productIds: z.array(z.string().uuid()).max(200).optional().describe("Product Ids to filter"),
  fromDate: z.string().describe("Start date in YYYY-MM-DD format"),
  toDate: z.string().describe("End date in YYYY-MM-DD format"),
  bucket: z.enum(["day", "month"]).optional().default("month").describe("Aggregation bucket")
});

const monthlyForecastSchema = z.object({
  warehouseIds: z.array(z.string().uuid()).max(100).optional().describe("Warehouse Ids to filter"),
  productIds: z.array(z.string().uuid()).max(200).optional().describe("Product Ids to filter"),
  targetYear: z.number().int().min(2000).max(2100).describe("Forecast target year"),
  targetMonth: z.number().int().min(1).max(12).describe("Forecast target month"),
  method: z.enum(["avg_delivered_last_3_months"]).optional().default("avg_delivered_last_3_months").describe("Forecast calculation method")
});

export const registerDemandTools = (server: McpServer) => {
  server.registerTool(
    "get_demand_history",
    {
      description: "Retrieve delivered sales demand history by product and warehouse.",
      inputSchema: demandHistorySchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ warehouseIds, productIds, fromDate, toDate, bucket }) => {
      const periodExpression =
        bucket === "day"
          ? `TO_CHAR(so."OrderDate", 'YYYY-MM-DD')`
          : `TO_CHAR(DATE_TRUNC('month', so."OrderDate"), 'YYYY-MM')`;

      let query = `
        SELECT
          sol."ProductId",
          p."Code" AS "ProductCode",
          p."Name" AS "ProductName",
          so."WarehouseId",
          w."Code" AS "WarehouseCode",
          w."Name" AS "WarehouseName",
          ${periodExpression} AS "Period",
          SUM(sol."DeliveredQuantity") AS "DeliveredQuantity"
        FROM "AppSalesOrderLines" sol
        INNER JOIN "AppSalesOrders" so ON so."Id" = sol."SalesOrderId"
        INNER JOIN "AppProducts" p ON p."Id" = sol."ProductId"
        INNER JOIN "AppWarehouses" w ON w."Id" = so."WarehouseId"
        WHERE so."IsDeleted" = false
          AND p."IsDeleted" = false
          AND w."IsDeleted" = false
          AND so."OrderDate" >= $1
          AND so."OrderDate" < ($2::date + INTERVAL '1 day')
      `;
      const params: Array<string | string[]> = [fromDate, toDate];

      if (warehouseIds && warehouseIds.length > 0) {
        params.push(warehouseIds);
        query += ` AND so."WarehouseId" = ANY($${params.length})`;
      }

      if (productIds && productIds.length > 0) {
        params.push(productIds);
        query += ` AND sol."ProductId" = ANY($${params.length})`;
      }

      query += `
        GROUP BY
          sol."ProductId",
          p."Code",
          p."Name",
          so."WarehouseId",
          w."Code",
          w."Name",
          "Period"
        HAVING SUM(sol."DeliveredQuantity") > 0
        ORDER BY "Period" ASC, w."Name" ASC, p."Name" ASC
      `;

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No demand history found matching the criteria." }] };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          productId: row.ProductId,
          productCode: row.ProductCode,
          productName: row.ProductName,
          warehouseId: row.WarehouseId,
          warehouseCode: row.WarehouseCode,
          warehouseName: row.WarehouseName,
          period: row.Period,
          deliveredQuantity: Number(row.DeliveredQuantity)
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

  server.registerTool(
    "get_monthly_demand_forecast",
    {
      description: "Calculate the forecast demand for the target month using average delivered quantity from the last 3 months.",
      inputSchema: monthlyForecastSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ warehouseIds, productIds, targetYear, targetMonth }) => {
      const targetMonthStart = new Date(Date.UTC(targetYear, targetMonth - 1, 1));
      const historyEnd = new Date(Date.UTC(targetYear, targetMonth - 1, 0));
      const historyStart = new Date(Date.UTC(targetYear, targetMonth - 4, 1));

      const formatDate = (value: Date): string => value.toISOString().slice(0, 10);

      let query = `
        SELECT
          sol."ProductId",
          p."Code" AS "ProductCode",
          p."Name" AS "ProductName",
          so."WarehouseId",
          w."Code" AS "WarehouseCode",
          w."Name" AS "WarehouseName",
          AVG(monthly_data."DeliveredQuantity") AS "ForecastDemand"
        FROM (
          SELECT
            sol."ProductId",
            so."WarehouseId",
            DATE_TRUNC('month', so."OrderDate") AS "OrderMonth",
            SUM(sol."DeliveredQuantity") AS "DeliveredQuantity"
          FROM "AppSalesOrderLines" sol
          INNER JOIN "AppSalesOrders" so ON so."Id" = sol."SalesOrderId"
          WHERE so."IsDeleted" = false
            AND so."OrderDate" >= $1
            AND so."OrderDate" < ($2::date + INTERVAL '1 day')
          GROUP BY sol."ProductId", so."WarehouseId", DATE_TRUNC('month', so."OrderDate")
        ) monthly_data
        INNER JOIN "AppProducts" p ON p."Id" = monthly_data."ProductId"
        INNER JOIN "AppWarehouses" w ON w."Id" = monthly_data."WarehouseId"
        INNER JOIN "AppSalesOrderLines" sol ON sol."ProductId" = monthly_data."ProductId"
        INNER JOIN "AppSalesOrders" so ON so."Id" = sol."SalesOrderId" AND so."WarehouseId" = monthly_data."WarehouseId"
        WHERE p."IsDeleted" = false
          AND w."IsDeleted" = false
      `;
      const params: Array<string | string[]> = [formatDate(historyStart), formatDate(historyEnd)];

      if (warehouseIds && warehouseIds.length > 0) {
        params.push(warehouseIds);
        query += ` AND monthly_data."WarehouseId" = ANY($${params.length})`;
      }

      if (productIds && productIds.length > 0) {
        params.push(productIds);
        query += ` AND monthly_data."ProductId" = ANY($${params.length})`;
      }

      query += `
        GROUP BY
          monthly_data."ProductId",
          p."Code",
          p."Name",
          monthly_data."WarehouseId",
          w."Code",
          w."Name"
        ORDER BY w."Name" ASC, p."Name" ASC
      `;

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return {
            content: [{
              type: "text",
              text: JSON.stringify({
                targetYear,
                targetMonth,
                targetMonthStart: formatDate(targetMonthStart),
                method: "avg_delivered_last_3_months",
                items: []
              })
            }]
          };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          productId: row.ProductId,
          productCode: row.ProductCode,
          productName: row.ProductName,
          warehouseId: row.WarehouseId,
          warehouseCode: row.WarehouseCode,
          warehouseName: row.WarehouseName,
          forecastDemand: Number(row.ForecastDemand)
        }));

        return {
          content: [{
            type: "text",
            text: JSON.stringify({
              targetYear,
              targetMonth,
              targetMonthStart: formatDate(targetMonthStart),
              method: "avg_delivered_last_3_months",
              items
            })
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
