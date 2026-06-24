using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Agent;
using SupplyCoreERP.Settings;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace SupplyCoreERP.Agent;

public class AgentManager : DomainService, IAgentManager
{
    private readonly IRepository<AgentSession, Guid> _sessionRepository;
    private readonly IRepository<AgentMessage, Guid> _messageRepository;
    private readonly ISettingProvider _settingProvider;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AgentManager(
        IRepository<AgentSession, Guid> sessionRepository,
        IRepository<AgentMessage, Guid> messageRepository,
        ISettingProvider settingProvider)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _settingProvider = settingProvider;
    }

    /// <summary>
    /// Lấy session hiện tại hoặc tự động tìm session gần đây nhất của user.
    /// Nếu không có, tạo session mới.
    /// </summary>
    public async Task<AgentSession> GetOrCreateSessionAsync(Guid? sessionId, Guid userId)
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
            session = new AgentSession(GuidGenerator.Create(), userId);
            await _sessionRepository.InsertAsync(session, autoSave: true);
        }

        return session;
    }

    /// <summary>
    /// Nạp lịch sử tin nhắn tối ưu trực tiếp từ database
    /// </summary>
    public async Task<List<AgentMessage>> GetOptimizedHistoryAsync(Guid sessionId)
    {
        // 1. Đọc cấu hình số lượng tin nhắn tối đa (sử dụng GetAsync<int> từ SettingProvider)
        int maxMessages = await _settingProvider.GetAsync<int>(SupplyCoreERPSettings.AgentMaxHistoryMessages);

        // 2. Truy cập database thông qua _messageRepository thay vì nạp cả Session kèm Messages
        IQueryable<AgentMessage> queryable = await _messageRepository.GetQueryableAsync();

        // Lấy maxMessages tin nhắn mới nhất (sắp xếp giảm dần theo CreationTime để lấy tin nhắn mới trước)
        List<AgentMessage> latestMessages = await AsyncExecuter.ToListAsync(
            queryable
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreationTime)
                .Take(maxMessages)
        );

        if (!latestMessages.Any())
        {
            return new List<AgentMessage>();
        }

        // Đảo ngược danh sách để đưa về thứ tự thời gian tăng dần (cũ đến mới)
        latestMessages.Reverse();

        // 3. Đảm bảo điểm bắt đầu hợp lệ: luôn bắt đầu bằng tin nhắn của "user" có văn bản hợp lệ
        if (latestMessages[0].Role != "user" || string.IsNullOrEmpty(latestMessages[0].Text))
        {
            DateTime firstMessageCreationTime = latestMessages[0].CreationTime;

            // Tìm tin nhắn "user" gần nhất trước thời điểm firstMessageCreationTime
            AgentMessage? priorUserMessage = await AsyncExecuter.FirstOrDefaultAsync(
                queryable
                    .Where(m => m.SessionId == sessionId && m.CreationTime < firstMessageCreationTime && m.Role == "user" && !string.IsNullOrEmpty(m.Text))
                    .OrderByDescending(m => m.CreationTime)
            );

            if (priorUserMessage != null)
            {
                // Lấy tất cả tin nhắn nằm giữa priorUserMessage và tin nhắn đầu tiên (firstMessageCreationTime)
                List<AgentMessage> gapMessages = await AsyncExecuter.ToListAsync(
                    queryable
                        .Where(m => m.SessionId == sessionId && m.CreationTime >= priorUserMessage.CreationTime && m.CreationTime < firstMessageCreationTime)
                        .OrderBy(m => m.CreationTime)
                );

                latestMessages.InsertRange(0, gapMessages);
            }
        }

        return latestMessages;
    }

    /// <summary>
    /// Thêm tin nhắn mới vào Aggregate Root Session và cập nhật DB (có lọc DLP)
    /// </summary>
    public async Task AddMessageAsync(AgentSession session, string role, string? text, string? toolCallsJson = null, string? toolResponsesJson = null)
    {
        List<DlpRule> dlpRules = await LoadDlpRulesAsync();

        string? sanitizedText = text != null ? SanitizeText(text, dlpRules) : null;
        string? sanitizedToolCalls = toolCallsJson != null ? SanitizeJsonString(toolCallsJson, dlpRules) : null;
        string? sanitizedToolResponses = toolResponsesJson != null ? SanitizeJsonString(toolResponsesJson, dlpRules) : null;

        session.AddMessage(GuidGenerator.Create(), role, sanitizedText, sanitizedToolCalls, sanitizedToolResponses);
        await _sessionRepository.UpdateAsync(session, autoSave: true);
    }

    private async Task<List<DlpRule>> LoadDlpRulesAsync()
    {
        try
        {
            string? dlpRulesJson = await _settingProvider.GetOrNullAsync(SupplyCoreERPSettings.DlpRules);
            if (!string.IsNullOrEmpty(dlpRulesJson))
            {
                List<DlpRule>? rules = JsonSerializer.Deserialize<List<DlpRule>>(dlpRulesJson, _jsonOptions);
                if (rules != null && rules.Count > 0)
                {
                    return rules;
                }
            }
        }
        catch
        {
            // Bỏ qua lỗi cấu hình, không lọc
        }

        return new List<DlpRule>();
    }

    private string SanitizeText(string text, List<DlpRule> rules)
    {
        if (string.IsNullOrEmpty(text) || rules.Count == 0)
        {
            return text;
        }

        foreach (DlpRule rule in rules)
        {
            if (!string.IsNullOrEmpty(rule.Pattern) && !string.IsNullOrEmpty(rule.Replacement))
            {
                try
                {
                    text = Regex.Replace(text, rule.Pattern, rule.Replacement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                }
                catch
                {
                    // Bỏ qua regex lỗi
                }
            }
        }

        return text;
    }

    private string? SanitizeJsonString(string json, List<DlpRule> rules)
    {
        if (string.IsNullOrEmpty(json))
        {
            return json;
        }

        try
        {
            JsonNode? node = JsonNode.Parse(json);
            SanitizeJsonNode(node, rules);
            return node?.ToJsonString();
        }
        catch
        {
            return SanitizeText(json, rules);
        }
    }

    private void SanitizeJsonNode(JsonNode? node, List<DlpRule> rules)
    {
        if (node == null)
        {
            return;
        }

        if (node is JsonObject obj)
        {
            List<string> keys = obj.Select(x => x.Key).ToList();
            foreach (string key in keys)
            {
                JsonNode? child = obj[key];
                if (child is JsonValue val && val.TryGetValue<string>(out string? str) && str != null)
                {
                    obj[key] = JsonValue.Create(SanitizeText(str, rules));
                }
                else
                {
                    SanitizeJsonNode(child, rules);
                }
            }
        }
        else if (node is JsonArray arr)
        {
            for (int i = 0; i < arr.Count; i++)
            {
                JsonNode? child = arr[i];
                if (child is JsonValue val && val.TryGetValue<string>(out string? str) && str != null)
                {
                    arr[i] = JsonValue.Create(SanitizeText(str, rules));
                }
                else
                {
                    SanitizeJsonNode(child, rules);
                }
            }
        }
    }

    /// <summary>
    /// Xóa toàn bộ lịch sử tin nhắn và tác vụ của session (New Chat)
    /// </summary>
    public async Task ClearSessionHistoryAsync(AgentSession session)
    {
        session.ClearSessionData();
        await _sessionRepository.UpdateAsync(session);
    }

    /// <summary>
    /// Tạo mới một tác vụ Agent (duyệt hoặc điền form) ở trạng thái Pending.
    /// Logic chuyển trạng thái các task cũ cùng loại sang Cancelled được đóng gói trong Aggregate Root.
    /// </summary>
    public async Task<AgentTask> CreateTaskAsync(AgentSession session, AgentTaskType taskType, string? formJson, string? suspendedDataJson)
    {
        Guid taskId = GuidGenerator.Create();
        session.AddTask(taskId, taskType, formJson, suspendedDataJson);
        await _sessionRepository.UpdateAsync(session);
        return session.Tasks.First(t => t.Id == taskId);
    }

    /// <summary>
    /// Tìm tác vụ đang Pending của session
    /// </summary>
    public Task<AgentTask?> FindPendingTaskAsync(AgentSession session, AgentTaskType taskType)
    {
        AgentTask? task = session.Tasks.FirstOrDefault(t => t.TaskType == taskType && t.Status == AgentTaskStatus.Pending);
        return Task.FromResult(task);
    }

    /// <summary>
    /// Hoàn thành tác vụ (đổi trạng thái sang Completed)
    /// </summary>
    public async Task CompleteTaskAsync(AgentSession session, AgentTask task)
    {
        session.CompleteTask(task.Id);
        await _sessionRepository.UpdateAsync(session);
    }

    /// <summary>
    /// Hủy tác vụ (đổi trạng thái sang Cancelled)
    /// </summary>
    public async Task CancelTaskAsync(AgentSession session, AgentTask task)
    {
        session.CancelTask(task.Id);
        await _sessionRepository.UpdateAsync(session);
    }
}

public class DlpRule
{
    public string Name { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string Replacement { get; set; } = string.Empty;
}

