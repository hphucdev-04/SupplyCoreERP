using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SupplyCoreERP.AiChats.Dtos;
using SupplyCoreERP.AiChats.Mcp;

namespace SupplyCoreERP.AiChats;

[Authorize]
public class AiChatAppService : SupplyCore, IAiChatAppService
{
    private readonly IMcpClientService _mcpClientService;

    public AiChatAppService(IMcpClientService mcpClientService)
    {
        _mcpClientService = mcpClientService;
    }

    public async Task<ChatResponseOutputDto> SendMessageAsync(ChatRequestInputDto input)
    {
        // Điều phối gửi tin nhắn và lịch sử hội thoại qua MCP Client để gọi Gemini
        string responseText = await _mcpClientService.ExecuteConversationAsync(input.Text, input.History);

        return new ChatResponseOutputDto
        {
            Text = responseText
        };
    }
}
