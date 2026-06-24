using System.Text.Json.Nodes;

namespace SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;

public class LlmToolCallDto
{
    public string Name { get; set; } = string.Empty;
    public JsonObject? Arguments { get; set; }
    public string? ThoughtSignature { get; set; }
}
