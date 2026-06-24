using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Agent;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Agent;

public interface IAgentManager : IDomainService
{
    Task<AgentSession> GetOrCreateSessionAsync(Guid? sessionId, Guid userId);
    Task<List<AgentMessage>> GetOptimizedHistoryAsync(Guid sessionId);
    Task AddMessageAsync(AgentSession session, string role, string? text, string? toolCallsJson = null, string? toolResponsesJson = null);
    Task ClearSessionHistoryAsync(AgentSession session);
    Task<AgentTask> CreateTaskAsync(AgentSession session, AgentTaskType taskType, string? formJson, string? suspendedDataJson);
    Task<AgentTask?> FindPendingTaskAsync(AgentSession session, AgentTaskType taskType);
    Task CompleteTaskAsync(AgentSession session, AgentTask task);
    Task CancelTaskAsync(AgentSession session, AgentTask task);
}
