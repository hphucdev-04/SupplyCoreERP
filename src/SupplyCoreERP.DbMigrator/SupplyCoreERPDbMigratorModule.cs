using SupplyCoreERP.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace SupplyCoreERP.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(SupplyCoreERPEntityFrameworkCoreModule),
    typeof(SupplyCoreERPApplicationContractsModule)
)]
public class SupplyCoreERPDbMigratorModule : AbpModule
{
}
