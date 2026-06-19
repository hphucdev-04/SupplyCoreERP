using System.Collections.Generic;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentHistoryDto
{
    public List<AgentSessionMessageDto> Steps { get; set; } = new();

    public object? PendingTask { get; set; }
}
