using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentSessionInputDto
{
    [Required]
    public Guid SessionId { get; set; }
}
