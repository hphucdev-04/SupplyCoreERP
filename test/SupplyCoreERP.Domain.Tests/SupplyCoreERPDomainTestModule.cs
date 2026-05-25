using Volo.Abp.Modularity;

namespace SupplyCoreERP;

[DependsOn(
    typeof(SupplyCoreERPDomainModule),
    typeof(SupplyCoreERPTestBaseModule)
)]
public class SupplyCoreERPDomainTestModule : AbpModule
{

}

