using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Notifications;

/// <summary>
/// Theo dõi trạng thái đọc của từng user.
/// Được tạo lazy — chỉ khi user mark as read.
/// </summary>
public class UserNotification : CreationAuditedEntity<Guid>
{
    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private UserNotification() { }

    internal UserNotification(Guid id, Guid notificationId, Guid userId) : base(id)
    {
        NotificationId = notificationId;
        UserId = userId;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }
}