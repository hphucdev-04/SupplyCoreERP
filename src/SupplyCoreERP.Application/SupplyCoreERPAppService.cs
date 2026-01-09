using SupplyCoreERP.Localization;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP;

/* Inherit your application services from this class.
 */
public abstract class SupplyCoreERPAppService : ApplicationService
{
    protected SupplyCoreERPAppService()
    {
        LocalizationResource = typeof(SupplyCoreERPResource);
    }
}
