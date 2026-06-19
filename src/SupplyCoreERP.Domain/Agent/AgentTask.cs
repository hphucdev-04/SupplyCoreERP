using System;
using SupplyCoreERP.Enums.Agent;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Agent;

public class AgentTask : CreationAuditedEntity<Guid>
{
    public Guid SessionId { get; private set; }

    public AgentTaskType TaskType { get; private set; }

    public AgentTaskStatus Status { get; internal set; }

    public string? FormJson { get; private set; }

    public string? SuspendedDataJson { get; private set; }

    protected AgentTask()
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

    internal void Complete()
    {
        if (Status != AgentTaskStatus.Pending)
        {
            throw new BusinessException("AgentTask:ErrorComplete", "Chỉ các tác vụ đang chờ mới có thể hoàn thành.");
        }
        Status = AgentTaskStatus.Completed;
    }

    internal void Cancel()
    {
        if (Status != AgentTaskStatus.Pending)
        {
            throw new BusinessException("AgentTask:ErrorCancel", "Chỉ các tác vụ đang chờ mới có thể hủy.");
        }
        Status = AgentTaskStatus.Cancelled;
    }
}
