using SupplyCoreERP.Enums.Notificaitons;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Notifications.Dtos;

public class GetNotificationListDto : PagedAndSortedResultRequestDto
{
    public bool? IsRead { get; set; }
    public NotificationLevel? Level { get; set; }
}

