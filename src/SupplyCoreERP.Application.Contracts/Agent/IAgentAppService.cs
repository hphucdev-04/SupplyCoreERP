using System.Threading.Tasks;
using SupplyCoreERP.Agent.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Agent;

public interface IAgentAppService : IApplicationService
{
    Task<object> SendMessageAsync(AgentRequestInputDto input);

    Task<object> ApproveAsync(AgentSessionInputDto input);

    Task<object> RejectAsync(AgentSessionInputDto input);
}
