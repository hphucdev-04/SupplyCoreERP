const SENSITIVE_COLUMNS = [
    "CreatorId", "LastModifierId", "DeleterId",
    "IsDeleted", "DeletionTime",
    "ExtraProperties", "ConcurrencyStamp", "TenantId",
    "creatorid", "lastmodifierid", "deleterid",
    "isdeleted", "deletiontime",
    "extraproperties", "concurrencystamp", "tenantid"
];
export function sanitizeRows(rows) {
    if (!Array.isArray(rows))
        return [];
    return rows.map(row => {
        const clean = { ...row };
        for (const col of SENSITIVE_COLUMNS) {
            delete clean[col];
            // Hỗ trợ xóa các cột viết thường
            const lowerCol = col.toLowerCase();
            if (lowerCol in clean) {
                delete clean[lowerCol];
            }
        }
        return clean;
    });
}
