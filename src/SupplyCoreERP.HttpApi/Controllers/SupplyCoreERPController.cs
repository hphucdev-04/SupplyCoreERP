using SupplyCoreERP.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace SupplyCoreERP.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class SupplyCoreERPController : AbpControllerBase
{
    protected SupplyCoreERPController()
    {
        LocalizationResource = typeof(SupplyCoreERPResource);
    }
}
