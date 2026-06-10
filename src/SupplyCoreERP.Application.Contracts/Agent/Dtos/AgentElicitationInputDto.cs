using System;
using System.Collections.Generic;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentElicitationInputDto
{
    public Guid SessionId { get; set; }

    public Dictionary<string, string> FormValues { get; set; } = new();
}
