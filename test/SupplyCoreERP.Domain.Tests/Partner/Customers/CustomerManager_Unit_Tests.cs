using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.Locations.Areas;
using SupplyCoreERP.Locations.Cities;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Sales.PriceLists;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace SupplyCoreERP.Partner.Customers;

public class CustomerManager_Unit_Tests
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Country, Guid> _countryRepo;
    private readonly IRepository<City, Guid> _cityRepo;
    private readonly IRepository<Area, Guid> _areaRepo;
    private readonly IRepository<PriceList, Guid> _priceListRepo;
    private readonly IDocumentSequenceManager _documentSequenceManager;
    private readonly CustomerManager _customerManager;

    public CustomerManager_Unit_Tests()
    {
        _customerRepository = Substitute.For<IRepository<Customer, Guid>>();
        _countryRepo = Substitute.For<IRepository<Country, Guid>>();
        _cityRepo = Substitute.For<IRepository<City, Guid>>();
        _areaRepo = Substitute.For<IRepository<Area, Guid>>();
        _priceListRepo = Substitute.For<IRepository<PriceList, Guid>>();

        _documentSequenceManager = Substitute.For<IDocumentSequenceManager>();

        _customerManager = new CustomerManager(
            _customerRepository, _countryRepo, _cityRepo, _areaRepo, _priceListRepo, _documentSequenceManager
        );

        IGuidGenerator guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(x => Guid.NewGuid());

        IAbpLazyServiceProvider lazyServiceProvider = Substitute.For<IAbpLazyServiceProvider>();
        lazyServiceProvider.LazyGetRequiredService(typeof(IGuidGenerator)).Returns(guidGenerator);

        typeof(Volo.Abp.Domain.Services.DomainService)
            .GetProperty("LazyServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            ?.SetValue(_customerManager, lazyServiceProvider);
    }
    [QATest(scenario: "Ném ngoại lệ khi xóa khách hàng vẫn còn dư nợ.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Delete_Customer_With_Outstanding_Debt()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Customer customer = new(
            id, "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        customer.AddDebt(1000000m); // Add debt

        _customerRepository.GetAsync(id).Returns(customer);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.DeleteAsync(id);
        });
        ex.Code.ShouldBe("SupplyCoreERP:CannotDeleteCustomerWithOutstandingDebt");
    }
    [QATest(scenario: "Xóa khách hàng thành công khi không có dư nợ.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Delete_Customer_Successfully()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Customer customer = new(
            id, "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        _customerRepository.GetAsync(id).Returns(customer);

        // Act
        await _customerManager.DeleteAsync(id);

        // Assert
        await _customerRepository.Received(1).DeleteAsync(customer);
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có quốc gia không tồn tại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_Country_NotFound()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        Guid countryId = Guid.NewGuid();
        _countryRepo.AnyAsync(Arg.Any<Expression<Func<Country, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.UpdateAsync(
                customer, "Customer A Updated", null, null, null, null, CustomerType.Individual, null,
                null, countryId, null, null, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidCountry");
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có tỉnh/thành phố không tồn tại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_City_NotFound()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        Guid cityId = Guid.NewGuid();
        _cityRepo.FindAsync(cityId).Returns((City)null);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.UpdateAsync(
                customer, "Customer A Updated", null, null, null, null, CustomerType.Individual, null,
                null, null, cityId, null, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidCity");
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi vị trí city country mismatch.", feature: "Customer", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_CityCountry_Mismatch()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        Guid countryId = Guid.NewGuid();
        Guid cityId = Guid.NewGuid();

        _countryRepo.AnyAsync(Arg.Any<Expression<Func<Country, bool>>>()).Returns(true);
        City city = new(cityId, Guid.NewGuid(), "Tp. Ho Chi Minh"); // Different CountryId
        _cityRepo.FindAsync(cityId).Returns(city);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.UpdateAsync(
                customer, "Customer A Updated", null, null, null, null, CustomerType.Individual, null,
                null, countryId, cityId, null, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidCityCountry");
    }
    [QATest(scenario: "Ném ngoại lệ khi validate vị trí có quận/huyện không tồn tại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_Area_NotFound()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        Guid areaId = Guid.NewGuid();
        _areaRepo.FindAsync(areaId).Returns((Area)null);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.UpdateAsync(
                customer, "Customer A Updated", null, null, null, null, CustomerType.Individual, null,
                null, null, null, areaId, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidArea");
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi vị trí area city mismatch.", feature: "Customer", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Location_AreaCity_Mismatch()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        Guid cityId = Guid.NewGuid();
        Guid areaId = Guid.NewGuid();

        City city = new(cityId, Guid.NewGuid(), "Tp. Ho Chi Minh");
        _cityRepo.FindAsync(cityId).Returns(city);

        Area area = new(areaId, Guid.NewGuid(), "70000", "District 1"); // Different CityId
        _areaRepo.FindAsync(areaId).Returns(area);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.UpdateAsync(
                customer, "Customer A Updated", null, null, null, null, CustomerType.Individual, null,
                null, null, cityId, areaId, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidAreaCity");
    }
    [QATest(scenario: "Ném ngoại lệ khi chọn bảng giá không tồn tại trên hệ thống.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_PriceList_NotFound()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        Guid priceListId = Guid.NewGuid();
        _priceListRepo.AnyAsync(Arg.Any<Expression<Func<PriceList, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.UpdateAsync(
                customer, "Customer A Updated", null, null, null, null, CustomerType.Individual, null,
                null, null, null, null, null, priceListId: priceListId
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidPriceList");
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo KH trùng mã đã tồn tại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Customer_Code_Exists()
    {
        // Arrange
        _customerRepository.AnyAsync(Arg.Any<Expression<Func<Customer, bool>>>()).Returns(true);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.CheckCodeAndNameAsync("CUS-001", "Customer A");
        });
        ex.Code.ShouldBe("SupplyCoreERP:CustomerCodeAlreadyExists");
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo KH trùng tên đã tồn tại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Customer_Name_Exists()
    {
        // Arrange
        _customerRepository.AnyAsync(Arg.Any<Expression<Func<Customer, bool>>>())
            .Returns(x =>
            {
                string exprStr = x.Arg<Expression<Func<Customer, bool>>>().ToString();
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
            await _customerManager.CheckCodeAndNameAsync("CUS-001", "Customer A");
        });
        ex.Code.ShouldBe("SupplyCoreERP:CustomerNameAlreadyExists");
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo KH trùng số điện thoại đã tồn tại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_PhoneNumber_Exists()
    {
        // Arrange
        _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeCustomer).Returns("CUS-001");
        _customerRepository.AnyAsync(Arg.Any<Expression<Func<Customer, bool>>>())
            .Returns(x =>
            {
                string exprStr = x.Arg<Expression<Func<Customer, bool>>>().ToString();
                if (exprStr.Contains("PhoneNumber"))
                {
                    return true;
                }

                return false;
            });

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _customerManager.CreateAsync(
                "Customer A", "0909999999", null, null, null, CustomerType.Individual, null,
                null, null, null, null, null
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:PhoneNumberAlreadyExists");
    }
    [QATest(scenario: "Tạo mới khách hàng thành công qua Manager.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_Customer_Successfully()
    {
        // Arrange
        _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeCustomer).Returns("CUS-001");
        _customerRepository.AnyAsync(Arg.Any<Expression<Func<Customer, bool>>>()).Returns(false);

        // Act
        Customer customer = await _customerManager.CreateAsync(
            "Customer A", "0909999999", "customer@test.com", "Rep Name", Gender.Male,
            CustomerType.Individual, "123456", "Address", null, null, null, "Note",
            10000000m, 30, null
        );

        // Assert
        customer.ShouldNotBeNull();
        customer.Code.ShouldBe("CUS-001");
        customer.Name.ShouldBe("Customer A");
        customer.PhoneNumber.ShouldBe("0909999999");
        customer.Email.ShouldBe("customer@test.com");
        customer.RepresentativeName.ShouldBe("Rep Name");
        customer.Gender.ShouldBe(Gender.Male);
        customer.Type.ShouldBe(CustomerType.Individual);
        customer.TaxCode.ShouldBe("123456");
        customer.Address.ShouldBe("Address");
        customer.Note.ShouldBe("Note");
        customer.DebtLimit.ShouldBe(10000000m);
        customer.PaymentTermDays.ShouldBe(30);
    }
    [QATest(scenario: "Cập nhật thông tin khách hàng thành công khi không thay đổi số điện thoại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Update_Customer_Successfully_Without_Phone_Change()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", "0909999999", null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );

        // Act
        await _customerManager.UpdateAsync(
            customer, "Customer A Updated", "0909999999", "cust@test.com", "Rep Name B", Gender.Female,
            CustomerType.Organization, "654321", "Address B", null, null, null, "Note B", 5000000m, 15, null
        );

        // Assert
        customer.Name.ShouldBe("Customer A Updated");
        customer.PhoneNumber.ShouldBe("0909999999");
        customer.Email.ShouldBe("cust@test.com");
        customer.RepresentativeName.ShouldBe("Rep Name B");
        customer.Gender.ShouldBe(Gender.Female);
        customer.Type.ShouldBe(CustomerType.Organization);
        customer.TaxCode.ShouldBe("654321");
        customer.Address.ShouldBe("Address B");
        customer.Note.ShouldBe("Note B");
        customer.DebtLimit.ShouldBe(5000000m);
        customer.PaymentTermDays.ShouldBe(15);

        // Ensure no AnyAsync was called since phone wasn't changed
        await _customerRepository.DidNotReceive().AnyAsync(Arg.Any<Expression<Func<Customer, bool>>>());
    }
    [QATest(scenario: "Cập nhật thông tin khách hàng thành công khi thay đổi số điện thoại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Update_Customer_Successfully_With_Phone_Change()
    {
        // Arrange
        Customer customer = new(
            Guid.NewGuid(), "CUS-001", "Customer A", "0909999999", null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
        _customerRepository.AnyAsync(Arg.Any<Expression<Func<Customer, bool>>>()).Returns(false);

        // Act
        await _customerManager.UpdateAsync(
            customer, "Customer A Updated", "0908888888", "cust@test.com", "Rep Name B", Gender.Female,
            CustomerType.Organization, "654321", "Address B", null, null, null, "Note B", 5000000m, 15, null
        );

        // Assert
        customer.Name.ShouldBe("Customer A Updated");
        customer.PhoneNumber.ShouldBe("0908888888");
    }
}
