using System;
using System.Threading.Tasks;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.SeedData;

public class WarehouseTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;

    public WarehouseTestDataSeedContributor(IRepository<Warehouse, Guid> warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _warehouseRepository.FindAsync(TestDataConsts.WarehouseMainId) == null)
        {
            var warehouse = new Warehouse(
                TestDataConsts.WarehouseMainId,
                "WH-MAIN",
                "Kho chính",
                "Địa chỉ kho chính",
                null,
                null,
                null
            );

            await _warehouseRepository.InsertAsync(warehouse, autoSave: true);
        }
    }
}
