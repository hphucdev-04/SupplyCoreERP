using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Partner.Suppliers;

public class SupplierTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Country, Guid> _countryRepository;
    private readonly IRepository<City, Guid> _cityRepository;
    private readonly IRepository<Area, Guid> _areaRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<BaseUnit, Guid> _unitRepository;

    public SupplierTestDataSeedContributor(
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Country, Guid> countryRepository,
        IRepository<City, Guid> cityRepository,
        IRepository<Area, Guid> areaRepository,
        IRepository<Product, Guid> productRepository,
        IRepository<BaseUnit, Guid> unitRepository)
    {
        _supplierRepository = supplierRepository;
        _countryRepository = countryRepository;
        _cityRepository = cityRepository;
        _areaRepository = areaRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        Country? country = await _countryRepository.FirstOrDefaultAsync(x => x.ISO == "VNM");
        City? city = await _cityRepository.FirstOrDefaultAsync(x => x.Name == "Tp. Hồ Chí Minh");
        Area? area = await _areaRepository.FirstOrDefaultAsync(x => x.Name == "Quận 1");

        if (await _supplierRepository.FindAsync(SeedData.TestDataConsts.SupplierAId) == null)
        {
            var supplier = new Supplier(
                SeedData.TestDataConsts.SupplierAId,
                "SUP-001",
                "Nhà Cung Cấp A",
                "MST-123456",
                "0901234567",
                "supplier_a@test.com",
                "Nguyen Van A",
                "Ghi chu NCC",
                "123 Nguyen Hue",
                country?.Id,
                city?.Id,
                area?.Id,
                Gender.Male,
                500000000m,
                30
            );

            // Gán sản phẩm Paracetamol
            SupplierProduct sp = supplier.AddProduct(
                Guid.NewGuid(),
                SeedData.TestDataConsts.MedicineParacetamolId,
                SeedData.TestDataConsts.UnitBoxId,
                5,
                true,
                "San pham Paracetamol cung cap chinh"
            );

            // Thêm các mức giá MOQ
            sp.AddCondition(new SupplierProductCondition(
                Guid.NewGuid(),
                sp.Id,
                SeedData.TestDataConsts.UnitBoxId,
                1,
                100000m, // 100k cho MOQ 10
                10m
            ));

            sp.AddCondition(new SupplierProductCondition(
                Guid.NewGuid(),
                sp.Id,
                SeedData.TestDataConsts.UnitBoxId,
                1,
                90000m, // 90k cho MOQ 100
                100m
            ));

            sp.ValidateConditions();

            await _supplierRepository.InsertAsync(supplier, autoSave: true);
        }
    }
}
