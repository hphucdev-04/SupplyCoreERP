using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Notifications.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Notifications;

public interface INotificationAppService : IApplicationService
{
    Task<PagedResultDto<NotificationDto>> GetListAsync(GetNotificationListDto input);
    Task MarkReadAsync(Guid notificationId);
    Task MarkAllReadAsync(List<Guid> ids);
    Task MarkDeleteAsync(Guid notificationId);
    Task MarkAllDeleteAsync(List<Guid> ids);

    Task<NotificationDto> CreateGlobalAsync(
        string title, string content, NotificationSeverity severity);

    Task<NotificationDto> CreateForPermissionAsync(
        string title, string content,
        NotificationSeverity severity,
        List<string> targetPermissions);
}

