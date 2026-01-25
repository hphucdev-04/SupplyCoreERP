using SupplyCoreERP.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.OpenIddict;
using Volo.Abp.BlobStoring.Database;
using Localization.Resources.AbpUi;

namespace SupplyCoreERP;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(BlobStoringDatabaseDomainSharedModule)
    )]
public class SupplyCoreERPDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        SupplyCoreERPGlobalFeatureConfigurator.Configure();
        SupplyCoreERPModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SupplyCoreERPDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<SupplyCoreERPResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
				.AddBaseTypes(typeof(AbpUiResource))
				.AddVirtualJson("/Localization/SupplyCoreERP")
                .AddVirtualJson("/Localization/Permission");

            options.DefaultResourceType = typeof(SupplyCoreERPResource);

			options.Languages.Add(new LanguageInfo("en", "en", "English"));
			options.Languages.Add(new LanguageInfo("vi", "vi", "Vietnamese")); 

        });
        
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SupplyCoreERP", typeof(SupplyCoreERPResource));
        });
    }
}
