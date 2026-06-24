using SupplyCoreERP.Catalog.Products;
using Xunit;

namespace SupplyCoreERP.EntityFrameworkCore.Domains.Catalog;

[Collection("EFCoreTests")]
public class ProductManager_Integration_Tests : ProductManager_Integration_Tests<SupplyCoreERPEntityFrameworkCoreTestModule>
{
}
