import fs from "fs";
import path from "path";
import { queryDb } from "../db.js";
import { fileURLToPath } from "url";
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const blacklistPath = path.resolve(__dirname, "../../query_blacklist.json");
let blacklistTables = [];
let blacklistColumns = [];
try {
    const blacklistRaw = fs.readFileSync(blacklistPath, "utf-8");
    const blacklistData = JSON.parse(blacklistRaw);
    blacklistTables = (blacklistData.tables || []).map((t) => t.toLowerCase());
    blacklistColumns = (blacklistData.columns || []).map((c) => c.toLowerCase());
}
catch (error) {
    console.error("[MCP-Server] Failed to load query_blacklist.json, using defaults:", error);
    blacklistColumns = ["password", "hash", "salt", "secret", "token", "key", "credential", "auth"];
}
const EMAIL_REGEX = /[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}/g;
const PHONE_REGEX = /(?:\+84|0)[35789]\d{8}\b/g;
const TAXCODE_REGEX = /\b\d{10}(?:-\d{3})?\b/g;
const ABP_AUDIT_COLUMNS = [
    "CreatorId", "LastModifierId", "DeleterId",
    "IsDeleted", "DeletionTime",
    "ExtraProperties", "ConcurrencyStamp", "TenantId"
];
/**
 * Remove comments and replace string literals to avoid false matches.
 */
function cleanSqlString(sql) {
    let clean = sql.replace(/--.*$/gm, "");
    clean = clean.replace(/\/\*[\s\S]*?\*\//g, "");
    clean = clean.replace(/'(?:''|[^'])*'/g, "''");
    return clean;
}
/**
 * Parse CTE names defined in WITH clause.
 */
function getCteNames(cleanSql) {
    const cteNames = new Set();
    const cteRegex = /\b(?:WITH|,\s*)([a-zA-Z0-9_"]+)\s+AS\s*\(/gi;
    let match;
    while ((match = cteRegex.exec(cleanSql)) !== null) {
        cteNames.add(match[1].replace(/"/g, "").toLowerCase());
    }
    return cteNames;
}
/**
 * Extract table names referenced in FROM and JOIN clauses.
 */
function extractTableNames(cleanSql) {
    const tables = [];
    const fromJoinRegex = /\b(?:FROM|JOIN)\s+([^;()]+?)(?:\b(?:WHERE|ON|GROUP|HAVING|ORDER|LIMIT|OFFSET|JOIN|UNION|INTERSECT|EXCEPT|SELECT)|$|\))/gi;
    let match;
    while ((match = fromJoinRegex.exec(cleanSql)) !== null) {
        const clause = match[1];
        const tableParts = clause.split(",");
        for (const part of tableParts) {
            const trimmed = part.trim();
            if (!trimmed)
                continue;
            const firstTokenMatch = trimmed.match(/^(?:"[^"]+"|[a-zA-Z0-9_.]+)+/);
            if (firstTokenMatch) {
                let tableName = firstTokenMatch[0];
                if (tableName.includes(".")) {
                    const parts = tableName.split(".");
                    tableName = parts[parts.length - 1];
                }
                tableName = tableName.replace(/"/g, "");
                tables.push(tableName);
            }
        }
    }
    return tables;
}
// ============================================================
// Layer 1: SQL Input Validation
// ============================================================
/**
 * Validate a SQL query for read-only safety, blacklist compliance, and star projection.
 */
export async function validateSqlQuery(sql) {
    const cleanSql = cleanSqlString(sql);
    // Must start with SELECT or WITH
    const trimmedClean = cleanSql.trim();
    if (!/^(?:SELECT|WITH)\b/i.test(trimmedClean)) {
        return {
            isValid: false,
            errorReason: "Only SELECT or WITH queries are allowed. Write operations are strictly forbidden.",
            hasStar: false,
            tables: []
        };
    }
    // Block destructive keywords
    const destructiveRegex = /\b(INSERT|UPDATE|DELETE|DROP|TRUNCATE|ALTER|CREATE|REPLACE|GRANT|REVOKE)\b/i;
    if (destructiveRegex.test(cleanSql)) {
        return {
            isValid: false,
            errorReason: "Query contains forbidden destructive or DDL keywords.",
            hasStar: false,
            tables: []
        };
    }
    // Block blacklisted column keywords
    const words = cleanSql.match(/[a-zA-Z0-9_]+/g) || [];
    for (const word of words) {
        const wordLower = word.toLowerCase();
        if (blacklistColumns.some(col => wordLower.includes(col))) {
            return {
                isValid: false,
                errorReason: `Validation Error: Accessing potential sensitive column or keyword containing '${word}' is restricted for security reasons.`,
                hasStar: false,
                tables: []
            };
        }
    }
    // Extract CTE names and referenced tables, then check table blacklist
    const ctes = getCteNames(cleanSql);
    const tables = extractTableNames(cleanSql);
    for (const table of tables) {
        const tableLower = table.toLowerCase();
        if (ctes.has(tableLower)) {
            continue;
        }
        if (blacklistTables.includes(tableLower)) {
            return {
                isValid: false,
                errorReason: `Validation Error: Table access to '${table}' is restricted for security reasons.`,
                hasStar: false,
                tables: []
            };
        }
    }
    // Detect star projection (excluding COUNT(*))
    const sqlWithoutCount = cleanSql.replace(/count\s*\(\s*\*\s*\)/gi, "");
    const hasStar = /\*/.test(sqlWithoutCount);
    return { isValid: true, hasStar, tables };
}
/**
 * Check if SELECT * would expose sensitive columns in the target tables.
 */
export async function checkSelectStarSensitiveColumns(tables) {
    const columnsQuery = `
    SELECT column_name 
    FROM information_schema.columns 
    WHERE table_schema = 'public' 
      AND table_name = ANY($1)
  `;
    const rows = await queryDb(columnsQuery, [tables]);
    for (const row of rows) {
        const colName = row.column_name.toLowerCase();
        if (blacklistColumns.some(col => colName.includes(col))) {
            return {
                allowed: false,
                reason: `'SELECT *' is rejected because the target table contains potentially sensitive columns (e.g. '${row.column_name}'). Please specify columns explicitly.`
            };
        }
    }
    return { allowed: true };
}
// ============================================================
// Layer 2: Response Sanitization
// ============================================================
/**
 * Remove ABP audit/infrastructure columns from query results.
 */
function stripAbpColumns(rows) {
    if (!Array.isArray(rows))
        return [];
    return rows.map(row => {
        const clean = { ...row };
        for (const col of ABP_AUDIT_COLUMNS) {
            delete clean[col];
            const lowerCol = col.toLowerCase();
            if (lowerCol in clean) {
                delete clean[lowerCol];
            }
        }
        return clean;
    });
}
/**
 * Recursively redact sensitive data: blacklisted keys → [REDACTED_DATA], PII patterns → [REDACTED_*].
 */
function redactSensitiveData(val, keyName) {
    if (val === null || val === undefined)
        return val;
    if (keyName) {
        const lowerKey = keyName.toLowerCase();
        if (blacklistColumns.some(col => lowerKey.includes(col))) {
            return "[REDACTED_DATA]";
        }
    }
    if (typeof val === "string") {
        let sanitized = val;
        sanitized = sanitized.replace(EMAIL_REGEX, "[REDACTED_EMAIL]");
        sanitized = sanitized.replace(PHONE_REGEX, "[REDACTED_PHONE]");
        sanitized = sanitized.replace(TAXCODE_REGEX, "[REDACTED_TAXCODE]");
        return sanitized;
    }
    if (Array.isArray(val)) {
        return val.map(item => redactSensitiveData(item, keyName));
    }
    if (typeof val === "object") {
        const cleanObj = {};
        for (const k of Object.keys(val)) {
            cleanObj[k] = redactSensitiveData(val[k], k);
        }
        return cleanObj;
    }
    return val;
}
/**
 * Full sanitization pipeline: strip ABP columns → redact blacklisted keys → redact PII.
 */
export function sanitizeResponse(rows) {
    const stripped = stripAbpColumns(rows);
    return redactSensitiveData(stripped);
}
