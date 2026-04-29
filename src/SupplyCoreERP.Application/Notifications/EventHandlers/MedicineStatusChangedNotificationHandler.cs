using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Medicines.Events;
using SupplyCoreERP.Notifications.Jobs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using static SupplyCoreERP.Permissions.SupplyCoreERPPermissions;

namespace SupplyCoreERP.Notifications.Handlers;

public class MedicineStatusChangedNotificationHandler
    : ILocalEventHandler<MedicineStatusChangedDomainEvent>, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public MedicineStatusChangedNotificationHandler(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public async Task HandleEventAsync(MedicineStatusChangedDomainEvent eventData)
    {
        bool isApproved = eventData.NewStatus == MedicineStatus.Approved;
        string statusText = isApproved ? "được duyệt" : "bị từ chối";

        await _backgroundJobManager.EnqueueAsync(new NotificationJobArgs
        {
            Title = $"Thuốc {statusText}",
            Content = $"Thuốc [{eventData.MedicineCode}] {eventData.MedicineName} đã {statusText}.",
            Severity = isApproved ? NotificationSeverity.Success : NotificationSeverity.Warning,
            Level = NotificationLevel.Permission,
            TargetPermissions = new List<string>
            {
                Catalog.Medicine.Approve,
                Catalog.Medicine.Reject,
            }
        });
    }
}