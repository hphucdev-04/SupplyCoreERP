import { z } from "zod";
export const registerGetCurrentDateTimeTool = (server) => {
    server.registerTool("get_current_datetime", {
        description: "Get the current date and time of the system. Use this tool when you need to know today's date, current year, month, or time for querying or filtering data.",
        inputSchema: z.object({}),
    }, async () => {
        const now = new Date();
        const offset = now.getTimezoneOffset();
        const localTime = new Date(now.getTime() - offset * 60 * 1000);
        const formatted = localTime
            .toISOString()
            .replace("T", " ")
            .substring(0, 19);
        return {
            content: [{ type: "text", text: formatted }],
        };
    });
};
