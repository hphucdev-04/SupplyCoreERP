using Volo.Abp.Modularity;

namespace SupplyCoreERP.Mcp.Client;

[DependsOn(
    typeof(SupplyCoreERPApplicationContractsModule)
)]
public class SupplyCoreERPMcpClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // ABP Dependency Injection sẽ tự động quét và đăng ký McpClientService
        // vì nó kế thừa interface ITransientDependency.
    }
}
