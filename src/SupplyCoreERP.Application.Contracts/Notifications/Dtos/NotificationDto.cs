using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Notificaitons;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Notifications.Dtos;

public class NotificationDto : EntityDto<Guid>
{
    public string Title { get; set; }
    public string Content { get; set; }
    public NotificationSeverity Severity { get; set; }
    public NotificationLevel Level { get; set; }
    public List<string> TargetPermissions { get; set; }
    public DateTime CreationTime { get; set; }
    public bool IsRead { get; set; }
}
