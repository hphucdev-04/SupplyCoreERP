using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Batches.Events;
using SupplyCoreERP.Notifications.Jobs;
using SupplyCoreERP.Permissions;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace SupplyCoreERP.Notifications.Handlers;

public class BatchQAStatusChangedNotificationHandler
    : ILocalEventHandler<BatchQAStatusChangedDomainEvent>, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public BatchQAStatusChangedNotificationHandler(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task HandleEventAsync(BatchQAStatusChangedDomainEvent eventData)
    {
        (string? title, string? content, NotificationSeverity severity) = eventData.NewStatus switch
        {
            BatchQAStatus.Approved => (
                "Lô hàng đạt QA - Cần chuyển phân khu",
                $"Lô hàng [{eventData.BatchNumber}] đã được phê duyệt QA. Vui lòng chuyển từ phân khu QA sang phân khu Lưu trữ.",
                NotificationSeverity.Success),
            BatchQAStatus.Rejected => (
                "Lô hàng không đạt QA - Cần chuyển phân khu",
                $"Lô hàng [{eventData.BatchNumber}] không đạt yêu cầu QA. Vui lòng chuyển từ phân khu QA sang phân khu Biệt trữ (Quarantine).",
                NotificationSeverity.Warning),
            BatchQAStatus.Recalled => (
                "Lô hàng bị thu hồi - Cần chuyển phân khu",
                $"Lô hàng [{eventData.BatchNumber}] bị thu hồi khẩn cấp. Vui lòng chuyển từ phân khu Lưu trữ sang phân khu Biệt trữ (Quarantine).",
                NotificationSeverity.Error),
            BatchQAStatus.Expired => (
                "Lô hàng hết hạn - Cần chuyển phân khu",
                $"Lô hàng [{eventData.BatchNumber}] đã hết hạn sử dụng. Vui lòng chuyển sang phân khu Biệt trữ (Quarantine).",
                NotificationSeverity.Warning),
            _ => default
        };

        if (title is null)
        {
            return;
        }

        await _backgroundJobManager.EnqueueAsync(new NotificationSentJobArgs
        {
            Title = title,
            Content = content,
            Severity = severity,
            Level = NotificationLevel.Permission,
            TargetPermissions = new List<string>
            {
                SupplyCoreERPPermissions.Inventory.Warehouse.ZoneTransfer
            }
        });
    }
}
