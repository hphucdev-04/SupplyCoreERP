using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Enums.Partner;

namespace SupplyCoreERP.Suppliers
{
	public class SupplierManager : DomainService
	{
		private readonly IRepository<Supplier, Guid> _supplierRepository;
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
			string code, string name, string? taxCode, string? phoneNumber, string? email,
			string? representativeName, Gender? gender, string? note,
			string? address, Guid? countryId, Guid? cityId, Guid? areaId,
			decimal debtLimit = 0, int paymentTermDays = 0)
		{
			await CheckCodeAndNameAsync(code, name);
			await ValidateLocationAsync(countryId, cityId, areaId);

			return new Supplier(
				GuidGenerator.Create(),
				code, name, taxCode, phoneNumber, email, representativeName, note,
				address, countryId, cityId, areaId, gender, debtLimit, paymentTermDays
			);
		}

		public async Task UpdateAsync(
			Supplier supplier,
			string code,
			string name, string? taxCode, string? phoneNumber, string? email,
			string? representativeName, Gender? gender, string? note,
			string? address, Guid? countryId, Guid? cityId, Guid? areaId,
			decimal debtLimit = 0, int paymentTermDays = 0)
		{
			Check.NotNull(supplier, nameof(supplier));
			await CheckCodeAndNameAsync(code, name, supplier.Id);
			await ValidateLocationAsync(countryId, cityId, areaId);

			supplier.UpdateCode(code);
			supplier.UpdateInfo(name, gender, taxCode, phoneNumber, email, representativeName, note);
			supplier.SetLocation(address, countryId, cityId, areaId);
			supplier.SetDebtInfo(debtLimit, paymentTermDays);
		}

		public async Task DeleteAsync(Guid id)
		{
			var supplier = await _supplierRepository.GetAsync(id);

			// Logic nghiệp vụ: Không được xóa nếu đang còn nợ tiền
			if (supplier.CurrentDebt > 0)
			{
				throw new UserFriendlyException($"Không thể xóa nhà cung cấp '{supplier.Name}' vì vẫn còn dư nợ ({supplier.CurrentDebt:N0}) chưa thanh toán!");
			}

			// Có thể kiểm tra thêm: Đã có đơn hàng nào chưa? (Nếu có thì ko cho xóa, chỉ cho chuyển IsActive = false)

			await _supplierRepository.DeleteAsync(supplier);
		}

		
		private async Task ValidateLocationAsync(Guid? countryId, Guid? cityId, Guid? areaId)
		{
			if (countryId.HasValue && !await _countryRepo.AnyAsync(x => x.Id == countryId))
				throw new UserFriendlyException("Quốc gia không tồn tại!");

			if (cityId.HasValue)
			{
				var city = await _cityRepo.FindAsync(cityId.Value);
				if (city == null) throw new UserFriendlyException("Tỉnh/Thành phố không tồn tại!");
				if (countryId.HasValue && city.CountryId != countryId)
					throw new UserFriendlyException($"Thành phố '{city.Name}' không thuộc quốc gia đã chọn!");
			}

			if (areaId.HasValue)
			{
				var area = await _areaRepo.FindAsync(areaId.Value);
				if (area == null) throw new UserFriendlyException("Khu vực (Quận/Huyện) không tồn tại!");
				if (cityId.HasValue && area.CityId != cityId)
					throw new UserFriendlyException($"Khu vực '{area.Name}' không thuộc Tỉnh/Thành phố đã chọn!");
			}
		}

		public async Task CheckCodeAndNameAsync(string code, string name, Guid? excludeId = null)
		{
			Check.NotNullOrWhiteSpace(code, nameof(code));
			Check.NotNullOrWhiteSpace(name, nameof(name));

			var normalizedCode = code.Trim().ToUpper();
			var normalizedName = name.Trim();

			if (await _supplierRepository.AnyAsync(x =>
				x.Code == normalizedCode &&
				(!excludeId.HasValue || x.Id != excludeId.Value)))
			{
				throw new UserFriendlyException($"Mã nhà cung cấp '{code}' đã tồn tại!");
			}

			if (await _supplierRepository.AnyAsync(x =>
				x.Name == normalizedName &&
				(!excludeId.HasValue || x.Id != excludeId.Value)))
			{
				throw new UserFriendlyException($"Tên nhà cung cấp '{name}' đã tồn tại!");
			}
		}
	}
}