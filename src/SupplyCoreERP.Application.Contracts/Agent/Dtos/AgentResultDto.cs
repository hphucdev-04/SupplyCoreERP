namespace SupplyCoreERP.Agent.Dtos;

public class AgentResultDto
{
    public string? FinalText { get; set; }

    public bool RequiresApproval { get; set; }

    public string? PendingToolName { get; set; }

    public string? PendingToolArguments { get; set; }

    public bool RequiresElicitation { get; set; }

    public string? ElicitationFormJson { get; set; }
}
