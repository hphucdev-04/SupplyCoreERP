using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentRequestInputDto
{
    [Required]
    public string Text { get; set; }

    public List<AgentMessageDto> History { get; set; } = new();
}
