using Microsoft.Extensions.Localization;
using SupplyCoreERP.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace SupplyCoreERP;

[Dependency(ReplaceServices = true)]
public class SupplyCoreERPBrandingProvider : DefaultBrandingProvider
{
    private readonly IStringLocalizer<SupplyCoreERPResource> _localizer;

    public SupplyCoreERPBrandingProvider(IStringLocalizer<SupplyCoreERPResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
