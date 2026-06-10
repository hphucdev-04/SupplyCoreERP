using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Agent.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Agent;

public interface IAgentAppService : IApplicationService
{
    Task<object> SendMessageAsync(AgentRequestInputDto input);

    Task<object> ApproveAsync(AgentSessionInputDto input);

    Task<object> RejectAsync(AgentSessionInputDto input);

    Task<object> SubmitElicitationAsync(AgentElicitationInputDto input);

    Task<AgentHistoryDto> GetHistoryAsync(AgentSessionInputDto input);
}
