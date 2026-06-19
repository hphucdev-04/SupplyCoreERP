import { z } from "zod";
import { McpServer } from "@modelcontextprotocol/server";

export const registerDatetimeTools = (server: McpServer) => {
  server.registerTool(
    "get_current_datetime",
    {
      description: "Get the current system date and time.",
      inputSchema: z.object({})
    },
    async () => {
      const now = new Date();
      const offset = now.getTimezoneOffset();
      const localTime = new Date(now.getTime() - (offset * 60 * 1000));
      const formatted = localTime.toISOString().replace('T', ' ').substring(0, 19);
      return {
        content: [{ type: "text", text: formatted }]
      };
    }
  );
};
