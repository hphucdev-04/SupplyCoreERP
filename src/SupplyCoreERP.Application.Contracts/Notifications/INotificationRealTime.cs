using SupplyCoreERP.Notifications.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SupplyCoreERP.Notifications
{
    public interface INotificationRealTime
    {
        Task SendToGlobalAsync(NotificationDto dto);
        Task SendToPermissionGroupsAsync(IEnumerable<string> permissions, NotificationDto dto);
    }
}
