using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentSessionPagedInputDto : PagedResultRequestDto
{
    public Guid SessionId { get; set; }
}
