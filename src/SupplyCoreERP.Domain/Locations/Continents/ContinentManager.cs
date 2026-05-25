using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Locations.Continents;

public class ContinentManager : DomainService
{
    // Dependencies
    private readonly IRepository<Continent, Guid> _continentRepository;

    // Constructor injection
    public ContinentManager(IRepository<Continent, Guid> continentRepository)
    {
        _continentRepository = continentRepository;
    }

    public async Task<Continent> CreateAsync(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));

        if (await _continentRepository.AnyAsync(x => x.Name == name))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateContinent", $"Châu lục '{name}' đã tồn tại!");
        }

        return new Continent(GuidGenerator.Create(), name);
    }
}






