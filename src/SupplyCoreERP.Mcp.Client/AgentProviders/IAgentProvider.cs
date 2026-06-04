using SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;
using SupplyCoreERP.Mcp.Dtos;

namespace SupplyCoreERP.Mcp.Client.AgentProviders;

public interface IAgentProvider
{
    Task<AgentResponseDto> GenerateContentAsync(List<AgentChatMessageDto> chatHistory, List<McpToolDto> tools);
}
