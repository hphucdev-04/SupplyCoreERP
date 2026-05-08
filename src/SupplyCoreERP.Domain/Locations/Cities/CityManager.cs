using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Locations.Cities;

public class CityManager : DomainService
{
    private readonly IRepository<City, Guid> _cityRepository;
    private readonly IRepository<Country, Guid> _countryRepository;

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

        //Check Quốc gia
        if (!await _countryRepository.AnyAsync(x => x.Id == countryId))
        {
            throw new UserFriendlyException("Quốc gia không tồn tại!");
        }

        //Check trùng tên 
        if (await _cityRepository.AnyAsync(x => x.CountryId == countryId && x.Name == name))
        {
            throw new UserFriendlyException($"Thành phố '{name}' đã tồn tại trong quốc gia này!");
        }

        return new City(GuidGenerator.Create(), countryId, name);
    }
}
