using System;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.SeedData;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Catalog.Products;

public abstract class ProductManager_Integration_Tests<TStartupModule> : SupplyCoreERPDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ProductManager _productManager;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepository;

    protected ProductManager_Integration_Tests()
    {
        _productManager = GetRequiredService<ProductManager>();
        _balanceRepository = GetRequiredService<IRepository<InventoryBalance, Guid>>();
    }

    [Fact]
    public async Task Should_Return_False_When_Product_Has_No_Transactions()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act
            var hasTransactions = await _productManager.HasTransactionsAsync(TestDataConsts.MedicineParacetamolId);

            // Assert
            hasTransactions.ShouldBeFalse();
        });
    }
}
