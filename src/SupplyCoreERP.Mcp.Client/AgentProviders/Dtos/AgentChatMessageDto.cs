namespace SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;

public class AgentChatMessageDto
{
    public string Role { get; set; }

    public string? Text { get; set; }

    public List<AgentToolCallDto> ToolCalls { get; set; } = new();

    public List<AgentToolResponseDto> ToolResponses { get; set; } = new();
}
