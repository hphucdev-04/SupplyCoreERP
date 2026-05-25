using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Manufacturers;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Manufacturers.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Manufacturers;

public class ManufacturerAppService :
    CrudAppService<
        Manufacturer,
        ManufacturerDto,
        Guid,
        GetManufacturerListDto,
        CreateUpdateManufacturerDto>,
    IManufacturerAppService
{
    private readonly ManufacturerManager _manager;
    private readonly IRepository<Country, Guid> _countryRepository;
    private readonly IRepository<Continent, Guid> _continentRepository;

    public ManufacturerAppService(
        IRepository<Manufacturer, Guid> repository,
        ManufacturerManager manager,
        IRepository<Country, Guid> countryRepository,
        IRepository<Continent, Guid> continentRepository)
        : base(repository)
    {
        _manager = manager;
        _countryRepository = countryRepository;
        _continentRepository = continentRepository;
    }

    protected override async Task<IQueryable<Manufacturer>> CreateFilteredQueryAsync(GetManufacturerListDto input)
    {
        IQueryable<Manufacturer> query = await base.CreateFilteredQueryAsync(input);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
        }

        return query;
    }

    public override async Task<PagedResultDto<ManufacturerDto>> GetListAsync(GetManufacturerListDto input)
    {
        IQueryable<Manufacturer> query = await CreateFilteredQueryAsync(input);
        int totalCount = await AsyncExecuter.CountAsync(query);

        List<Manufacturer> entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(e => e.CreationTime)
                 .PageBy(input)
        );

        List<ManufacturerDto> dtos = ObjectMapper.Map<List<Manufacturer>, List<ManufacturerDto>>(entities);

        Guid[] countryIds = dtos.Select(x => x.CountryId).Distinct().ToArray();
        Guid[] continentIds = dtos.Select(x => x.ContinentId).Distinct().ToArray();

        List<Country> countries = await _countryRepository.GetListAsync(x => countryIds.Contains(x.Id));
        List<Continent> continents = await _continentRepository.GetListAsync(x => continentIds.Contains(x.Id));

        foreach (ManufacturerDto dto in dtos)
        {
            dto.CountryName = countries.FirstOrDefault(x => x.Id == dto.CountryId)?.Name;
            dto.ContinentName = continents.FirstOrDefault(x => x.Id == dto.ContinentId)?.Name;
        }

        return new PagedResultDto<ManufacturerDto>(totalCount, dtos);
    }

    public override async Task<ManufacturerDto> CreateAsync(CreateUpdateManufacturerDto input)
    {
        Manufacturer entity = await _manager.CreateAsync(
            input.Name,
            input.ContinentId,
            input.CountryId
        );

        await Repository.InsertAsync(entity);

        return ObjectMapper.Map<Manufacturer, ManufacturerDto>(entity);
    }

    public override async Task<ManufacturerDto> UpdateAsync(Guid id, CreateUpdateManufacturerDto input)
    {
        Manufacturer entity = await Repository.GetAsync(id);

        await _manager.UpdateAsync(
            entity,
            input.Name,
            input.ContinentId,
            input.CountryId
        );

        await Repository.UpdateAsync(entity);

        return ObjectMapper.Map<Manufacturer, ManufacturerDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        Manufacturer entity = await Repository.GetAsync(id);
        await _manager.DeleteAsync(entity);
    }
}

