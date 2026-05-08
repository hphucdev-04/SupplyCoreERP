using SupplyCoreERP.EntityFrameworkCore;
using Xunit;

namespace SupplyCoreERP.Categories
{
    [Collection(SupplyCoreERPTestConsts.CollectionDefinitionName)]
    public class CategoryAppService_EntityFrameworkCore_Integration_Tests : CategoryAppService_Integration_Tests<SupplyCoreERPEntityFrameworkCoreTestModule>
    {
    }
}
