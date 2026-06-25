import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { queryDb } from "../db.js";
import { sanitizeResponse } from "../utils/security.js";

const purchaseFilterSchema = z.object({
  warehouseIds: z.array(z.string().uuid()).max(100).optional().describe("Warehouse Ids to filter"),
  productIds: z.array(z.string().uuid()).max(200).optional().describe("Product Ids to filter")
});

const inboundPipelineSchema = purchaseFilterSchema.extend({
  asOfDate: z.string().optional().describe("As of date in YYYY-MM-DD format")
});

const supplierSupplyOptionsSchema = z.object({
  productIds: z.array(z.string().uuid()).min(1).max(200).describe("Product Ids to filter"),
  supplierIds: z.array(z.string().uuid()).max(200).optional().describe("Supplier Ids to filter"),
  preferredOnly: z.boolean().optional().default(false).describe("Return only preferred suppliers")
});

export const registerPurchaseTools = (server: McpServer) => {
  server.registerTool(
    "get_inbound_pipeline",
    {
      description: "Retrieve purchase order quantities that are still inbound and not fully received.",
      inputSchema: inboundPipelineSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ warehouseIds, productIds, asOfDate }) => {
      let query = `
        SELECT
          pol."ProductId",
          p."Code" AS "ProductCode",
          p."Name" AS "ProductName",
          po."WarehouseId",
          w."Code" AS "WarehouseCode",
          w."Name" AS "WarehouseName",
          po."SupplierId",
          s."Code" AS "SupplierCode",
          s."Name" AS "SupplierName",
          po."Id" AS "PurchaseOrderId",
          po."Code" AS "PurchaseOrderCode",
          po."ExpectedDeliveryDate",
          pol."UnitId",
          bu."Code" AS "UnitCode",
          bu."Name" AS "UnitName",
          SUM(pol."Quantity") AS "OrderedQuantity",
          SUM(pol."ReceivedQuantity") AS "ReceivedQuantity",
          SUM(pol."Quantity" - pol."ReceivedQuantity") AS "RemainingInboundQuantity"
        FROM "AppPurchaseOrderLines" pol
        INNER JOIN "AppPurchaseOrders" po ON po."Id" = pol."PurchaseOrderId"
        INNER JOIN "AppProducts" p ON p."Id" = pol."ProductId"
        INNER JOIN "AppWarehouses" w ON w."Id" = po."WarehouseId"
        INNER JOIN "AppSuppliers" s ON s."Id" = po."SupplierId"
        INNER JOIN "AppBaseUnits" bu ON bu."Id" = pol."UnitId"
        WHERE po."IsDeleted" = false
          AND p."IsDeleted" = false
          AND w."IsDeleted" = false
          AND s."IsDeleted" = false
          AND pol."Quantity" > pol."ReceivedQuantity"
      `;
      const params: Array<string | string[]> = [];

      if (warehouseIds && warehouseIds.length > 0) {
        params.push(warehouseIds);
        query += ` AND po."WarehouseId" = ANY($${params.length})`;
      }

      if (productIds && productIds.length > 0) {
        params.push(productIds);
        query += ` AND pol."ProductId" = ANY($${params.length})`;
      }

      if (asOfDate) {
        params.push(asOfDate);
        query += ` AND (po."ExpectedDeliveryDate" IS NULL OR po."ExpectedDeliveryDate" >= $${params.length})`;
      }

      query += `
        GROUP BY
          pol."ProductId",
          p."Code",
          p."Name",
          po."WarehouseId",
          w."Code",
          w."Name",
          po."SupplierId",
          s."Code",
          s."Name",
          po."Id",
          po."Code",
          po."ExpectedDeliveryDate",
          pol."UnitId",
          bu."Code",
          bu."Name"
        ORDER BY po."ExpectedDeliveryDate" ASC NULLS LAST, w."Name" ASC, p."Name" ASC
      `;

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No inbound pipeline found matching the criteria." }] };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          productId: row.ProductId,
          productCode: row.ProductCode,
          productName: row.ProductName,
          warehouseId: row.WarehouseId,
          warehouseCode: row.WarehouseCode,
          warehouseName: row.WarehouseName,
          supplierId: row.SupplierId,
          supplierCode: row.SupplierCode,
          supplierName: row.SupplierName,
          purchaseOrderId: row.PurchaseOrderId,
          purchaseOrderCode: row.PurchaseOrderCode,
          expectedDeliveryDate: row.ExpectedDeliveryDate,
          unitId: row.UnitId,
          unitCode: row.UnitCode,
          unitName: row.UnitName,
          orderedQuantity: Number(row.OrderedQuantity),
          receivedQuantity: Number(row.ReceivedQuantity),
          remainingInboundQuantity: Number(row.RemainingInboundQuantity)
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
    "get_requisition_backlog",
    {
      description: "Retrieve purchase requisition quantities that are not yet fully converted into purchase orders.",
      inputSchema: purchaseFilterSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ warehouseIds, productIds }) => {
      let query = `
        SELECT
          prl."ProductId",
          p."Code" AS "ProductCode",
          p."Name" AS "ProductName",
          pr."WarehouseId",
          w."Code" AS "WarehouseCode",
          w."Name" AS "WarehouseName",
          pr."Id" AS "PurchaseRequisitionId",
          pr."Code" AS "PurchaseRequisitionCode",
          pr."RequestedDate",
          pr."RequiredDate",
          prl."UnitId",
          bu."Code" AS "UnitCode",
          bu."Name" AS "UnitName",
          SUM(prl."Quantity") AS "RequestedQuantity",
          SUM(prl."OrderedQuantity") AS "OrderedQuantity",
          SUM(prl."Quantity" - prl."OrderedQuantity") AS "RemainingToOrderQuantity"
        FROM "AppPurchaseRequisitionLines" prl
        INNER JOIN "AppPurchaseRequisitions" pr ON pr."Id" = prl."PurchaseRequisitionId"
        INNER JOIN "AppProducts" p ON p."Id" = prl."ProductId"
        INNER JOIN "AppWarehouses" w ON w."Id" = pr."WarehouseId"
        INNER JOIN "AppBaseUnits" bu ON bu."Id" = prl."UnitId"
        WHERE pr."IsDeleted" = false
          AND p."IsDeleted" = false
          AND w."IsDeleted" = false
          AND prl."Quantity" > prl."OrderedQuantity"
      `;
      const params: Array<string[]> = [];

      if (warehouseIds && warehouseIds.length > 0) {
        params.push(warehouseIds);
        query += ` AND pr."WarehouseId" = ANY($${params.length})`;
      }

      if (productIds && productIds.length > 0) {
        params.push(productIds);
        query += ` AND prl."ProductId" = ANY($${params.length})`;
      }

      query += `
        GROUP BY
          prl."ProductId",
          p."Code",
          p."Name",
          pr."WarehouseId",
          w."Code",
          w."Name",
          pr."Id",
          pr."Code",
          pr."RequestedDate",
          pr."RequiredDate",
          prl."UnitId",
          bu."Code",
          bu."Name"
        ORDER BY pr."RequiredDate" ASC NULLS LAST, w."Name" ASC, p."Name" ASC
      `;

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No purchase requisition backlog found matching the criteria." }] };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          productId: row.ProductId,
          productCode: row.ProductCode,
          productName: row.ProductName,
          warehouseId: row.WarehouseId,
          warehouseCode: row.WarehouseCode,
          warehouseName: row.WarehouseName,
          purchaseRequisitionId: row.PurchaseRequisitionId,
          purchaseRequisitionCode: row.PurchaseRequisitionCode,
          requestedDate: row.RequestedDate,
          requiredDate: row.RequiredDate,
          unitId: row.UnitId,
          unitCode: row.UnitCode,
          unitName: row.UnitName,
          requestedQuantity: Number(row.RequestedQuantity),
          orderedQuantity: Number(row.OrderedQuantity),
          remainingToOrderQuantity: Number(row.RemainingToOrderQuantity)
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
    "get_supplier_supply_options",
    {
      description: "Retrieve supplier options, purchasing units, prices, and minimum order quantities for selected products.",
      inputSchema: supplierSupplyOptionsSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ productIds, supplierIds, preferredOnly }) => {
      let query = `
        SELECT
          sp."ProductId",
          p."Code" AS "ProductCode",
          p."Name" AS "ProductName",
          sp."SupplierId",
          s."Code" AS "SupplierCode",
          s."Name" AS "SupplierName",
          sp."DefaultUnitId",
          defaultUnit."Code" AS "DefaultUnitCode",
          defaultUnit."Name" AS "DefaultUnitName",
          sp."LeadTimeDays",
          sp."IsPreferred",
          sp."IsActive",
          spc."UnitId" AS "ConditionUnitId",
          conditionUnit."Code" AS "ConditionUnitCode",
          conditionUnit."Name" AS "ConditionUnitName",
          spc."ConversionFactor",
          spc."StandardPrice",
          spc."LastPurchasePrice",
          spc."MinOrderQuantity",
          spc."OverDeliveryTolerancePct",
          spc."UnderDeliveryTolerancePct"
        FROM "AppSupplierProducts" sp
        INNER JOIN "AppSupplierProductConditions" spc ON spc."SupplierProductId" = sp."Id"
        INNER JOIN "AppProducts" p ON p."Id" = sp."ProductId"
        INNER JOIN "AppSuppliers" s ON s."Id" = sp."SupplierId"
        INNER JOIN "AppBaseUnits" defaultUnit ON defaultUnit."Id" = sp."DefaultUnitId"
        INNER JOIN "AppBaseUnits" conditionUnit ON conditionUnit."Id" = spc."UnitId"
        WHERE p."IsDeleted" = false
          AND s."IsDeleted" = false
          AND sp."IsActive" = true
          AND sp."ProductId" = ANY($1)
      `;
      const params: Array<string[] | boolean> = [productIds];

      if (supplierIds && supplierIds.length > 0) {
        params.push(supplierIds);
        query += ` AND sp."SupplierId" = ANY($${params.length})`;
      }

      if (preferredOnly) {
        query += ` AND sp."IsPreferred" = true`;
      }

      query += `
        ORDER BY
          sp."IsPreferred" DESC,
          sp."LeadTimeDays" ASC,
          spc."LastPurchasePrice" ASC,
          spc."StandardPrice" ASC,
          s."Name" ASC
      `;

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No supplier supply options found matching the criteria." }] };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          productId: row.ProductId,
          productCode: row.ProductCode,
          productName: row.ProductName,
          supplierId: row.SupplierId,
          supplierCode: row.SupplierCode,
          supplierName: row.SupplierName,
          defaultUnitId: row.DefaultUnitId,
          defaultUnitCode: row.DefaultUnitCode,
          defaultUnitName: row.DefaultUnitName,
          leadTimeDays: Number(row.LeadTimeDays),
          isPreferred: row.IsPreferred,
          isActive: row.IsActive,
          conditionUnitId: row.ConditionUnitId,
          conditionUnitCode: row.ConditionUnitCode,
          conditionUnitName: row.ConditionUnitName,
          conversionFactor: Number(row.ConversionFactor),
          standardPrice: Number(row.StandardPrice),
          lastPurchasePrice: Number(row.LastPurchasePrice),
          minOrderQuantity: Number(row.MinOrderQuantity),
          overDeliveryTolerancePct: Number(row.OverDeliveryTolerancePct),
          underDeliveryTolerancePct: Number(row.UnderDeliveryTolerancePct)
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
