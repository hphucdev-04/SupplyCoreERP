using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Areas;

namespace SupplyCoreERP.Suppliers
{
	public class SupplierManager : DomainService
	{
		private readonly IRepository<Supplier, Guid> _supplierRepository;
		// Inject Repo của Location
		private readonly IRepository<Country, Guid> _countryRepo;
		private readonly IRepository<City, Guid> _cityRepo;
		private readonly IRepository<Area, Guid> _areaRepo;

		public SupplierManager(
			IRepository<Supplier, Guid> supplierRepository,
			IRepository<Country, Guid> countryRepo,
			IRepository<City, Guid> cityRepo,
			IRepository<Area, Guid> areaRepo)
		{
			_supplierRepository = supplierRepository;
			_countryRepo = countryRepo;
			_cityRepo = cityRepo;
			_areaRepo = areaRepo;
		}

		public async Task<Supplier> CreateAsync(
			string code, string name, string taxCode, string phoneNumber, string email,
			string representativeName, string note,
			string address, Guid? countryId, Guid? cityId, Guid? areaId)
		{
			await CheckCodeExistsAsync(code);

			//Validate địa lý
			await ValidateLocationAsync(countryId, cityId, areaId);

			return new Supplier(
				GuidGenerator.Create(),
				code, name, taxCode, phoneNumber, email, representativeName, note,
				address, countryId, cityId, areaId
			);
		}

		public async Task UpdateAsync(
			Supplier supplier,
			string name, string taxCode, string phoneNumber, string email,
			string representativeName, string note,
			string address, Guid? countryId, Guid? cityId, Guid? areaId)
		{
			Check.NotNull(supplier, nameof(supplier));

			// Validate địa lý 
			await ValidateLocationAsync(countryId, cityId, areaId);

			supplier.UpdateInfo(name, taxCode, phoneNumber, email, representativeName, note);
			supplier.SetLocation(address, countryId, cityId, areaId);
		}

		private async Task CheckCodeExistsAsync(string code)
		{
			if (await _supplierRepository.AnyAsync(x => x.Code == code))
				throw new UserFriendlyException($"Mã nhà cung cấp '{code}' đã tồn tại!");
		}

		private async Task ValidateLocationAsync(Guid? countryId, Guid? cityId, Guid? areaId)
		{
			//Check Country
			if (countryId.HasValue && !await _countryRepo.AnyAsync(x => x.Id == countryId))
				throw new UserFriendlyException("Quốc gia không tồn tại!");

			//Check City và quan hệ City -> Country
			if (cityId.HasValue)
			{
				var city = await _cityRepo.FindAsync(cityId.Value);
				if (city == null) throw new UserFriendlyException("Tỉnh/Thành phố không tồn tại!");

				if (countryId.HasValue && city.CountryId != countryId)
					throw new UserFriendlyException($"Thành phố '{city.Name}' không thuộc quốc gia đã chọn!");
			}

			//Check Area và quan hệ Area -> City
			if (areaId.HasValue)
			{
				var area = await _areaRepo.FindAsync(areaId.Value);
				if (area == null) throw new UserFriendlyException("Khu vực (Quận/Huyện) không tồn tại!");

				if (cityId.HasValue && area.CityId != cityId)
					throw new UserFriendlyException($"Khu vực '{area.Name}' không thuộc Tỉnh/Thành phố đã chọn!");
			}
		}
	}
}