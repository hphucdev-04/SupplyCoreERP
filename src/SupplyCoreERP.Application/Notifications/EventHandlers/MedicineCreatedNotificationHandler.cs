using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Medicines.Events;
using SupplyCoreERP.Notifications.Jobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using static SupplyCoreERP.Permissions.SupplyCoreERPPermissions;

namespace SupplyCoreERP.Notifications.Handlers;

public class MedicineCreatedNotificationHandler
    : ILocalEventHandler<MedicineCreatedDomainEvent>, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public MedicineCreatedNotificationHandler(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task HandleEventAsync(MedicineCreatedDomainEvent eventData)
    {
        await _backgroundJobManager.EnqueueAsync(new NotificationSentJobArgs
        {
            Title = "Thuốc mới chờ duyệt",
            Content = $"Thuốc [{eventData.MedicineCode}] {eventData.MedicineName} vừa được tạo và đang chờ phê duyệt.",
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
