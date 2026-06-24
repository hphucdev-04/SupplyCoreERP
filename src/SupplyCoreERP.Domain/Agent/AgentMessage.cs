using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Agent;

public class AgentMessage : CreationAuditedEntity<Guid>
{
    public Guid SessionId { get; private set; }

    public string Role { get; private set; }

    public string? Text { get; private set; }

    public string? ToolCallsJson { get; private set; }

    public string? ToolResponsesJson { get; private set; }

    protected AgentMessage()
    {
    }

    internal AgentMessage(
        Guid id,
        Guid sessionId,
        string role,
        string? text,
        string? toolCallsJson,
        string? toolResponsesJson) : base(id)
    {
        SessionId = sessionId;
        Role = role;
        Text = text;
        ToolCallsJson = toolCallsJson;
        ToolResponsesJson = toolResponsesJson;
    }
}
