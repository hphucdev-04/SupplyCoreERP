using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentRequestInputDto
{
    [Required]
    public string Text { get; set; }

    public Guid? SessionId { get; set; }
}
