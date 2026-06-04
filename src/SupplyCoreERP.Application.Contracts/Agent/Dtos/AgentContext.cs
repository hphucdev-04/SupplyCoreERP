using System.Collections.Generic;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentContext
{
    public List<AgentMessageDto> Steps { get; set; } = new();
}
