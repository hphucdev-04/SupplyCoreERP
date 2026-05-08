using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SupplyCoreERP.Notifications;
using SupplyCoreERP.Notifications.Dtos;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.SignalR.Notifications;

public class NotificationRealTime : INotificationRealTime, ITransientDependency
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationRealTime(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToGlobalAsync(NotificationDto dto)
    {
        await _hubContext.Clients
            .Group("Global")
            .SendAsync("ReceiveNotification", dto);
    }

    public async Task SendToPermissionGroupsAsync(
        IEnumerable<string> permissions, NotificationDto dto)
    {
        foreach (var perm in permissions)
        {
            await _hubContext.Clients
                .Group(perm)
                .SendAsync("ReceiveNotification", dto);
        }
    }
}
