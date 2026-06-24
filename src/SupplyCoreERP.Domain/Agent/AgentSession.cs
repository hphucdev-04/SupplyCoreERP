using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Enums.Agent;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Agent;

public class AgentSession : CreationAuditedAggregateRoot<Guid>
{
    public Guid UserId { get; private set; }

    public virtual ICollection<AgentMessage> Messages { get; private set; }

    public virtual ICollection<AgentTask> Tasks { get; private set; }

    protected AgentSession()
    {
        Messages = new List<AgentMessage>();
        Tasks = new List<AgentTask>();
    }

    public AgentSession(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
        Messages = new List<AgentMessage>();
        Tasks = new List<AgentTask>();
    }

    public void AddMessage(Guid id, string role, string? text, string? toolCallsJson, string? toolResponsesJson)
    {
        Messages.Add(new AgentMessage(id, Id, role, text, toolCallsJson, toolResponsesJson));
    }

    public void AddTask(Guid id, AgentTaskType taskType, string? formJson, string? suspendedDataJson)
    {
        List<AgentTask> pendingTasks = Tasks
            .Where(t => t.TaskType == taskType && t.Status == AgentTaskStatus.Pending)
            .ToList();

        foreach (AgentTask? pendingTask in pendingTasks)
        {
            pendingTask.Cancel();
        }

        Tasks.Add(new AgentTask(id, Id, taskType, formJson, suspendedDataJson));
    }

    public void CompleteTask(Guid taskId)
    {
        AgentTask? task = Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new BusinessException("AgentSession:CompleteTask", "Không tìm thấy tác vụ tương ứng trong phiên hội thoại này.");
        }

        task.Complete();
    }

    public void CancelTask(Guid taskId)
    {
        AgentTask? task = Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new BusinessException("AgentSession:CancelTask", "Không tìm thấy tác vụ tương ứng trong phiên hội thoại này.");
        }

        task.Cancel();
    }

    public void ClearSessionData()
    {
        Messages.Clear();
        Tasks.Clear();
    }
}
