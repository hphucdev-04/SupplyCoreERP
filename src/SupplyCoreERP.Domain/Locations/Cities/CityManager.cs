using System;
using System.Threading.Tasks;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Locations.Cities;

public class CityManager : DomainService
{
    // Dependencies
    private readonly IRepository<City, Guid> _cityRepository;
    private readonly IRepository<Country, Guid> _countryRepository;

    // Constructor injection
    public CityManager(
        IRepository<City, Guid> cityRepository,
        IRepository<Country, Guid> countryRepository)
    {
        _cityRepository = cityRepository;
        _countryRepository = countryRepository;
    }

    public async Task<City> CreateAsync(Guid countryId, string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));

        if (!await _countryRepository.AnyAsync(x => x.Id == countryId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidCountry", "Quốc gia không tồn tại!");
        }

        if (await _cityRepository.AnyAsync(x => x.CountryId == countryId && x.Name == name))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateCity", $"Thành phố '{name}' đã tồn tại trong quốc gia này!");
        }

        return new City(GuidGenerator.Create(), countryId, name);
    }
}






