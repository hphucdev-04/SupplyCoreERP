using System.Collections.Generic;
using SupplyCoreERP.Enums.Notificaitons;

namespace SupplyCoreERP.Notifications.Jobs;

public class NotificationSentJobArgs
{
    public string Title { get; set; }
    public string Content { get; set; }
    public NotificationSeverity Severity { get; set; }
    public NotificationLevel Level { get; set; }

    /// <summary>Chỉ set khi Level = Permission.</summary>
    public List<string> TargetPermissions { get; set; } = new();
}
