using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace SupplyCoreERP.Notifications.Jobs;

public class NotificationCleanupJob
    : AsyncBackgroundJob<NotificationCleanupJobArgs>, ITransientDependency
{
    private readonly IRepository<Notification, Guid> _notificationRepo;
    private readonly IRepository<UserNotification, Guid> _userNotifRepo;
    private readonly IClock _clock;

    public NotificationCleanupJob(
        IRepository<Notification, Guid> notificationRepo,
        IRepository<UserNotification, Guid> userNotifRepo,
        IClock clock)
    {
        _notificationRepo = notificationRepo;
        _userNotifRepo = userNotifRepo;
        _clock = clock;
    }

    public override async Task ExecuteAsync(NotificationCleanupJobArgs args)
    {
        DateTime cutoff = _clock.Now.AddDays(-args.RetentionDays);

        List<Notification> oldNotifications = await _notificationRepo
            .GetListAsync(n => n.CreationTime < cutoff);

        if (!oldNotifications.Any()) return;

        List<Guid> oldIds = oldNotifications.Select(n => n.Id).ToList();

        List<UserNotification> relatedUserNotifs = await _userNotifRepo
            .GetListAsync(x => oldIds.Contains(x.NotificationId));

        if (relatedUserNotifs.Any())
            await _userNotifRepo.DeleteManyAsync(relatedUserNotifs, autoSave: true);

        await _notificationRepo.DeleteManyAsync(oldNotifications, autoSave: true);
    }
}