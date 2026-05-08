using Volo.Abp.Modularity;

namespace SupplyCoreERP;

[DependsOn(
    typeof(SupplyCoreERPApplicationModule),
    typeof(SupplyCoreERPDomainTestModule)
)]
public class SupplyCoreERPApplicationTestModule : AbpModule
{

}
