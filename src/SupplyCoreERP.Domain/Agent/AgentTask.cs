using System;
using SupplyCoreERP.Enums.Agent;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Agent;

public class AgentTask : CreationAuditedEntity<Guid>
{
    public Guid SessionId { get; set; }

    public AgentTaskType TaskType { get; set; }

    public AgentTaskStatus Status { get; set; }

    public string? FormJson { get; set; }

    public string? SuspendedDataJson { get; set; }

    private AgentTask()
    {
    }

    internal AgentTask(Guid id, Guid sessionId, AgentTaskType taskType, string? formJson, string? suspendedDataJson) : base(id)
    {
        SessionId = sessionId;
        TaskType = taskType;
        Status = AgentTaskStatus.Pending;
        FormJson = formJson;
        SuspendedDataJson = suspendedDataJson;
    }
}
