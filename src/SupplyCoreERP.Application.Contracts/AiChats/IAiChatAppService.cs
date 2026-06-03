using System.Threading.Tasks;
using SupplyCoreERP.AiChats.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.AiChats;

public interface IAiChatAppService : IApplicationService
{
    Task<ChatResponseOutputDto> SendMessageAsync(ChatRequestInputDto input);
}
