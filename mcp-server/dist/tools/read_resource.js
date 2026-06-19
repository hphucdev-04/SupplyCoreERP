import { z } from "zod";
import fs from "fs/promises";
import path from "path";
import { fileURLToPath } from "url";
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
// URI → file path mapping for all server resources
const RESOURCE_MAP = {
    "schema://database": path.resolve(__dirname, "../../resources/db_schema.md")
};
export const registerReadResourceTool = (server) => {
    server.registerTool("read_resource", {
        description: "Read the contents of a server resource by its URI.",
        inputSchema: z.object({
            uri: z.string().describe("URI of the resource to read.")
        }),
        annotations: {
            readOnlyHint: true,
            destructiveHint: false,
            idempotentHint: true,
            openWorldHint: false
        }
    }, async ({ uri }) => {
        const filePath = RESOURCE_MAP[uri];
        if (!filePath) {
            const availableUris = Object.keys(RESOURCE_MAP).join(", ");
            return {
                isError: true,
                content: [{ type: "text", text: `Unknown resource URI: '${uri}'. Available: ${availableUris}` }]
            };
        }
        try {
            const content = await fs.readFile(filePath, "utf-8");
            return {
                content: [{ type: "text", text: content }]
            };
        }
        catch (error) {
            return {
                isError: true,
                content: [{ type: "text", text: `Failed to read resource '${uri}': ${error.message}` }]
            };
        }
    });
};
