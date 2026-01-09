using SupplyCoreERP.Samples;
using Xunit;

namespace SupplyCoreERP.EntityFrameworkCore.Applications;

[Collection(SupplyCoreERPTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<SupplyCoreERPEntityFrameworkCoreTestModule>
{

}
