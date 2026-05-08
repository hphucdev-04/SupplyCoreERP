using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SupplyCoreERP.Data;

/* This is used if database provider does't define
 * ISupplyCoreERPDbSchemaMigrator implementation.
 */
public class NullSupplyCoreERPDbSchemaMigrator : ISupplyCoreERPDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
