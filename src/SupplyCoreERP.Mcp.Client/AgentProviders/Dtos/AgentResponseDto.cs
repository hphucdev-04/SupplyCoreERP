namespace SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;

public class AgentResponseDto
{
    public string? Text { get; set; }

    public List<LlmToolCallDto> ToolCalls { get; set; } = new();

    public bool IsToolCall => ToolCalls.Count > 0;
}
