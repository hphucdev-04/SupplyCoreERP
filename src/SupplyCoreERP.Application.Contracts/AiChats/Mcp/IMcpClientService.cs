using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.AiChats.Dtos;

namespace SupplyCoreERP.AiChats.Mcp;

public interface IMcpClientService
{
    /// <summary>
    /// Điều phối gửi tin nhắn và lịch sử trò chuyện sang Gemini API,
    /// đồng thời tự động gọi các Tools trên Python/Node.js MCP Server nếu Gemini yêu cầu.
    /// </summary>
    Task<string> ExecuteConversationAsync(string userMessage, List<ChatMessageDto> history);
}
