using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;

namespace SupplyCoreERP.Agent.Dtos;

public class AgentMessageDto
{
    [Required]
    public string Role { get; set; } // "user" hoặc "model"

    public string? Text { get; set; }

    public List<AgentToolCallMessageDto>? ToolCalls { get; set; }

    public List<AgentToolResponseMessageDto>? ToolResponses { get; set; }
}

public class AgentToolCallMessageDto
{
    public string Name { get; set; }

    public JsonObject Arguments { get; set; }
}

public class AgentToolResponseMessageDto
{
    public string Name { get; set; }

    public string Content { get; set; }
}
