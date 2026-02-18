using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Medicines;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Manufacturers
{
	public class ManufacturerManager : DomainService
	{
		private readonly IRepository<Manufacturer, Guid> _repository;
		private readonly IRepository<Continent, Guid> _continentRepository;
		private readonly IRepository<Country, Guid> _countryRepository;
		private readonly IRepository<Medicine, Guid> _medicineRepository;

		public ManufacturerManager(
			IRepository<Manufacturer, Guid> repository,
			IRepository<Continent, Guid> continentRepository,
			IRepository<Country, Guid> countryRepository,
			IRepository<Medicine, Guid> medicineRepository)
		{
			_repository = repository;
			_continentRepository = continentRepository;
			_countryRepository = countryRepository;
			_medicineRepository = medicineRepository;
		}

		public async Task<Manufacturer> CreateAsync(string name, Guid continentId, Guid countryId)
		{
			Check.NotNullOrWhiteSpace(name, nameof(name));
			var normalizedName = name.Trim();

			//Check tồn tại Châu lục & Quốc gia
			if (!await _continentRepository.AnyAsync(x => x.Id == continentId))
				throw new UserFriendlyException("Châu lục không tồn tại!");

			var country = await _countryRepository.GetAsync(countryId);
			if (country == null)
				throw new UserFriendlyException("Quốc gia không tồn tại!");

			//Check Logic: Quốc gia phải thuộc Châu lục đã chọn
			if (country.ContinentId != continentId)
				throw new UserFriendlyException($"Quốc gia '{country.Name}' không thuộc châu lục đã chọn!");

			//Check trùng tên Nhà sản xuất
			if (await _repository.AnyAsync(x => x.Name == normalizedName))
				throw new UserFriendlyException($"Nhà sản xuất '{name}' đã tồn tại!");

			return new Manufacturer(GuidGenerator.Create(), normalizedName, continentId, countryId);
		}

		public async Task UpdateAsync(Manufacturer entity, string newName, Guid newContinentId, Guid newCountryId)
		{
			Check.NotNull(entity, nameof(entity));
			Check.NotNullOrWhiteSpace(newName, nameof(newName));
			var normalizedName = newName.Trim();

			//Check tồn tại Châu lục & Quốc gia
			if (!await _continentRepository.AnyAsync(x => x.Id == newContinentId))
				throw new UserFriendlyException("Châu lục không tồn tại!");

			var country = await _countryRepository.GetAsync(newCountryId);
			if (country == null)
				throw new UserFriendlyException("Quốc gia không tồn tại!");

			//Check Logic: Quốc gia phải thuộc Châu lục
			if (country.ContinentId != newContinentId)
				throw new UserFriendlyException($"Quốc gia '{country.Name}' không thuộc châu lục đã chọn!");

			//Check trùng tên (Trừ chính nó ra)
			if (await _repository.AnyAsync(x => x.Name == normalizedName && x.Id != entity.Id))
				throw new UserFriendlyException($"Tên nhà sản xuất '{newName}' đã được sử dụng!");

			entity.Update(normalizedName, newContinentId, newCountryId);
		}

		public async Task DeleteAsync(Manufacturer entity)
		{
			Check.NotNull(entity, nameof(entity));

			//Check ràng buộc với Medicine
			var isInUse = await _medicineRepository.AnyAsync(x => x.ManufacturerId == entity.Id);

			if (isInUse)
			{
				throw new UserFriendlyException($"Không thể xóa '{entity.Name}' vì đang có thuốc thuộc hãng sản xuất này!");
			}

			await _repository.DeleteAsync(entity);
		}
	}
}