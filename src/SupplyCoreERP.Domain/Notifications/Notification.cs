using SupplyCoreERP.Enums.Notificaitons;
using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Notifications;

public class Notification : CreationAuditedAggregateRoot<Guid>
{
    public string Title { get; private set; }
    public string Content { get; private set; }
    public NotificationSeverity Severity { get; private set; }
    public NotificationLevel Level { get; private set; }

    /// <summary>
    /// Danh sách ABP permission string.
    /// Chỉ có giá trị khi Level = Permission.
    /// User có BẤT KỲ permission nào trong list sẽ nhận được notification.
    /// </summary>
    public List<string> TargetPermissions { get; private set; }


    private Notification() { }

    internal Notification(
        Guid id,
        string title,
        string content,
        NotificationSeverity severity,
        NotificationLevel level,
        List<string>? targetPermissions = null)
        : base(id)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 255);
        Content = Check.NotNullOrWhiteSpace(content, nameof(content), 2055);
        Severity = severity;
        Level = level;
        TargetPermissions = targetPermissions ?? new List<string>();
    }
}