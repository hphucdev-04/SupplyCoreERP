using System;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentResponseOutputDto
{
    public string Text { get; set; }

    public Guid? SessionId { get; set; }
}
