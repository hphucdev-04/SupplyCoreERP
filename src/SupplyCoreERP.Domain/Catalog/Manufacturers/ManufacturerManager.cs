using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Catalog.Manufacturers;

public class ManufacturerManager : DomainService
{
    // Dependencies
    private readonly IRepository<Manufacturer, Guid> _repository;
    private readonly IRepository<Continent, Guid> _continentRepository;
    private readonly IRepository<Country, Guid> _countryRepository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IDocumentSequenceManager _documentSequenceManager;

    // Constructor injection
    public ManufacturerManager(
        IRepository<Manufacturer, Guid> repository,
        IRepository<Continent, Guid> continentRepository,
        IRepository<Country, Guid> countryRepository,
        IRepository<Medicine, Guid> medicineRepository,
        IDocumentSequenceManager documentSequenceManager)
    {
        _repository = repository;
        _continentRepository = continentRepository;
        _countryRepository = countryRepository;
        _medicineRepository = medicineRepository;
        _documentSequenceManager = documentSequenceManager;
    }

    public async Task<Manufacturer> CreateAsync(string name, Guid continentId, Guid countryId)
    {

        Check.NotNullOrWhiteSpace(name, nameof(name));
        string normalizedName = name.Trim();

        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeManufacturer);

        //Check châu lục và quốc gia có tồn tại không
        if (!await _continentRepository.AnyAsync(x => x.Id == continentId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidContinent", "Châu lục không tồn tại!");
        }

        Country country = await _countryRepository.GetAsync(countryId);
        if (country == null)
        {
            throw new BusinessException("SupplyCoreERP:InvalidCountry", "Quốc gia không tồn tại!");
        }

        //Check Logic: Quốc gia phải thuộc Châu lục đã chọn
        if (country.ContinentId != continentId)
        {
            throw new BusinessException("SupplyCoreERP:InvalidCountry", $"Quốc gia '{country.Name}' không thuộc châu lục đã chọn!");
        }

        //Check trùng tên Nhà sản xuất
        if (await _repository.AnyAsync(x => x.Name == normalizedName))
        {
            throw new BusinessException("SupplyCoreERP:InvalidManufacturerName", $"Nhà sản xuất '{name}' đã tồn tại!");
        }

        return new Manufacturer(GuidGenerator.Create(), code, normalizedName, continentId, countryId);
    }

    public async Task UpdateAsync(Manufacturer entity, string newName, Guid newContinentId, Guid newCountryId)
    {
        Check.NotNull(entity, nameof(entity));
        Check.NotNullOrWhiteSpace(newName, nameof(newName));
        string normalizedName = newName.Trim();

        //Check châu lục và quốc gia có tồn tại không
        if (!await _continentRepository.AnyAsync(x => x.Id == newContinentId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidContinent", "Châu lục không tồn tại!");
        }

        Country country = await _countryRepository.GetAsync(newCountryId);
        if (country == null)
        {
            throw new BusinessException("SupplyCoreERP:InvalidCountry", "Quốc gia không tồn tại!");
        }

        //Check Logic: Quốc gia phải thuộc Châu lục đã chọn     
        if (country.ContinentId != newContinentId)
        {
            throw new BusinessException("SupplyCoreERP:InvalidCountry", $"Quốc gia '{country.Name}' không thuộc châu lục đã chọn!");
        }

        //Check trùng tên (Trừ chính nó)
        if (await _repository.AnyAsync(x => x.Name == normalizedName && x.Id != entity.Id))
        {
            throw new BusinessException("SupplyCoreERP:InvalidManufacturerName", $"Tên nhà sản xuất '{newName}' đã được sử dụng!");
        }

        entity.Update(normalizedName, newContinentId, newCountryId);
    }

    public async Task DeleteAsync(Manufacturer entity)
    {
        Check.NotNull(entity, nameof(entity));

        //Check ràng buộc với Medicine
        bool isInUse = await _medicineRepository.AnyAsync(x => x.ManufacturerId == entity.Id);

        if (isInUse)
        {
            throw new BusinessException("SupplyCoreERP:ManufacturerInUse", $"Không thể xóa '{entity.Name}' vì đang có thuốc thuộc hãng sản xuất này!");
        }

        await _repository.DeleteAsync(entity);
    }
}







