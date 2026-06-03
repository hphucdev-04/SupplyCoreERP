import { queryDb } from "./db.js";
/**
 * Trả về danh sách định nghĩa cấu trúc của các Tools (bao gồm Specific và Generic Tools) gửi cho AI
 */
export const getToolsDefinition = () => {
    return [
        // 1. SPECIFIC TOOL: Nghiệp vụ chi tiết tồn kho
        {
            name: "get_inventory_balance",
            description: "Tra cứu số lượng tồn kho thực tế của sản phẩm theo mã sản phẩm (productCode) và mã kho (warehouseCode - tùy chọn).",
            inputSchema: {
                type: "object",
                properties: {
                    productCode: {
                        type: "string",
                        description: "Mã code của sản phẩm cần tra cứu (ví dụ: SP001, MEDICINE002)"
                    },
                    warehouseCode: {
                        type: "string",
                        description: "Mã code của kho hàng cần lọc (ví dụ: KHO_HCM, WH_DEFAULT). Nếu để trống, sẽ tra cứu trên toàn bộ các kho."
                    }
                },
                required: ["productCode"]
            }
        },
        // 2. GENERIC TOOL: Tra cứu danh mục dùng chung (Catalog / Master Data)
        {
            name: "query_generic_data",
            description: "Truy vấn danh sách dữ liệu từ các danh mục cơ bản của hệ thống như Nhà cung cấp (supplier), Khách hàng (customer), Sản phẩm (product), Đơn vị tính (unit), Lô sản phẩm (batch), Kho hàng (warehouse). Hỗ trợ lọc theo từ khóa tìm kiếm.",
            inputSchema: {
                type: "object",
                properties: {
                    entityType: {
                        type: "string",
                        enum: ["supplier", "customer", "product", "unit", "batch", "warehouse"],
                        description: "Loại đối tượng danh mục cần tra cứu."
                    },
                    searchKeyword: {
                        type: "string",
                        description: "Từ khóa tìm kiếm nhanh theo Tên hoặc Mã code hoặc Số điện thoại (tùy chọn)."
                    },
                    limit: {
                        type: "number",
                        description: "Số lượng dòng tối đa cần lấy (mặc định là 10, tối đa 50)."
                    }
                },
                required: ["entityType"]
            }
        }
    ];
};
/**
 * Thực thi logic nghiệp vụ và truy vấn Database khi AI yêu cầu gọi Tool
 * @param name Tên tool được gọi
 * @param args Các đối số truyền vào từ AI
 */
export const executeTool = async (name, args) => {
    // --- HỒ SƠ 1: SPECIFIC TOOL - get_inventory_balance ---
    if (name === "get_inventory_balance") {
        const { productCode, warehouseCode } = args;
        let query = `
      SELECT w."Name" as "WarehouseName", b."Quantity", p."Name" as "ProductName"
      FROM "AppInventoryBalances" b
      JOIN "AppProducts" p ON b."ProductId" = p."Id"
      JOIN "AppWarehouses" w ON b."WarehouseId" = w."Id"
      WHERE p."Code" = $1
    `;
        const params = [productCode];
        if (warehouseCode) {
            query += ` AND w."Code" = $2`;
            params.push(warehouseCode);
        }
        try {
            const rows = await queryDb(query, params);
            if (rows.length === 0) {
                return {
                    content: [{
                            type: "text",
                            text: `Không tìm thấy thông tin tồn kho cho sản phẩm có mã '${productCode}'` + (warehouseCode ? ` tại kho '${warehouseCode}'.` : ".")
                        }]
                };
            }
            const resultText = rows
                .map((r) => `Sản phẩm: ${r.ProductName} | Kho: ${r.WarehouseName} | Số lượng tồn: ${Number(r.Quantity).toLocaleString('vi-VN')}`)
                .join("\n");
            return {
                content: [{ type: "text", text: resultText }]
            };
        }
        catch (error) {
            console.error('[MCP Server] Error executing tool get_inventory_balance:', error);
            return {
                isError: true,
                content: [{ type: "text", text: `Lỗi truy vấn database: ${error.message}` }]
            };
        }
    }
    // --- HỒ SƠ 2: GENERIC TOOL - query_generic_data ---
    if (name === "query_generic_data") {
        const { entityType, searchKeyword, limit = 10 } = args;
        // Khống chế số dòng tối đa để bảo vệ hiệu năng Database
        const finalLimit = Math.min(limit, 50);
        // Whitelist cấu hình các bảng và cột được phép truy xuất an toàn (Không SELECT *)
        const whitelist = {
            supplier: {
                table: "AppSuppliers",
                searchCols: ["Name", "Code"],
                selectCols: ["Id", "Code", "Name", "PhoneNumber", "Email"]
            },
            customer: {
                table: "AppCustomers",
                searchCols: ["Name", "Code", "PhoneNumber"],
                selectCols: ["Id", "Code", "Name", "PhoneNumber"]
            },
            product: {
                table: "AppProducts",
                searchCols: ["Name", "Code"],
                selectCols: ["Id", "Code", "Name", "BaseUnitId"]
            },
            unit: {
                table: "AppBaseUnits",
                searchCols: ["Name", "Code"],
                selectCols: ["Id", "Code", "Name"]
            },
            batch: {
                table: "AppProductBatches",
                searchCols: ["BatchNumber", "Code"],
                selectCols: ["Id", "Code", "BatchNumber", "ExpiryDate", "Status"]
            },
            warehouse: {
                table: "AppWarehouses",
                searchCols: ["Name", "Code"],
                selectCols: ["Id", "Code", "Name", "Address"]
            }
        };
        const config = whitelist[entityType];
        if (!config) {
            throw new Error(`Thực thể '${entityType}' không thuộc danh mục được phép truy cập.`);
        }
        // Xây dựng câu lệnh SQL an toàn (Tự động thêm bộ lọc IsDeleted = false theo chuẩn ABP)
        const selectFields = config.selectCols.map(c => `"${c}"`).join(", ");
        let query = `SELECT ${selectFields} FROM "${config.table}" WHERE "IsDeleted" = false`;
        const params = [];
        if (searchKeyword) {
            // Dùng ILIKE trong PostgreSQL để tìm kiếm không phân biệt hoa thường
            const searchConditions = config.searchCols
                .map((col, idx) => `"${col}" ILIKE $1`)
                .join(" OR ");
            query += ` AND (${searchConditions})`;
            params.push(`%${searchKeyword}%`);
        }
        query += ` LIMIT $${params.length + 1}`;
        params.push(finalLimit);
        try {
            const rows = await queryDb(query, params);
            if (rows.length === 0) {
                return {
                    content: [{
                            type: "text",
                            text: `Không tìm thấy dữ liệu nào cho danh mục '${entityType}'` + (searchKeyword ? ` khớp với từ khóa '${searchKeyword}'.` : ".")
                        }]
                };
            }
            // Format kết quả trả về dạng chuỗi text thuộc tính rõ ràng cho AI
            const resultText = rows.map((r) => {
                return Object.entries(r)
                    .map(([key, val]) => `${key}: ${val}`)
                    .join(" | ");
            }).join("\n");
            return {
                content: [{
                        type: "text",
                        text: `Danh sách danh mục ${entityType} tìm thấy:\n${resultText}`
                    }]
            };
        }
        catch (error) {
            console.error(`[MCP Server] Error executing tool query_generic_data for ${entityType}:`, error);
            return {
                isError: true,
                content: [{ type: "text", text: `Lỗi truy cập dữ liệu danh mục: ${error.message}` }]
            };
        }
    }
    throw new Error(`Tool '${name}' không được hỗ trợ trên MCP Server.`);
};
