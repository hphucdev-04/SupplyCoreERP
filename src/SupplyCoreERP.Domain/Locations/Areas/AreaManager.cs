using System;
using System.Threading.Tasks;
using SupplyCoreERP.Locations.Cities;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Locations.Areas;

public class AreaManager : DomainService
{
    // Dependencies
    private readonly IRepository<Area, Guid> _areaRepository;
    private readonly IRepository<City, Guid> _cityRepository;

    // Constructor injection
    public AreaManager(
        IRepository<Area, Guid> areaRepository,
        IRepository<City, Guid> cityRepository)
    {
        _areaRepository = areaRepository;
        _cityRepository = cityRepository;
    }

    public async Task<Area> CreateAsync(Guid cityId, string zipCode, string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));

        if (!await _cityRepository.AnyAsync(x => x.Id == cityId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidCity", "Thành phố không tồn tại!");
        }

        if (await _areaRepository.AnyAsync(x => x.CityId == cityId && x.Name == name))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateArea", $"Khu vực '{name}' đã tồn tại trong thành phố này!");
        }

        return new Area(GuidGenerator.Create(), cityId, zipCode, name);
    }
}






