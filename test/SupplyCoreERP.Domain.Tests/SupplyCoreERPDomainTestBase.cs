using Volo.Abp.Modularity;

namespace SupplyCoreERP;

/* Inherit from this class for your domain layer tests. */
public abstract class SupplyCoreERPDomainTestBase<TStartupModule> : SupplyCoreERPTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
