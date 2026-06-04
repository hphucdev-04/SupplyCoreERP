using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Ai;

public class AgentSession : CreationAuditedEntity<Guid>
{
    public Guid UserId { get; set; }
    
    public string ConversationHistoryJson { get; set; }
    
    public bool IsPendingApproval { get; set; }
    
    public string? PendingToolCallJson { get; set; }

    private AgentSession()
    {
    }

    public AgentSession(Guid id, Guid userId, string conversationHistoryJson) : base(id)
    {
        UserId = userId;
        ConversationHistoryJson = conversationHistoryJson;
        IsPendingApproval = false;
    }
}
