using System;
using System.Collections.Generic;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentSessionMessageDto
{
    public string Role { get; set; } = string.Empty;
    public string? Text { get; set; }
    public List<AgentToolCallMessageDto>? ToolCalls { get; set; }
    public List<AgentToolResponseMessageDto>? ToolResponses { get; set; }
    public DateTime CreationTime { get; set; }
}
