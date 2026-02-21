using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Customers
{
	public class CustomerManager : DomainService
	{
		private readonly IRepository<Customer, Guid> _customerRepository;
		private readonly IRepository<Country, Guid> _countryRepo;
		private readonly IRepository<City, Guid> _cityRepo;
		private readonly IRepository<Area, Guid> _areaRepo;

		public CustomerManager(
			IRepository<Customer, Guid> customerRepository,
			IRepository<Country, Guid> countryRepo,
			IRepository<City, Guid> cityRepo,
			IRepository<Area, Guid> areaRepo)
		{
			_customerRepository = customerRepository;
			_countryRepo = countryRepo;
			_cityRepo = cityRepo;
			_areaRepo = areaRepo;
		}

		public async Task<Customer> CreateAsync(
			string code,
			string name,
			string? phoneNumber,
			string? email,
			string? representativeName,
			Gender? gender,
			CustomerType type,
			string? taxCode,
			string? address,
			Guid? countryId,
			Guid? cityId,
			Guid? areaId,
			string? note,
			decimal debtLimit = 0,
			int paymentTermDays = 0)
		{
			await CheckCodeAndNameAsync(code, name);
			await CheckPhoneNumberExistsAsync(phoneNumber);
			await ValidateLocationAsync(countryId, cityId, areaId);


			return new Customer(
				GuidGenerator.Create(),
				code, name, phoneNumber, email, representativeName, gender, type, taxCode,
				address, countryId, cityId, areaId, note, debtLimit, paymentTermDays
			);
		}

		public async Task UpdateAsync(
			Customer customer,
			string code,
			string name,
			string? phoneNumber,
			string? email,
			string? representativeName,
			Gender? gender,
			CustomerType type,
			string? taxCode,
			string? address,
			Guid? countryId,
			Guid? cityId,
			Guid? areaId,
			string? note,
			decimal debtLimit = 0,
			int paymentTermDays = 0)
		{
			Check.NotNull(customer, nameof(customer));
			await CheckCodeAndNameAsync(code, name, customer.Id);

			if (customer.PhoneNumber != phoneNumber)
			{
				await CheckPhoneNumberExistsAsync(phoneNumber);
			}

			await ValidateLocationAsync(countryId, cityId, areaId);

			customer.UpdateCode(code);
			customer.UpdateInfo(name, phoneNumber, email, representativeName, gender, type, taxCode, note);
			customer.SetLocation(address, countryId, cityId, areaId);
			customer.SetDebtInfo(debtLimit, paymentTermDays);
		}

		public async Task DeleteAsync(Guid id)
		{
			var customer = await _customerRepository.GetAsync(id);

			// Logic nghiệp vụ: Không được xóa nếu khách hàng còn nợ tiền chưa trả
			if (customer.CurrentDebt > 0)
			{
				throw new UserFriendlyException($"Không thể xóa khách hàng '{customer.Name}' vì vẫn còn khoản nợ ({customer.CurrentDebt:N0}) chưa thanh toán!");
			}

			await _customerRepository.DeleteAsync(customer);
		}

		private async Task CheckPhoneNumberExistsAsync(string? phoneNumber)
		{
			if (string.IsNullOrWhiteSpace(phoneNumber)) return;

			if (await _customerRepository.AnyAsync(x => x.PhoneNumber == phoneNumber))
			{
				throw new UserFriendlyException($"Số điện thoại '{phoneNumber}' đã được đăng ký cho khách hàng khác!");
			}
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

			if (await _customerRepository.AnyAsync(x =>
				x.Code == normalizedCode &&
				(!excludeId.HasValue || x.Id != excludeId.Value)))
			{
				throw new UserFriendlyException($"Mã nhà cung cấp '{code}' đã tồn tại!");
			}

			if (await _customerRepository.AnyAsync(x =>
				x.Name == normalizedName &&
				(!excludeId.HasValue || x.Id != excludeId.Value)))
			{
				throw new UserFriendlyException($"Tên nhà cung cấp '{name}' đã tồn tại!");
			}
		}
	}
}