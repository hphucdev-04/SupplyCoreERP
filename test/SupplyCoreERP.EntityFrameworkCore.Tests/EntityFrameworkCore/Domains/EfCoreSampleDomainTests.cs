using SupplyCoreERP.Samples;
using Xunit;

namespace SupplyCoreERP.EntityFrameworkCore.Domains;

[Collection(SupplyCoreERPTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<SupplyCoreERPEntityFrameworkCoreTestModule>
{

}
