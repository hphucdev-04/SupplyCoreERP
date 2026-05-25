using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Sales.PriceLists;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Partner.Customers;

public class CustomerTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Country, Guid> _countryRepository;
    private readonly IRepository<City, Guid> _cityRepository;
    private readonly IRepository<Area, Guid> _areaRepository;
    private readonly IRepository<PriceList, Guid> _priceListRepository;

    public CustomerTestDataSeedContributor(
        IRepository<Customer, Guid> customerRepository,
        IRepository<Country, Guid> countryRepository,
        IRepository<City, Guid> cityRepository,
        IRepository<Area, Guid> areaRepository,
        IRepository<PriceList, Guid> priceListRepository)
    {
        _customerRepository = customerRepository;
        _countryRepository = countryRepository;
        _cityRepository = cityRepository;
        _areaRepository = areaRepository;
        _priceListRepository = priceListRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        Country? country = await _countryRepository.FirstOrDefaultAsync(x => x.ISO == "VNM");
        City? city = await _cityRepository.FirstOrDefaultAsync(x => x.Name == "Tp. Hồ Chí Minh");
        Area? area = await _areaRepository.FirstOrDefaultAsync(x => x.Name == "Quận 1");

        // Seed PriceList
        if (await _priceListRepository.FindAsync(SeedData.TestDataConsts.PriceListOfficialId) == null)
        {
            await _priceListRepository.InsertAsync(
                new PriceList(SeedData.TestDataConsts.PriceListOfficialId, "PL-OFFICIAL", "Bảng giá sỉ chính thức", true),
                autoSave: true
            );
        }

        if (await _customerRepository.FindAsync(SeedData.TestDataConsts.CustomerAId) == null)
        {
            await _customerRepository.InsertAsync(
                new Customer(
                    SeedData.TestDataConsts.CustomerAId,
                    "CUS-001",
                    "Khách Hàng A",
                    "0909999999",
                    "customer_a@test.com",
                    "Nguyen Van B",
                    Gender.Male,
                    CustomerType.Organization,
                    "MST-CUS-123",
                    "456 Le Loi",
                    country?.Id,
                    city?.Id,
                    area?.Id,
                    "Ghi chu khach hang",
                    300000000m,
                    30,
                    SeedData.TestDataConsts.PriceListOfficialId
                ),
                autoSave: true
            );
        }
    }
}
