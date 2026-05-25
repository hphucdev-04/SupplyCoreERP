using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Notifications.Dtos;

namespace SupplyCoreERP.Notifications;

public interface INotificationRealTime
{
    Task SendToGlobalAsync(NotificationDto dto);
    Task SendToPermissionGroupsAsync(IEnumerable<string> permissions, NotificationDto dto);
}

