using SupplyCoreERP.Enums.Notificaitons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Notifications;

public class NotificationManager : DomainService
{
    private readonly IRepository<UserNotification, Guid> _userNotificationRepo;

    public NotificationManager(IRepository<UserNotification, Guid> userNotificationRepo)
    {
        _userNotificationRepo = userNotificationRepo;
    }

    public Notification CreateForGlobal(
        string title, string content, NotificationSeverity severity)
    {
        return new Notification(
            GuidGenerator.Create(),
            title, content, severity,
            NotificationLevel.Global);
    }

    public Notification CreateForPermission(
        string title, string content,
        NotificationSeverity severity,
        List<string> targetPermissions)
    {
        return new Notification(
            GuidGenerator.Create(),
            title, content, severity,
            NotificationLevel.Permission,
            targetPermissions);
    }

    public async Task MarkReadAsync(Guid notificationId, Guid userId)
    {
        UserNotification? record = await _userNotificationRepo
            .FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

        if (record == null)
        {
            record = new UserNotification(GuidGenerator.Create(), notificationId, userId);
            await _userNotificationRepo.InsertAsync(record);
        }

        record.MarkAsRead();
        await _userNotificationRepo.UpdateAsync(record);
    }

    public async Task MarkManyReadAsync(IEnumerable<Guid> notificationIds, Guid userId)
    {
        List<Guid> ids = notificationIds.ToList();
        if (!ids.Any()) return;

        List<UserNotification> existing = await _userNotificationRepo
            .GetListAsync(x => x.UserId == userId && ids.Contains(x.NotificationId));

        HashSet<Guid> existingIds = existing.Select(x => x.NotificationId).ToHashSet();

        List<UserNotification> toInsert = ids
            .Where(id => !existingIds.Contains(id))
            .Select(id => new UserNotification(GuidGenerator.Create(), id, userId))
            .ToList();

        if (toInsert.Any())
            await _userNotificationRepo.InsertManyAsync(toInsert, autoSave: true);

        List<UserNotification> toUpdate = existing.Where(x => !x.IsRead).ToList();
        foreach (UserNotification un in toUpdate.Concat(toInsert))
            un.MarkAsRead();

        if (toUpdate.Any())
            await _userNotificationRepo.UpdateManyAsync(toUpdate, autoSave: true);
    }

    public async Task MarkDeleteAsync(Guid notificationId, Guid userId)
    {
        UserNotification? record = await _userNotificationRepo.FirstOrDefaultAsync(x => x.NotificationId == notificationId && x.UserId == userId);

        if (record == null)
        {
            record = new UserNotification(GuidGenerator.Create(), notificationId, userId);
            await _userNotificationRepo.InsertAsync(record);
        }

        record.MarkAsDeleted();
        await _userNotificationRepo.UpdateAsync(record);
    }

    public async Task MarkManyDeleteAsync(IEnumerable<Guid> notificationIds, Guid userId)
    {
        List<Guid> ids = notificationIds.ToList();
        if (!ids.Any()) return;

        List<UserNotification> existing = await _userNotificationRepo
            .GetListAsync(x => x.UserId == userId && ids.Contains(x.NotificationId));

        HashSet<Guid> existingIds = existing.Select(x => x.NotificationId).ToHashSet();

        List<UserNotification> toInsert = ids
            .Where(id => !existingIds.Contains(id))
            .Select(id => new UserNotification(GuidGenerator.Create(), id, userId))
            .ToList();

        if (toInsert.Any())
            await _userNotificationRepo.InsertManyAsync(toInsert, autoSave: true);

        List<UserNotification> toUpdate = existing.Where(x => !x.IsDelete).ToList();
        foreach (UserNotification un in toUpdate.Concat(toInsert))
            un.MarkAsDeleted();

        if (toUpdate.Any())
            await _userNotificationRepo.UpdateManyAsync(toUpdate, autoSave: true);
    }
}