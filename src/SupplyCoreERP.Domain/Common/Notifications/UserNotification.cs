using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Common.Notifications;

/// <summary>
/// Theo giõi trạng thái của một thông báo đối với một người dùng cụ thể, bao gồm việc đã đọc hay chưa, thời gian đọc, và trạng thái xóa.
/// Lazy create không tạo riêng đối tượng này khi tạo thông báo mới, chỉ đạo khi change status.
/// </summary>
public class UserNotification : CreationAuditedEntity<Guid>
{
    public Guid NotificationId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public bool IsDelete { get; private set; }

    private UserNotification() { }

    internal UserNotification(Guid id, Guid notificationId, Guid userId) : base(id)
    {
        NotificationId = notificationId;
        UserId = userId;
        IsRead = false;
        IsDelete = false;
    }

    public void MarkAsRead()
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted() => IsDelete = true;
}






