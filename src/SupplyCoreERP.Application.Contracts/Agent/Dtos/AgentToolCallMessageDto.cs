using System.Text.Json.Nodes;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentToolCallMessageDto
{
    public string Name { get; set; } = string.Empty;

    public JsonObject Arguments { get; set; } = new();

    public string? ThoughtSignature { get; set; }
}
