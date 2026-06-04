import { z } from "zod";
export const registerPrompts = (server) => {
    server.registerPrompt("analyze_inventory_balance", {
        description: "Prompt scenario guiding the AI to analyze and report inventory status",
        argsSchema: {
            productName: z.string().describe("The name or code of the product/medicine to analyze")
        }
    }, async ({ productName }) => {
        return {
            description: `Scenario guiding the AI to analyze and report inventory status for product: ${productName}`,
            messages: [{
                    role: "user",
                    content: {
                        type: "text",
                        text: `You are a professional supply chain and warehouse management expert for SupplyCoreERP.
                  Please perform the following steps to help me analyze the inventory of the product/medicine: "${productName}":

                  1. Use the appropriate tool to look up the list of products matching this name or code to find its official Code.
                  2. Use the tool to look up the physical stock inventory of that product code across all warehouses in the system.
                  3. Consolidate the figures and analyze:
                    - What is the total inventory quantity of this product across the entire system?
                    - Which warehouse holds the largest inventory quantity (posing a risk of surplus or tied-up capital)?
                    - Which warehouse has the lowest inventory quantity or is out of stock (posing a risk of supply chain disruption)?
                  4. Provide specific recommendations regarding product redistribution or ordering more stock from a suitable supplier (if necessary).`
                    }
                }]
        };
    });
};
