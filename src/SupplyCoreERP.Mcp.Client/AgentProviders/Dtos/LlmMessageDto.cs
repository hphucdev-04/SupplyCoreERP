namespace SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;

public class LlmMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string? Text { get; set; }
    public List<LlmToolCallDto>? ToolCalls { get; set; }
    public List<LlmToolResponseDto>? ToolResponses { get; set; }
}
