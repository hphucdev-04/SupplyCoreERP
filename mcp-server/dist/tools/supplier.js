import { z } from "zod";
import { queryDb } from "../db.js";
import { sanitizeResponse } from "../utils/security.js";
import crypto from "crypto";
import { elicitInput } from "../utils/elicitation.js";
export const registerSupplierTools = (server) => {
    // Lấy nhà cung cấp
    server.registerTool("get_suppliers", {
        description: "Retrieve the list of suppliers in the SupplyCoreERP system.",
        inputSchema: z.object({
            name: z.string().optional().describe("Supplier name to search for"),
            code: z.string().optional().describe("Supplier code to search for"),
            limit: z.number().optional().default(10).describe("Maximum number of rows to retrieve (default 10, max 50)")
        }),
        annotations: {
            readOnlyHint: true,
            destructiveHint: false,
            idempotentHint: true,
            openWorldHint: false
        }
    }, async ({ name, code, limit }) => {
        let query = `SELECT "Id", "Code", "Name", "PhoneNumber", "Email" FROM "AppSuppliers" WHERE "IsDeleted" = false`;
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
                return { content: [{ type: "text", text: "No suppliers found matching the criteria." }] };
            }
            const sanitizedRows = sanitizeResponse(rows);
            return {
                content: [{
                        type: "text",
                        text: JSON.stringify(sanitizedRows)
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
    // Tool tạo nhà cung cấp mới hỗ trợ Elicitation Form
    server.registerTool("create_supplier", {
        description: "Create a new supplier. ALWAYS invoke this tool immediately with just the supplier name. DO NOT ask the user for code, taxCode, phone, email, or address in the chat; the tool's form will collect them automatically.",
        inputSchema: z.object({
            name: z.string().describe("The name of the supplier to create"),
            code: z.string().optional().describe("The unique code of the supplier"),
            taxCode: z.string().optional().describe("Tax code of the supplier"),
            phoneNumber: z.string().optional().describe("Phone number of the supplier"),
            email: z.string().optional().describe("Email address of the supplier"),
            address: z.string().optional().describe("Physical address of the supplier")
        }),
        annotations: {
            readOnlyHint: false,
            destructiveHint: false,
            idempotentHint: false,
            openWorldHint: false
        }
    }, async ({ name, code, taxCode, phoneNumber, email, address }) => {
        // 1. Kiểm tra Elicitation bằng Helper chuẩn MCP
        const result = await elicitInput(server, {
            mode: "form",
            message: "Vui lòng nhập các thông tin chi tiết dưới đây để hoàn tất hồ sơ tạo nhà cung cấp mới.",
            requestedSchema: {
                type: "object",
                properties: {
                    code: {
                        type: "string",
                        title: "Mã nhà cung cấp (Bắt buộc)",
                        description: "Ví dụ: NCC001"
                    },
                    taxCode: {
                        type: "string",
                        title: "Mã số thuế",
                        description: "Ví dụ: 0102030405"
                    },
                    phoneNumber: {
                        type: "string",
                        title: "Số điện thoại liên hệ",
                        description: "Ví dụ: 0901234567"
                    },
                    email: {
                        type: "string",
                        title: "Địa chỉ Email",
                        description: "Ví dụ: partner@company.com"
                    },
                    address: {
                        type: "string",
                        title: "Địa chỉ văn phòng",
                        description: "Ví dụ: 123 Đường Nguyễn Huệ, Quận 1, TP.HCM"
                    }
                },
                required: ["code"]
            }
        }, { code, taxCode, phoneNumber, email, address });
        // Gán lại các đối số thu thập được từ Form ở lần gọi 2 để chạy tiếp logic ghi DB
        code = result.content.code;
        taxCode = result.content.taxCode;
        phoneNumber = result.content.phoneNumber;
        email = result.content.email;
        address = result.content.address;
        // 2. Thực hiện insert dữ liệu vào bảng AppSuppliers
        const newId = crypto.randomUUID();
        const concurrencyStamp = crypto.randomUUID();
        const insertQuery = `
        INSERT INTO "AppSuppliers" (
          "Id", "Code", "Name", "TaxCode", "PhoneNumber", "Email", "Address",
          "IsActive", "DebtLimit", "PaymentTermDays", "CurrentDebt",
          "ExtraProperties", "ConcurrencyStamp", "CreationTime", "IsDeleted"
        ) VALUES (
          $1, $2, $3, $4, $5, $6, $7,
          true, 0, 0, 0,
          '{}', $8, NOW(), false
        )
      `;
        try {
            await queryDb(insertQuery, [
                newId,
                code,
                name,
                taxCode || null,
                phoneNumber || null,
                email || null,
                address || null,
                concurrencyStamp
            ]);
            return {
                content: [{
                        type: "text",
                        text: JSON.stringify({
                            success: true,
                            message: "Supplier created successfully",
                            supplier: {
                                id: newId,
                                code: code,
                                name: name,
                                taxCode: taxCode || null,
                                phoneNumber: phoneNumber || null,
                                email: email || null,
                                address: address || null
                            }
                        })
                    }]
            };
        }
        catch (error) {
            return {
                isError: true,
                content: [{ type: "text", text: `Database insert error: ${error.message}` }]
            };
        }
    });
};
