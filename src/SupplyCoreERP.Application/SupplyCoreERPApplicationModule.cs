using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;

namespace SupplyCoreERP;    

[DependsOn(
    typeof(SupplyCoreERPDomainModule),
    typeof(SupplyCoreERPApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
	typeof(AbpAutoMapperModule)
	)]
public class SupplyCoreERPApplicationModule : AbpModule
{
	public override void ConfigureServices(ServiceConfigurationContext context)
	{
		Configure<AbpAutoMapperOptions>(options =>
		{
			//Quét project để đăng ký tất cả các profile được định nghĩa
			options.AddMaps<SupplyCoreERPApplicationModule>();
		});
	}
}
