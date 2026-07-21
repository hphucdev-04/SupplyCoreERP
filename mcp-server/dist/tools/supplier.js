import { z } from "zod";
import { queryDb } from "../db.js";
import { sanitizeResponse } from "../utils/security.js";
import crypto from "crypto";
import { elicitInput } from "../utils/elicitation.js";
const SUPPLIER_DOCUMENT_TYPE = "SP";
const formatSequenceDate = () => {
    const now = new Date();
    const year = now.getFullYear().toString().slice(-2);
    const month = (now.getMonth() + 1).toString().padStart(2, "0");
    const day = now.getDate().toString().padStart(2, "0");
    return `${year}${month}${day}`;
};
const generateSupplierCodeAsync = async () => {
    const todayStr = formatSequenceDate();
    const sequenceRows = await queryDb(`SELECT "Id", "PrefixDate", "LastValue"
     FROM "AppDocumentSequences"
     WHERE "DocumentType" = $1
     LIMIT 1`, [SUPPLIER_DOCUMENT_TYPE]);
    if (sequenceRows.length === 0) {
        const sequenceId = crypto.randomUUID();
        const concurrencyStamp = crypto.randomUUID();
        await queryDb(`INSERT INTO "AppDocumentSequences" (
        "Id", "DocumentType", "PrefixDate", "LastValue", "ExtraProperties", "ConcurrencyStamp"
      ) VALUES (
        $1, $2, $3, $4, '{}', $5
      )`, [sequenceId, SUPPLIER_DOCUMENT_TYPE, todayStr, 1, concurrencyStamp]);
        return `${SUPPLIER_DOCUMENT_TYPE}${todayStr}0001`;
    }
    const sequence = sequenceRows[0];
    const nextValue = sequence.PrefixDate !== todayStr ? 1 : Number(sequence.LastValue) + 1;
    await queryDb(`UPDATE "AppDocumentSequences"
     SET "PrefixDate" = $1,
         "LastValue" = $2
     WHERE "Id" = $3`, [todayStr, nextValue, sequence.Id]);
    return `${SUPPLIER_DOCUMENT_TYPE}${todayStr}${nextValue.toString().padStart(4, "0")}`;
};
const validateSupplierLocationAsync = async (countryId, cityId, areaId) => {
    if (countryId) {
        const countryRows = await queryDb(`SELECT "Id" FROM "AppCountries" WHERE "Id" = $1 AND "IsDeleted" = false LIMIT 1`, [countryId]);
        if (countryRows.length === 0) {
            throw new Error("Country not found.");
        }
    }
    if (cityId) {
        const cityRows = await queryDb(`SELECT "Id", "CountryId" FROM "AppCities" WHERE "Id" = $1 AND "IsDeleted" = false LIMIT 1`, [cityId]);
        if (cityRows.length === 0) {
            throw new Error("City not found.");
        }
        if (countryId && cityRows[0].CountryId !== countryId) {
            throw new Error("City does not belong to the selected country.");
        }
    }
    if (areaId) {
        const areaRows = await queryDb(`SELECT "Id", "CityId" FROM "AppAreas" WHERE "Id" = $1 AND "IsDeleted" = false LIMIT 1`, [areaId]);
        if (areaRows.length === 0) {
            throw new Error("Area not found.");
        }
        if (cityId && areaRows[0].CityId !== cityId) {
            throw new Error("Area does not belong to the selected city.");
        }
    }
};
const validateSupplierUniquenessAsync = async (code, name) => {
    const duplicateCodeRows = await queryDb(`SELECT "Id" FROM "AppSuppliers" WHERE "Code" = $1 AND "IsDeleted" = false LIMIT 1`, [code]);
    if (duplicateCodeRows.length > 0) {
        throw new Error(`Supplier code '${code}' already exists.`);
    }
    const duplicateNameRows = await queryDb(`SELECT "Id" FROM "AppSuppliers" WHERE "Name" = $1 AND "IsDeleted" = false LIMIT 1`, [name]);
    if (duplicateNameRows.length > 0) {
        throw new Error(`Supplier name '${name}' already exists.`);
    }
};
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
        description: "Create a new supplier. ALWAYS invoke this tool immediately with just the supplier name. DO NOT ask the user for code or other detailed fields in the chat; the tool's form will collect them automatically and the supplier code will be generated by document sequence.",
        inputSchema: z.object({
            name: z.string().describe("The name of the supplier to create"),
            taxCode: z.string().optional().describe("Tax code of the supplier"),
            phoneNumber: z.string().optional().describe("Phone number of the supplier"),
            email: z.string().optional().describe("Email address of the supplier"),
            representativeName: z.string().optional().describe("Representative name of the supplier"),
            gender: z.number().int().min(0).max(1).optional().describe("Gender enum value: 0 = Male, 1 = Female"),
            note: z.string().optional().describe("Internal note for the supplier"),
            address: z.string().optional().describe("Physical address of the supplier"),
            countryId: z.string().uuid().optional().describe("Country Id"),
            cityId: z.string().uuid().optional().describe("City Id"),
            areaId: z.string().uuid().optional().describe("Area Id"),
            debtLimit: z.number().min(0).optional().describe("Debt limit amount"),
            paymentTermDays: z.number().int().min(0).optional().describe("Payment term days"),
            isActive: z.boolean().optional().describe("Supplier active status")
        }),
        annotations: {
            readOnlyHint: false,
            destructiveHint: false,
            idempotentHint: false,
            openWorldHint: false
        }
    }, async ({ name, taxCode, phoneNumber, email, representativeName, gender, note, address, countryId, cityId, areaId, debtLimit, paymentTermDays, isActive }) => {
        // 1. Kiểm tra Elicitation bằng Helper chuẩn MCP
        const result = await elicitInput(server, {
            mode: "form",
            message: "Vui lòng nhập các thông tin chi tiết dưới đây để hoàn tất hồ sơ tạo nhà cung cấp mới.",
            requestedSchema: {
                type: "object",
                properties: {
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
                    representativeName: {
                        type: "string",
                        title: "Người đại diện",
                        description: "Ví dụ: Nguyen Van A"
                    },
                    gender: {
                        type: "integer",
                        title: "Giới tính người đại diện",
                        description: "0 = Nam, 1 = Nữ"
                    },
                    note: {
                        type: "string",
                        title: "Ghi chú",
                        description: "Ghi chú nội bộ cho nhà cung cấp"
                    },
                    address: {
                        type: "string",
                        title: "Địa chỉ văn phòng",
                        description: "Ví dụ: 123 Đường Nguyễn Huệ, Quận 1, TP.HCM"
                    },
                    countryId: {
                        type: "string",
                        title: "Country Id",
                        description: "GUID quốc gia, ví dụ dùng dữ liệu từ hệ thống location"
                    },
                    cityId: {
                        type: "string",
                        title: "City Id",
                        description: "GUID tỉnh/thành phố"
                    },
                    areaId: {
                        type: "string",
                        title: "Area Id",
                        description: "GUID quận/huyện/khu vực"
                    },
                    debtLimit: {
                        type: "number",
                        title: "Hạn mức công nợ",
                        description: "Nhập 0 nếu không giới hạn"
                    },
                    paymentTermDays: {
                        type: "integer",
                        title: "Số ngày thanh toán",
                        description: "Ví dụ: 30"
                    },
                    isActive: {
                        type: "boolean",
                        title: "Kích hoạt",
                        description: "true = hoạt động, false = ngừng hoạt động"
                    }
                },
                required: ["taxCode", "phoneNumber"]
            }
        }, {
            taxCode,
            phoneNumber,
            email,
            representativeName,
            gender,
            note,
            address,
            countryId,
            cityId,
            areaId,
            debtLimit,
            paymentTermDays,
            isActive
        });
        // Gán lại các đối số thu thập được từ Form ở lần gọi 2 để chạy tiếp logic ghi DB
        name = name?.trim();
        taxCode = result.content.taxCode;
        phoneNumber = result.content.phoneNumber;
        email = result.content.email;
        representativeName = result.content.representativeName;
        gender = result.content.gender;
        note = result.content.note;
        address = result.content.address;
        countryId = result.content.countryId;
        cityId = result.content.cityId;
        areaId = result.content.areaId;
        debtLimit = result.content.debtLimit;
        paymentTermDays = result.content.paymentTermDays;
        isActive = result.content.isActive;
        // 2. Thực hiện insert dữ liệu vào bảng AppSuppliers
        const newId = crypto.randomUUID();
        const concurrencyStamp = crypto.randomUUID();
        const code = await generateSupplierCodeAsync();
        if (!name) {
            return {
                isError: true,
                content: [{ type: "text", text: "Supplier name is required." }]
            };
        }
        const insertQuery = `
        INSERT INTO "AppSuppliers" (
          "Id", "Code", "Name", "TaxCode", "PhoneNumber", "Email", "RepresentativeName", "Note",
          "IsActive", "Address", "DebtLimit", "PaymentTermDays", "CurrentDebt",
          "CountryId", "CityId", "AreaId", "ExtraProperties", "ConcurrencyStamp",
          "CreationTime", "IsDeleted", "Gender"
        ) VALUES (
          $1, $2, $3, $4, $5, $6, $7, $8,
          $9, $10, $11, $12, 0,
          $13, $14, $15, '{}', $16,
          NOW(), false, $17
        )
      `;
        try {
            await validateSupplierLocationAsync(countryId, cityId, areaId);
            await validateSupplierUniquenessAsync(code, name);
            await queryDb(insertQuery, [
                newId,
                code,
                name,
                taxCode || null,
                phoneNumber || null,
                email || null,
                representativeName || null,
                note || null,
                isActive ?? true,
                address || null,
                debtLimit ?? 0,
                paymentTermDays ?? 0,
                countryId || null,
                cityId || null,
                areaId || null,
                concurrencyStamp,
                gender ?? null
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
                                representativeName: representativeName || null,
                                gender: gender ?? null,
                                note: note || null,
                                address: address || null,
                                countryId: countryId || null,
                                cityId: cityId || null,
                                areaId: areaId || null,
                                debtLimit: debtLimit ?? 0,
                                paymentTermDays: paymentTermDays ?? 0,
                                isActive: isActive ?? true
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
