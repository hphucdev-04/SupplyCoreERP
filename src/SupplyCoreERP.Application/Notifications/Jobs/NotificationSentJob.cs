using System.Threading.Tasks;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Notifications.Dtos;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Notifications.Jobs;

public class NotificationSentJob
    : AsyncBackgroundJob<NotificationSentJobArgs>, ITransientDependency
{
    private readonly INotificationAppService _notificationAppService;
    private readonly INotificationRealTime _notificationRealTime;

    public NotificationSentJob(
        INotificationAppService notificationAppService,
        INotificationRealTime notificationRealTime)
    {
        _notificationAppService = notificationAppService;
        _notificationRealTime = notificationRealTime;
    }

    public override async Task ExecuteAsync(NotificationSentJobArgs args)
    {
        // Persist vào DB qua interface (không gọi concrete AppService)
        NotificationDto dto = args.Level == NotificationLevel.Global
            ? await _notificationAppService.CreateGlobalAsync(
                args.Title, args.Content, args.Severity)
            : await _notificationAppService.CreateForPermissionAsync(
                args.Title, args.Content, args.Severity, args.TargetPermissions);

        // Gửi real-time — Application chỉ biết interface, không biết SignalR
        if (args.Level == NotificationLevel.Global)
        {
            await _notificationRealTime.SendToGlobalAsync(dto);
        }
        else
        {
            await _notificationRealTime.SendToPermissionGroupsAsync(args.TargetPermissions, dto);
        }
    }
}
