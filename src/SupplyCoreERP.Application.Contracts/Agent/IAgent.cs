using System.Threading.Tasks;
using SupplyCoreERP.Agent.Dtos;

namespace SupplyCoreERP.Agent;

public interface IAgent
{
    Task<AgentResultDto> RunAsync(AgentContext context);
}
