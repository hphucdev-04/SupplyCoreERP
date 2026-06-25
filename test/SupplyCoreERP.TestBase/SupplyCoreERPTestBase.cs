using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Uow;

namespace SupplyCoreERP;

public abstract class SupplyCoreERPTestBase<TStartupModule> : AbpIntegratedTest<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
    {
        options.UseAutofac();
    }

    protected override void BeforeAddApplication(IServiceCollection services)
    {
        ConfigurationBuilder builder = new();
        builder.AddJsonFile("appsettings.json", false);
        builder.AddJsonFile("appsettings.secrets.json", true);
        services.ReplaceConfiguration(builder.Build());
    }

    protected virtual Task WithUnitOfWorkAsync(Func<Task> func)
    {
        return WithUnitOfWorkAsync(new AbpUnitOfWorkOptions(), func);
    }

    protected virtual async Task WithUnitOfWorkAsync(AbpUnitOfWorkOptions options, Func<Task> action)
    {
        using IServiceScope scope = ServiceProvider.CreateScope();
        IUnitOfWorkManager uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        using IUnitOfWork uow = uowManager.Begin(options);
        try
        {
            await action();
            await uow.CompleteAsync();
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }

    protected virtual async Task<TResult> WithUnitOfWorkAsync<TResult>(AbpUnitOfWorkOptions options, Func<Task<TResult>> func)
    {
        using IServiceScope scope = ServiceProvider.CreateScope();
        IUnitOfWorkManager uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        using IUnitOfWork uow = uowManager.Begin(options);
        try
        {
            TResult? result = await func();
            await uow.CompleteAsync();
            return result;
        }
        catch
        {
            await uow.RollbackAsync();
            throw;
        }
    }
}

