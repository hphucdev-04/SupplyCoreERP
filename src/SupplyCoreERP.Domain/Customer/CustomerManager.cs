using System;
using System.Threading.Tasks;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Prices; // <--- GỌI MODULE GIÁ VÀO ĐÂY
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Customers;

public class CustomerManager : DomainService
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Country, Guid> _countryRepo;
    private readonly IRepository<City, Guid> _cityRepo;
    private readonly IRepository<Area, Guid> _areaRepo;
    private readonly IRepository<PriceList, Guid> _priceListRepo;
    private readonly DocumentSequenceManager _documentSequenceManager;

    public CustomerManager(
        IRepository<Customer, Guid> customerRepository,
        IRepository<Country, Guid> countryRepo,
        IRepository<City, Guid> cityRepo,
        IRepository<Area, Guid> areaRepo,
        IRepository<PriceList, Guid> priceListRepo,
        DocumentSequenceManager documentSequenceManager)
    {
        _customerRepository = customerRepository;
        _countryRepo = countryRepo;
        _cityRepo = cityRepo;
        _areaRepo = areaRepo;
        _priceListRepo = priceListRepo;
        _documentSequenceManager = documentSequenceManager;
    }

    public async Task<Customer> CreateAsync(
        string name, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, CustomerType type, string? taxCode,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId, string? note,
        decimal debtLimit = 0, int paymentTermDays = 0, Guid? priceListId = null)
    {

        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeCustomer);

        await CheckCodeAndNameAsync(code, name);
        await CheckPhoneNumberExistsAsync(phoneNumber);
        await ValidateLocationAsync(countryId, cityId, areaId);
        await ValidatePriceListAsync(priceListId);

        return new Customer(
            GuidGenerator.Create(), code, name, phoneNumber, email, representativeName,
            gender, type, taxCode, address, countryId, cityId, areaId, note,
            debtLimit, paymentTermDays, priceListId
        );
    }

    public async Task UpdateAsync(
        Customer customer, string name, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, CustomerType type, string? taxCode,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId, string? note,
        decimal debtLimit = 0, int paymentTermDays = 0, Guid? priceListId = null)
    {
        Check.NotNull(customer, nameof(customer));

        if (customer.PhoneNumber != phoneNumber)
        {
            await CheckPhoneNumberExistsAsync(phoneNumber);
        }

        await ValidateLocationAsync(countryId, cityId, areaId);
        await ValidatePriceListAsync(priceListId);

        customer.UpdateInfo(name, phoneNumber, email, representativeName, gender, type, taxCode, note);
        customer.SetLocation(address, countryId, cityId, areaId);
        customer.SetDebtInfo(debtLimit, paymentTermDays);
        customer.SetPriceList(priceListId);
    }

    public async Task DeleteAsync(Guid id)
    {
        Customer customer = await _customerRepository.GetAsync(id);

        if (customer.CurrentDebt > 0)
        {
            throw new UserFriendlyException($"Không thể xóa khách hàng '{customer.Name}' vì vẫn còn khoản nợ ({customer.CurrentDebt:N0}) chưa thanh toán!");
        }

        await _customerRepository.DeleteAsync(customer);
    }

    private async Task CheckPhoneNumberExistsAsync(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return;
        }

        if (await _customerRepository.AnyAsync(x => x.PhoneNumber == phoneNumber))
        {
            throw new UserFriendlyException($"Số điện thoại '{phoneNumber}' đã được đăng ký cho khách hàng khác!");
        }
    }

    private async Task ValidateLocationAsync(Guid? countryId, Guid? cityId, Guid? areaId)
    {
        if (countryId.HasValue && !await _countryRepo.AnyAsync(x => x.Id == countryId))
        {
            throw new UserFriendlyException("Quốc gia không tồn tại!");
        }

        if (cityId.HasValue)
        {
            City? city = await _cityRepo.FindAsync(cityId.Value);
            if (city == null)
            {
                throw new UserFriendlyException("Tỉnh/Thành phố không tồn tại!");
            }

            if (countryId.HasValue && city.CountryId != countryId)
            {
                throw new UserFriendlyException($"Thành phố '{city.Name}' không thuộc quốc gia đã chọn!");
            }
        }

        if (areaId.HasValue)
        {
            Area? area = await _areaRepo.FindAsync(areaId.Value);
            if (area == null)
            {
                throw new UserFriendlyException("Khu vực (Quận/Huyện) không tồn tại!");
            }

            if (cityId.HasValue && area.CityId != cityId)
            {
                throw new UserFriendlyException($"Khu vực '{area.Name}' không thuộc Tỉnh/Thành phố đã chọn!");
            }
        }
    }

    private async Task ValidatePriceListAsync(Guid? priceListId)
    {
        if (priceListId.HasValue && !await _priceListRepo.AnyAsync(x => x.Id == priceListId.Value))
        {
            throw new UserFriendlyException("Bảng giá được chọn không tồn tại trong hệ thống!");
        }
    }

    public async Task CheckCodeAndNameAsync(string code, string name, Guid? excludeId = null)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code));
        Check.NotNullOrWhiteSpace(name, nameof(name));

        string normalizedCode = code.Trim().ToUpper();
        string normalizedName = name.Trim();

        // Check Code
        if (await _customerRepository.AnyAsync(x => x.Code == normalizedCode && x.Id != excludeId))
        {
            throw new UserFriendlyException($"Mã khách hàng '{code}' đã tồn tại!");
        }

        // Check Name 
        if (await _customerRepository.AnyAsync(x => x.Name == normalizedName && x.Id != excludeId))
        {
            throw new UserFriendlyException($"Tên khách hàng '{name}' đã tồn tại!");
        }
    }
}
