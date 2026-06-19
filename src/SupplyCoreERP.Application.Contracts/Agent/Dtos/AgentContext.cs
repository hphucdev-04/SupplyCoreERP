using System.Collections.Generic;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentContext
{
    public List<AgentSessionMessageDto> Steps { get; set; } = new();
}
