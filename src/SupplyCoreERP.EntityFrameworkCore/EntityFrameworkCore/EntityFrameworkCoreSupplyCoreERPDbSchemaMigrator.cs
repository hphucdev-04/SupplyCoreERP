using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupplyCoreERP.Data;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.EntityFrameworkCore;

public class EntityFrameworkCoreSupplyCoreERPDbSchemaMigrator
    : ISupplyCoreERPDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreSupplyCoreERPDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the SupplyCoreERPDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<SupplyCoreERPDbContext>()
            .Database
            .MigrateAsync();
    }
}
