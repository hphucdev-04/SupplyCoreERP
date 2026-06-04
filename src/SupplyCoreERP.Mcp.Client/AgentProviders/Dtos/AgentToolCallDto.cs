using System.Text.Json.Nodes;

namespace SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;

public class AgentToolCallDto
{
    public string Name { get; set; }

    public JsonObject Arguments { get; set; }
}
