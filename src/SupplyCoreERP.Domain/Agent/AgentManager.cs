using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Agent;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Agent;

public class AgentManager : DomainService
{
    private readonly IRepository<AgentSession, Guid> _sessionRepository;
    private readonly IRepository<AgentTask, Guid> _agentTaskRepository;

    public AgentManager(
        IRepository<AgentSession, Guid> sessionRepository,
        IRepository<AgentTask, Guid> agentTaskRepository)
    {
        _sessionRepository = sessionRepository;
        _agentTaskRepository = agentTaskRepository;
    }

    /// <summary>
    /// Lấy session hiện tại hoặc tự động tìm session gần đây nhất của user.
    /// Nếu không có, tạo session mới.
    /// </summary>
    public async Task<AgentSession> GetOrCreateSessionAsync(Guid? sessionId, Guid userId, string defaultHistoryJson)
    {
        AgentSession? session = null;

        if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            session = await _sessionRepository.FindAsync(sessionId.Value);
        }
        else
        {
            IQueryable<AgentSession> queryable = await _sessionRepository.GetQueryableAsync();
            session = queryable
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreationTime)
                .FirstOrDefault();
        }

        if (session == null)
        {
            session = new AgentSession(GuidGenerator.Create(), userId, defaultHistoryJson);
            await _sessionRepository.InsertAsync(session);
        }

        return session;
    }

    /// <summary>
    /// Lưu lịch sử cuộc trò chuyện mới vào session
    /// </summary>
    public async Task UpdateSessionHistoryAsync(AgentSession session, string conversationHistoryJson)
    {
        session.ConversationHistoryJson = conversationHistoryJson;
        await _sessionRepository.UpdateAsync(session);
    }

    /// <summary>
    /// Tạo mới một tác vụ Agent (duyệt hoặc điền form) ở trạng thái Pending.
    /// Tự động hủy các tác vụ cũ cùng loại đang pending để tránh xung đột dữ liệu.
    /// </summary>
    public async Task<AgentTask> CreateTaskAsync(Guid sessionId, AgentTaskType taskType, string? formJson, string? suspendedDataJson)
    {
        List<AgentTask> oldTasks = (await _agentTaskRepository.GetQueryableAsync())
            .Where(t => t.SessionId == sessionId && t.TaskType == taskType && t.Status == AgentTaskStatus.Pending)
            .ToList();

        foreach (AgentTask? oldTask in oldTasks)
        {
            oldTask.Status = AgentTaskStatus.Cancelled;
            await _agentTaskRepository.UpdateAsync(oldTask);
        }

        AgentTask task = new(GuidGenerator.Create(), sessionId, taskType, formJson, suspendedDataJson);
        await _agentTaskRepository.InsertAsync(task);
        return task;
    }

    /// <summary>
    /// Tìm tác vụ đang Pending của session
    /// </summary>
    public async Task<AgentTask?> FindPendingTaskAsync(Guid sessionId, AgentTaskType taskType)
    {
        return (await _agentTaskRepository.GetQueryableAsync())
            .FirstOrDefault(t => t.SessionId == sessionId && t.TaskType == taskType && t.Status == AgentTaskStatus.Pending);
    }

    /// <summary>
    /// Hoàn thành tác vụ (đổi trạng thái sang Completed)
    /// </summary>
    public async Task CompleteTaskAsync(AgentTask task)
    {
        task.Status = AgentTaskStatus.Completed;
        await _agentTaskRepository.UpdateAsync(task);
    }

    /// <summary>
    /// Hủy tác vụ (đổi trạng thái sang Cancelled)
    /// </summary>
    public async Task CancelTaskAsync(AgentTask task)
    {
        task.Status = AgentTaskStatus.Cancelled;
        await _agentTaskRepository.UpdateAsync(task);
    }
}
