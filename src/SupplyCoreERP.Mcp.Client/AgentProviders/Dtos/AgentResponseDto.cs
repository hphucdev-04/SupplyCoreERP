namespace SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;

public class AgentResponseDto
{
    public string? Text { get; set; }

    public List<AgentToolCallDto> ToolCalls { get; set; } = new();

    public bool IsToolCall => ToolCalls.Count > 0;
}
