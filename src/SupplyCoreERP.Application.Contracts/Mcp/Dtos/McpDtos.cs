using System.Text.Json.Nodes;

namespace SupplyCoreERP.Mcp.Dtos;

public class McpToolDto
{
    public string Name { get; set; }

    public string Description { get; set; }

    public JsonObject InputSchema { get; set; }

    public bool RequiresApproval { get; set; }
}
