using System;
using SupplyCoreERP;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace SupplyCoreERP.Partner.Suppliers;

public class SupplierManager_Unit_Tests
{
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<BaseUnit, Guid> _unitRepo;
    private readonly IRepository<Country, Guid> _countryRepo;
    private readonly IRepository<City, Guid> _cityRepo;
    private readonly IRepository<Area, Guid> _areaRepo;
    private readonly DocumentSequenceManager _documentSequenceManager;
    private readonly SupplierManager _supplierManager;

    public SupplierManager_Unit_Tests()
    {
        _supplierRepository = Substitute.For<IRepository<Supplier, Guid>>();
        _productRepo = Substitute.For<IRepository<Product, Guid>>();
        _unitRepo = Substitute.For<IRepository<BaseUnit, Guid>>();
        _countryRepo = Substitute.For<IRepository<Country, Guid>>();
        _cityRepo = Substitute.For<IRepository<City, Guid>>();
        _areaRepo = Substitute.For<IRepository<Area, Guid>>();

        _documentSequenceManager = Substitute.For<DocumentSequenceManager>(
            Substitute.For<IRepository<DocumentSequence, Guid>>()
        );

        _supplierManager = new SupplierManager(
            _supplierRepository, _productRepo, _unitRepo, _countryRepo, _cityRepo, _areaRepo, _documentSequenceManager
        );

        IGuidGenerator guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(x => Guid.NewGuid());

        IAbpLazyServiceProvider lazyServiceProvider = Substitute.For<IAbpLazyServiceProvider>();
        lazyServiceProvider.LazyGetRequiredService(typeof(IGuidGenerator)).Returns(guidGenerator);

        typeof(Volo.Abp.Domain.Services.DomainService)
            .GetProperty("LazyServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            ?.SetValue(_supplierManager, lazyServiceProvider);
    }
    [QATest(scenario: "Ném ngoại lệ khi xóa nhà cung cấp vẫn còn dư nợ.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Delete_Supplier_With_Outstanding_Debt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supplier = new Supplier(
            id, "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        supplier.AddDebt(1000000m); // Add debt

        _supplierRepository.GetAsync(id).Returns(supplier);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.DeleteAsync(id);
        });
        ex.Code.ShouldBe("SupplyCoreERP:CannotDeleteSupplierWithOutstandingDebt");
    }
    [QATest(scenario: "Xóa nhà cung cấp thành công khi không có dư nợ.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Delete_Supplier_Successfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var supplier = new Supplier(
            id, "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        _supplierRepository.GetAsync(id).Returns(supplier);

        // Act
        await _supplierManager.DeleteAsync(id);

        // Assert
        await _supplierRepository.Received(1).DeleteAsync(supplier);
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có tỉnh/thành phố không thuộc quốc gia.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_Country_City_Mismatch()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var countryId = Guid.NewGuid();
        var cityId = Guid.NewGuid();

        _countryRepo.AnyAsync(Arg.Any<Expression<Func<Country, bool>>>()).Returns(true);

        var city = new City(cityId, Guid.NewGuid(), "Tp. Ho Chi Minh"); // Different CountryId
        _cityRepo.FindAsync(cityId).Returns(city);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.UpdateAsync(
                supplier, "Supplier A Updated", null, null, null, null, null, null, null, countryId, cityId, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:CityCountryMismatch");
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có quốc gia không tồn tại.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_Country_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var countryId = Guid.NewGuid();
        _countryRepo.AnyAsync(Arg.Any<Expression<Func<Country, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.UpdateAsync(
                supplier, "Supplier A Updated", null, null, null, null, null, null, null, countryId, null, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:CountryNotFound");
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có tỉnh/thành phố không tồn tại.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_City_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var cityId = Guid.NewGuid();
        _cityRepo.FindAsync(cityId).Returns((City)null);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.UpdateAsync(
                supplier, "Supplier A Updated", null, null, null, null, null, null, null, null, cityId, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:CityNotFound");
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có quận/huyện không tồn tại.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_Area_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var areaId = Guid.NewGuid();
        _areaRepo.FindAsync(areaId).Returns((Area)null);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.UpdateAsync(
                supplier, "Supplier A Updated", null, null, null, null, null, null, null, null, null, areaId
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:AreaNotFound");
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có quận/huyện không thuộc thành phố.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_Area_City_Mismatch()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var cityId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        var city = new City(cityId, Guid.NewGuid(), "Tp. Ho Chi Minh");
        _cityRepo.FindAsync(cityId).Returns(city);

        var area = new Area(areaId, Guid.NewGuid(), "70000", "District 1"); // Different CityId
        _areaRepo.FindAsync(areaId).Returns(area);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.UpdateAsync(
                supplier, "Supplier A Updated", null, null, null, null, null, null, null, null, cityId, areaId
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:AreaCityMismatch");
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo NCC trùng mã đã tồn tại.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Supplier_Code_Exists()
    {
        // Arrange
        _supplierRepository.AnyAsync(Arg.Any<Expression<Func<Supplier, bool>>>()).Returns(true);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.CheckCodeAndNameAsync("SUP-001", "Supplier A");
        });
        ex.Code.ShouldBe("SupplyCoreERP:SupplierCodeExists");
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo NCC trùng tên đã tồn tại.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Supplier_Name_Exists()
    {
        // Arrange
        _supplierRepository.AnyAsync(Arg.Any<Expression<Func<Supplier, bool>>>())
            .Returns(x =>
            {
                var exprStr = x.Arg<Expression<Func<Supplier, bool>>>().ToString();
                if (exprStr.Contains("Code"))
                {
                    return false;
                }

                if (exprStr.Contains("Name"))
                {
                    return true;
                }

                return false;
            });

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.CheckCodeAndNameAsync("SUP-001", "Supplier A");
        });
        ex.Code.ShouldBe("SupplyCoreERP:SupplierNameExists");
    }
    [QATest(scenario: "Tạo mới nhà cung cấp thành công qua Manager.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_Supplier_Successfully()
    {
        // Arrange
        _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeSupplier).Returns("SUP-001");
        _supplierRepository.AnyAsync(Arg.Any<Expression<Func<Supplier, bool>>>()).Returns(false);

        // Act
        Supplier supplier = await _supplierManager.CreateAsync(
            "Supplier A", "123456", "0909999999", "supplier@test.com", "Rep Name",
            Gender.Male, "Note", "Address", null, null, null, 10000000m, 30
        );

        // Assert
        supplier.ShouldNotBeNull();
        supplier.Code.ShouldBe("SUP-001");
        supplier.Name.ShouldBe("Supplier A");
        supplier.TaxCode.ShouldBe("123456");
        supplier.PhoneNumber.ShouldBe("0909999999");
        supplier.Email.ShouldBe("supplier@test.com");
        supplier.RepresentativeName.ShouldBe("Rep Name");
        supplier.Gender.ShouldBe(Gender.Male);
        supplier.Note.ShouldBe("Note");
        supplier.Address.ShouldBe("Address");
        supplier.DebtLimit.ShouldBe(10000000m);
        supplier.PaymentTermDays.ShouldBe(30);
    }
    [QATest(scenario: "Ném ngoại lệ khi thêm sản phẩm không tồn tại trên hệ thống vào NCC.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_AddProduct_Product_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        _productRepo.FindAsync(productId).Returns((Product)null);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.AddProductAsync(supplier, productId, Guid.NewGuid(), 5);
        });
        ex.Code.ShouldBe("SupplyCoreERP:ProductNotFound");
    }
    [QATest(scenario: "Ném ngoại lệ khi thêm sản phẩm chưa được duyệt hoạt động vào NCC.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_AddProduct_Product_NotAvailable()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        var medicine = new Medicine(
            productId, Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol",
            Guid.NewGuid(), Guid.NewGuid(), "REG-123", UsageRoute.Oral, StorageCondition.Normal, false
        );
        _productRepo.FindAsync(productId).Returns(medicine);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.AddProductAsync(supplier, productId, Guid.NewGuid(), 5);
        });
        ex.Code.ShouldBe("SupplyCoreERP:ProductNotAvailable");
    }
    [QATest(scenario: "Ném ngoại lệ khi thêm sản phẩm có đơn vị tính mặc định không tồn tại.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_AddProduct_Unit_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        var medicine = new Medicine(
            productId, Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol",
            Guid.NewGuid(), Guid.NewGuid(), "REG-123", UsageRoute.Oral, StorageCondition.Normal, false
        );
        medicine.Approve();
        _productRepo.FindAsync(productId).Returns(medicine);

        var unitId = Guid.NewGuid();
        _unitRepo.AnyAsync(Arg.Any<Expression<Func<BaseUnit, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.AddProductAsync(supplier, productId, unitId, 5);
        });
        ex.Code.ShouldBe("SupplyCoreERP:UnitNotFound");
    }
    [QATest(scenario: "Thêm sản phẩm thành công vào danh mục của NCC.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_AddProduct_Successfully()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        var medicine = new Medicine(
            productId, Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol",
            Guid.NewGuid(), Guid.NewGuid(), "REG-123", UsageRoute.Oral, StorageCondition.Normal, false
        );
        medicine.Approve();
        _productRepo.FindAsync(productId).Returns(medicine);

        var unitId = Guid.NewGuid();
        _unitRepo.AnyAsync(Arg.Any<Expression<Func<BaseUnit, bool>>>()).Returns(true);

        // Act
        SupplierProduct sp = await _supplierManager.AddProductAsync(supplier, productId, unitId, 5, true, "Note A");

        // Assert
        sp.ShouldNotBeNull();
        sp.ProductId.ShouldBe(productId);
        sp.DefaultUnitId.ShouldBe(unitId);
        sp.LeadTimeDays.ShouldBe(5);
        sp.IsPreferred.ShouldBeTrue();
        sp.Note.ShouldBe("Note A");
        supplier.SupplierProducts.ShouldContain(sp);
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi Cập nhật sản phẩm đơn vị tính not found.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_UpdateProduct_Unit_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var unitId = Guid.NewGuid();
        _unitRepo.AnyAsync(Arg.Any<Expression<Func<BaseUnit, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.UpdateProductAsync(supplier, Guid.NewGuid(), unitId, 5, true, "Note A");
        });
        ex.Code.ShouldBe("SupplyCoreERP:UnitNotFound");
    }
    [QATest(scenario: "Cập nhật sản phẩm thành công.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_UpdateProduct_Successfully()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var spId = Guid.NewGuid();

        // Add product first to update later
        supplier.AddProduct(spId, productId, unitId, 5, false, "Note A");

        _unitRepo.AnyAsync(Arg.Any<Expression<Func<BaseUnit, bool>>>()).Returns(true);

        var newUnitId = Guid.NewGuid();

        // Act
        await _supplierManager.UpdateProductAsync(supplier, productId, newUnitId, 10, true, "Note B");

        // Assert
        SupplierProduct? sp = supplier.SupplierProducts.FirstOrDefault(x => x.ProductId == productId);
        sp.ShouldNotBeNull();
        sp.DefaultUnitId.ShouldBe(newUnitId);
        sp.LeadTimeDays.ShouldBe(10);
        sp.IsPreferred.ShouldBeTrue();
        sp.Note.ShouldBe("Note B");
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi Loại bỏ sản phẩm sản phẩm not found.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_RemoveProduct_Product_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        _productRepo.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _supplierManager.RemoveProductAsync(supplier, productId);
        });
        ex.Code.ShouldBe("SupplyCoreERP:ProductNotFound");
    }
    [QATest(scenario: "Loại bỏ sản phẩm thành công.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_RemoveProduct_Successfully()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        supplier.AddProduct(Guid.NewGuid(), productId, unitId, 5, false, "Note A");

        _productRepo.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(true);

        // Act
        await _supplierManager.RemoveProductAsync(supplier, productId);

        // Assert
        supplier.SupplierProducts.ShouldBeEmpty();
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi toggle sản phẩm hoạt động sản phẩm not found.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_ToggleProductActive_Product_NotFound()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );

        // Act & Assert
        BusinessException ex = Assert.Throws<BusinessException>(() =>
        {
            _supplierManager.ToggleProductActive(supplier, Guid.NewGuid());
        });
        ex.Code.ShouldBe("SupplyCoreERP:ProductNotFound");
    }
    [QATest(scenario: "Toggle sản phẩm hoạt động thành công.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_ToggleProductActive_Successfully()
    {
        // Arrange
        var supplier = new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
        var productId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        SupplierProduct sp = supplier.AddProduct(Guid.NewGuid(), productId, unitId, 5, false, "Note A");
        sp.IsActive.ShouldBeTrue();

        // Act
        _supplierManager.ToggleProductActive(supplier, productId);

        // Assert
        sp.IsActive.ShouldBeFalse();

        // Act & Assert Toggle back
        _supplierManager.ToggleProductActive(supplier, productId);
        sp.IsActive.ShouldBeTrue();
    }
}
