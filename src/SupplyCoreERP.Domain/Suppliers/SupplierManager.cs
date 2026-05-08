using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Suppliers;

public class SupplierManager : DomainService
{
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<BaseUnit, Guid> _unitRepo;
    private readonly IRepository<Country, Guid> _countryRepo;
    private readonly IRepository<City, Guid> _cityRepo;
    private readonly IRepository<Area, Guid> _areaRepo;
    private readonly DocumentSequenceManager _documentSequenceManager;

    public SupplierManager(
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<Product, Guid> productRepo,
        IRepository<BaseUnit, Guid> unitRepo,
        IRepository<Country, Guid> countryRepo,
        IRepository<City, Guid> cityRepo,
        IRepository<Area, Guid> areaRepo,
        DocumentSequenceManager documentSequenceManager)
    {
        _supplierRepository = supplierRepository;
        _productRepo = productRepo;
        _unitRepo = unitRepo;
        _countryRepo = countryRepo;
        _cityRepo = cityRepo;
        _areaRepo = areaRepo;
        _documentSequenceManager = documentSequenceManager;
    }

    #region Supplier
    public async Task<Supplier> CreateAsync(
        string name, string? taxCode, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, string? note,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId,
        decimal debtLimit = 0, int paymentTermDays = 0)
    {
        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeSupplier);
        await CheckCodeAndNameAsync(code, name);
        await ValidateLocationAsync(countryId, cityId, areaId);

        return new Supplier(
            GuidGenerator.Create(),
            code, name, taxCode, phoneNumber, email, representativeName, note,
            address, countryId, cityId, areaId, gender, debtLimit, paymentTermDays);
    }

    public async Task UpdateAsync(
        Supplier supplier,
        string name, string? taxCode, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, string? note,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId,
        decimal debtLimit = 0, int paymentTermDays = 0)
    {
        await ValidateLocationAsync(countryId, cityId, areaId);
        supplier.UpdateInfo(name, gender, taxCode, phoneNumber, email, representativeName, note);
        supplier.SetLocation(address, countryId, cityId, areaId);
        supplier.SetDebtInfo(debtLimit, paymentTermDays);
    }

    public async Task DeleteAsync(Guid id)
    {
        Supplier supplier = await _supplierRepository.GetAsync(id);
        if (supplier.CurrentDebt > 0)
        {
            throw new UserFriendlyException(
                $"Không thể xóa nhà cung cấp '{supplier.Name}' vì vẫn còn dư nợ ({supplier.CurrentDebt:N0}) chưa thanh toán!");
        }

        await _supplierRepository.DeleteAsync(supplier);
    }
    #endregion

    #region Supplier Product
    public async Task<SupplierProduct> AddProductAsync(
        Supplier supplier,
        Guid productId,
        Guid defaultUnitId,
        int defaultConversionFactor,
        decimal standardPrice,
        int leadTimeDays,
        decimal minOrderQuantity,
        decimal overDeliveryTolerancePct = 0,
        decimal underDeliveryTolerancePct = 0,
        bool isPreferred = false,
        string? note = null)
    {
        // Cross-aggregate validations
        Product product = await _productRepo.FindAsync(productId)
            ?? throw new UserFriendlyException("Sản phẩm không tồn tại.");

        if (!product.IsAvailableForInventory)
        {
            throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa được duyệt. Không thể thêm vào danh mục cung cấp.");
        }

        if (!await _unitRepo.AnyAsync(x => x.Id == defaultUnitId))
        {
            throw new UserFriendlyException("Đơn vị tính không tồn tại.");
        }

        return supplier.AddProduct(
            GuidGenerator.Create(),
            productId, defaultUnitId,
            defaultConversionFactor, standardPrice, leadTimeDays,
            minOrderQuantity, overDeliveryTolerancePct, underDeliveryTolerancePct,
            isPreferred, note);
    }

    public async Task UpdateProductAsync(
        Supplier supplier,
        Guid productId,
        Guid defaultUnitId,
        int defaultConversionFactor,
        decimal standardPrice,
        int leadTimeDays,
        decimal minOrderQuantity,
        decimal overDeliveryTolerancePct,
        decimal underDeliveryTolerancePct,
        bool isPreferred,
        string? note)
    {
        if (!await _unitRepo.AnyAsync(x => x.Id == defaultUnitId))
        {
            throw new UserFriendlyException("Đơn vị tính không tồn tại.");
        }

        supplier.UpdateProduct(
            productId, defaultUnitId, defaultConversionFactor, standardPrice,
            leadTimeDays, minOrderQuantity,
            overDeliveryTolerancePct, underDeliveryTolerancePct,
            isPreferred, note);
    }
    public async Task RemoveProductAsync(Supplier supplier, Guid productId)
    {
        if (!await _productRepo.AnyAsync(x => x.Id == productId))
        {
            throw new UserFriendlyException("Sản phẩm không tồn tại trên hệ thống.");
        }

        SupplierProduct? sp = supplier.SupplierProducts.FirstOrDefault(x => x.ProductId == productId);
        if (sp != null && sp.IsPreferred)
        {
            throw new UserFriendlyException("Không thể xóa sản phẩm đang được đánh dấu là 'Ưu tiên'.");
        }

        supplier.RemoveProduct(productId);
    }
    public void ToggleProductActive(Supplier supplier, Guid productId)
    {
        SupplierProduct? sp = supplier.SupplierProducts.FirstOrDefault(x => x.ProductId == productId);
        if (sp == null)
        {
            throw new UserFriendlyException("Sản phẩm không thuộc nhà cung cấp này.");
        }

        if (sp.IsActive && sp.IsPreferred)
        {
            throw new UserFriendlyException("Phải bỏ đánh dấu 'Ưu tiên' trước khi ngừng hoạt động sản phẩm này.");
        }

        supplier.ToggleProductActive(productId);
    }
    #endregion

    #region Helper 
    public async Task CheckCodeAndNameAsync(string code, string name, Guid? excludeId = null)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code));
        Check.NotNullOrWhiteSpace(name, nameof(name));

        string normalizedCode = code.Trim().ToUpper();
        string normalizedName = name.Trim();

        if (await _supplierRepository.AnyAsync(x =>
                x.Code == normalizedCode && (!excludeId.HasValue || x.Id != excludeId.Value)))
        {
            throw new UserFriendlyException($"Mã nhà cung cấp '{code}' đã tồn tại!");
        }

        if (await _supplierRepository.AnyAsync(x =>
                x.Name == normalizedName && (!excludeId.HasValue || x.Id != excludeId.Value)))
        {
            throw new UserFriendlyException($"Tên nhà cung cấp '{name}' đã tồn tại!");
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
    #endregion
}
