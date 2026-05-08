using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Medicines.Events;
using SupplyCoreERP.Notifications.Jobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using static SupplyCoreERP.Permissions.SupplyCoreERPPermissions;

namespace SupplyCoreERP.Notifications.Handlers;

public class MedicineImportNotificationHandler
    : ILocalEventHandler<MedicineImportDomainEvent>, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public MedicineImportNotificationHandler(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task HandleEventAsync(MedicineImportDomainEvent eventData)
    {
        await _backgroundJobManager.EnqueueAsync(new NotificationSentJobArgs
        {
            Title = "Import thuốc hoàn tất",
            Content = $"Đã import {eventData.Items.Count} thuốc ({string.Join(", ", eventData.Items.Select(x => x.MedicineCode))}) đang chờ phê duyệt.",
            Severity = NotificationSeverity.Info,
            Level = NotificationLevel.Permission,
            TargetPermissions = new List<string>
            {
                Catalog.Medicine.Approve,
                Catalog.Medicine.Reject,
            }
        });
    }
}
