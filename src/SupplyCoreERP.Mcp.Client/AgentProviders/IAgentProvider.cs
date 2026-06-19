using SupplyCoreERP.Mcp.Client.AgentProviders.Dtos;
using SupplyCoreERP.Mcp.Dtos;

namespace SupplyCoreERP.Mcp.Client.AgentProviders;

public interface IAgentProvider
{
    Task<AgentResponseDto> GenerateContentAsync(List<LlmMessageDto> chatHistory, List<McpToolDto> tools, List<McpResourceDto> resources, string? systemInstruction = null);
}
