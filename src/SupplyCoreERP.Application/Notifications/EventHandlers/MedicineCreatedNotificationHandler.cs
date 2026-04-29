using SupplyCoreERP.Enums.Notificaitons;
using SupplyCoreERP.Medicines.Events;
using SupplyCoreERP.Notifications.Jobs;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

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
        await _backgroundJobManager.EnqueueAsync(new NotificationJobArgs
        {
            Title = "Thuốc mới được tạo",
            Content = $"Thuốc [{eventData.MedicineCode}] {eventData.MedicineName} vừa được thêm vào hệ thống.",
            Severity = NotificationSeverity.Info,
            Level = NotificationLevel.Global
        });
    }
}