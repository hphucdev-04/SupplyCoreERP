using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace SupplyCoreERP.Notifications.Jobs;

public class NotificationCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    public NotificationCleanupWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 24 * 60 * 60 * 1000; // 24 giờ
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        Logger.LogInformation("Enqueueing NotificationCleanupJob...");

        IBackgroundJobManager jobManager = workerContext.ServiceProvider
            .GetRequiredService<IBackgroundJobManager>();

        await jobManager.EnqueueAsync(new NotificationCleanupJobArgs());
    }
}