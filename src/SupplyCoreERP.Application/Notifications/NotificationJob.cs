using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Notifications.Dtos;
using SupplyCoreERP.Notifications.Jobs;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Notifications;

public class NotificationJob
    : AsyncBackgroundJob<NotificationJobArgs>, ITransientDependency
{
    private readonly INotificationAppService _notificationAppService;
    private readonly INotificationRealTime _notificationRealTime;

    public NotificationJob(
        INotificationAppService notificationAppService,
        INotificationRealTime notificationRealTime)
    {
        _notificationAppService = notificationAppService;
        _notificationRealTime = notificationRealTime;
    }

    public override async Task ExecuteAsync(NotificationJobArgs args)
    {
        // Bước 1: Persist vào DB qua interface (không gọi concrete AppService)
        NotificationDto dto = args.Level == NotificationLevel.Global
            ? await _notificationAppService.CreateGlobalAsync(
                args.Title, args.Content, args.Severity)
            : await _notificationAppService.CreateForPermissionAsync(
                args.Title, args.Content, args.Severity, args.TargetPermissions);

        // Bước 2: Gửi real-time — Application chỉ biết interface, không biết SignalR
        if (args.Level == NotificationLevel.Global)
            await _notificationRealTime.SendToGlobalAsync(dto);
        else
            await _notificationRealTime.SendToPermissionGroupsAsync(args.TargetPermissions, dto);
    }
}