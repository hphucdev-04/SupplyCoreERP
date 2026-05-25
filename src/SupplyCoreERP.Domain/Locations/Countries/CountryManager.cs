using System;
using System.Threading.Tasks;
using SupplyCoreERP.Locations.Continents;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Locations.Countries;

public class CountryManager : DomainService
{
    // Dependencies
    private readonly IRepository<Country, Guid> _countryRepository;
    private readonly IRepository<Continent, Guid> _continentRepository;

    // Constructor injection
    public CountryManager(
        IRepository<Country, Guid> countryRepository,
        IRepository<Continent, Guid> continentRepository)
    {
        _countryRepository = countryRepository;
        _continentRepository = continentRepository;
    }

    public async Task<Country> CreateAsync(Guid continentId, string iso, string name)
    {
        Check.NotNullOrWhiteSpace(iso, nameof(iso));
        Check.NotNullOrWhiteSpace(name, nameof(name));

        if (!await _continentRepository.AnyAsync(x => x.Id == continentId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidContinent", "Châu lục không tồn tại!");
        }

        if (await _countryRepository.AnyAsync(x => x.ISO == iso))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateCountry", $"Mã quốc gia '{iso}' đã tồn tại!");
        }

        return new Country(GuidGenerator.Create(), continentId, iso, name);
    }
}






