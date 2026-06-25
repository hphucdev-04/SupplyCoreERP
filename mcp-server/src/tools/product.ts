import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";
import { queryDb } from "../db.js";
import { sanitizeResponse } from "../utils/security.js";

const searchProductsSchema = z.object({
  name: z.string().optional().describe("Product name to search for"),
  code: z.string().optional().describe("Product code to search for"),
  limit: z.number().int().min(1).max(50).optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
});

const getProductUnitsSchema = z.object({
  productId: z.string().uuid().describe("Product Id")
});

export const registerProductTools = (server: McpServer) => {
  server.registerTool(
    "search_products",
    {
      description: "Search products by name or code and return the basic product information for planning and lookup.",
      inputSchema: searchProductsSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ name, code, limit }) => {
      let query = `
        SELECT
          p."Id",
          p."Code",
          p."Name",
          p."BaseUnitId",
          bu."Code" AS "BaseUnitCode",
          bu."Name" AS "BaseUnitName",
          p."ProductType"
        FROM "AppProducts" p
        INNER JOIN "AppBaseUnits" bu ON bu."Id" = p."BaseUnitId"
        WHERE p."IsDeleted" = false
      `;
      const params: Array<string | number> = [];

      if (name) {
        params.push(`%${name}%`);
        query += ` AND p."Name" ILIKE $${params.length}`;
      }

      if (code) {
        params.push(`%${code}%`);
        query += ` AND p."Code" ILIKE $${params.length}`;
      }

      query += ` ORDER BY p."Name" ASC LIMIT $${params.length + 1}`;
      params.push(limit);

      try {
        const rows = await queryDb(query, params);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No products found matching the criteria." }] };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          productId: row.Id,
          productCode: row.Code,
          productName: row.Name,
          baseUnitId: row.BaseUnitId,
          baseUnitCode: row.BaseUnitCode,
          baseUnitName: row.BaseUnitName,
          productType: row.ProductType
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
    "get_product_units",
    {
      description: "Retrieve all configured units and conversion factors for a product.",
      inputSchema: getProductUnitsSchema,
      annotations: {
        readOnlyHint: true,
        destructiveHint: false,
        idempotentHint: true,
        openWorldHint: false
      }
    },
    async ({ productId }) => {
      const query = `
        SELECT
          pu."Id",
          pu."ProductId",
          pu."UnitId",
          pu."ConversionFactor",
          pu."Level",
          pu."Volume",
          bu."Code" AS "UnitCode",
          bu."Name" AS "UnitName"
        FROM "AppProductUnits" pu
        INNER JOIN "AppBaseUnits" bu ON bu."Id" = pu."UnitId"
        WHERE pu."ProductId" = $1
        ORDER BY pu."Level" ASC, pu."ConversionFactor" ASC
      `;

      try {
        const rows = await queryDb(query, [productId]);
        if (rows.length === 0) {
          return { content: [{ type: "text", text: "No product units found for the specified product." }] };
        }

        const sanitizedRows = sanitizeResponse(rows);
        const items = sanitizedRows.map((row) => ({
          productUnitId: row.Id,
          productId: row.ProductId,
          unitId: row.UnitId,
          unitCode: row.UnitCode,
          unitName: row.UnitName,
          conversionFactor: row.ConversionFactor,
          level: row.Level,
          volume: row.Volume
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
