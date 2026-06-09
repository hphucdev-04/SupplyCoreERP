using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Agent;

public class AgentSession : CreationAuditedEntity<Guid>
{
    public Guid UserId { get; set; }

    public string ConversationHistoryJson { get; set; }

    private AgentSession()
    {
    }

    internal AgentSession(Guid id, Guid userId, string conversationHistoryJson) : base(id)
    {
        UserId = userId;
        ConversationHistoryJson = conversationHistoryJson;
    }
}
