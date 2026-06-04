import { z } from "zod";
import { queryDb } from "../db.js";
export const registerWarehouseTools = (server) => {
    server.registerTool("get_warehouses", {
        description: "Retrieve the list of warehouses in the SupplyCoreERP system.",
        inputSchema: z.object({
            name: z.string().optional().describe("Warehouse name to search for"),
            code: z.string().optional().describe("Warehouse code to search for (e.g., KHO_HCM)"),
            limit: z.number().optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
        })
    }, async ({ name, code, limit }) => {
        let query = `SELECT "Id", "Code", "Name", "Address" FROM "AppWarehouses" WHERE "IsDeleted" = false`;
        const params = [];
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
                return { content: [{ type: "text", text: "No warehouses found matching the criteria." }] };
            }
            const text = rows.map(r => `Code: ${r.Code} | Name: ${r.Name} | Address: ${r.Address || 'N/A'}`).join("\n");
            return { content: [{ type: "text", text: `Warehouse List:\n${text}` }] };
        }
        catch (error) {
            return {
                isError: true,
                content: [{ type: "text", text: `Database query error: ${error.message}` }]
            };
        }
    });
};
