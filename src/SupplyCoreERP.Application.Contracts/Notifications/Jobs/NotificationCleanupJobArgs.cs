namespace SupplyCoreERP.Notifications.Jobs;

public class NotificationCleanupJobArgs
{
    public int RetentionDays { get; set; } = 30;
}
