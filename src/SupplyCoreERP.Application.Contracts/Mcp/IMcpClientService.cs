using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SupplyCoreERP.Mcp.Dtos;

namespace SupplyCoreERP.Mcp;

public interface IMcpClientService
{
    Task<List<McpToolDto>> GetToolsAsync();

    Task<string> CallToolAsync(string toolName, JsonObject arguments);

    Task<List<McpResourceDto>> GetResourcesAsync();

    Task<string> GetServerInstructionsAsync();
}
