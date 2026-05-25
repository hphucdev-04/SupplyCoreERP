using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Notificaitons;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Common.Notifications;

public class Notification : CreationAuditedAggregateRoot<Guid>
{
    public string Title { get; private set; }
    public string Content { get; private set; }
    public NotificationSeverity Severity { get; private set; }
    public NotificationLevel Level { get; private set; }

    /// <summary>
    /// Danh sách các quyền mà người dùng cần có để nhận được thông báo này. Nếu để trống, tất cả người dùng đều nhận được thông báo.
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






