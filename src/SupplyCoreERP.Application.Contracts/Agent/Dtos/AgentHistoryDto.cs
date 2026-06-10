using System.Collections.Generic;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentHistoryDto
{
    public List<AgentMessageDto> Steps { get; set; } = new();
    
    public object? PendingTask { get; set; }
}
