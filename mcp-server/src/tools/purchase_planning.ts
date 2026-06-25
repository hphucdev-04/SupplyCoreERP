import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { buildPurchasePlanAsync } from "../services/purchasePlanningService.js";

const planPurchaseForNextMonthSchema = z.object({
  targetYear: z.number().int().min(2000).max(2100).describe("Planning target year"),
  targetMonth: z.number().int().min(1).max(12).describe("Planning target month"),
  warehouseIds: z.array(z.string().uuid()).max(100).optional().describe("Warehouse Ids to filter"),
  productIds: z.array(z.string().uuid()).max(200).optional().describe("Product Ids to filter"),
  forecastMethod: z.enum(["avg_delivered_last_3_months"]).optional().default("avg_delivered_last_3_months").describe("Forecast calculation method"),
  demandSource: z.enum(["sales_orders"]).optional().default("sales_orders").describe("Demand source"),
  safetyStockDays: z.number().int().min(0).max(90).optional().default(7).describe("Safety stock days"),
  includeRequisitionBacklog: z.boolean().optional().default(true).describe("Include purchase requisition backlog in planning"),
  preferredSupplierOnly: z.boolean().optional().default(false).describe("Only use preferred suppliers")
});

export const registerPurchasePlanningTools = (server: McpServer) => {
  server.registerTool(
    "plan_purchase_for_next_month",
    {
      description: "Build a purchase plan for the target month using delivered sales history, current stock, inbound purchase orders, requisition backlog, and supplier conditions.",
      inputSchema: planPurchaseForNextMonthSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async (input) => {
      try {
        const result = await buildPurchasePlanAsync(input);
        return {
          content: [{
            type: "text",
            text: JSON.stringify(result)
          }]
        };
      } catch (error: any) {
        return {
          isError: true,
          content: [{ type: "text", text: `Purchase planning error: ${error.message}` }]
        };
      }
    }
  );
};
