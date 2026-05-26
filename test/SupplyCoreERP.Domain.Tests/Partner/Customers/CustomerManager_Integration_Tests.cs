using System;
using SupplyCoreERP;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.SeedData;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Partner.Customers;

public abstract class CustomerManager_Integration_Tests<TStartupModule> : SupplyCoreERPDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly CustomerManager _customerManager;
    private readonly IRepository<Customer, Guid> _customerRepository;

    protected CustomerManager_Integration_Tests()
    {
        _customerManager = GetRequiredService<CustomerManager>();
        _customerRepository = GetRequiredService<IRepository<Customer, Guid>>();
    }
    [QATest(scenario: "Tạo khách hàng thành công và tự động sinh mã code tăng dần.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_Customer_And_Generate_Customer_Code()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act
            Customer customer = await _customerManager.CreateAsync(
                "Customer New",
                "0989123456",
                "new_customer@test.com",
                "Rep Customer",
                Gender.Female,
                CustomerType.Individual,
                "MST-CUS-NEW",
                "123 Nguyen Hue",
                null,
                null,
                null,
                "Note New",
                100000000m,
                30,
                TestDataConsts.PriceListOfficialId
            );

            // Assert
            customer.ShouldNotBeNull();
            customer.Code.ShouldNotBeNullOrWhiteSpace();
            customer.Name.ShouldBe("Customer New");
            customer.PhoneNumber.ShouldBe("0989123456");
            customer.PriceListId.ShouldBe(TestDataConsts.PriceListOfficialId);
        });
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo trùng số điện thoại khách hàng.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Phone_Number_Already_Exists()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act & Assert
            BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _customerManager.CreateAsync(
                    "Customer Duplicate Phone",
                    "0909999999", // Duplicated from seed
                    "duplicate@test.com",
                    null,
                    null,
                    CustomerType.Individual,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null
                );
            });
            ex.Code.ShouldBe("SupplyCoreERP:PhoneNumberAlreadyExists");
        });
    }
    [QATest(scenario: "Xóa khách hàng thành công khi không có dư nợ.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Delete_Customer_Successfully()
    {
        Guid customerId = Guid.Empty;

        // UoW 1: Tạo customer và lưu vào DB
        await WithUnitOfWorkAsync(async () =>
        {
            Customer customer = await _customerManager.CreateAsync(
                "Customer Temp To Delete",
                "0901112223",
                null, null, null,
                CustomerType.Individual,
                null, null, null, null, null, null
            );
            await _customerRepository.InsertAsync(customer, autoSave: true);
            customerId = customer.Id;
        });

        // UoW 2: Thực hiện xóa
        await WithUnitOfWorkAsync(async () =>
        {
            await _customerManager.DeleteAsync(customerId);
        });

        // UoW 3: Kiểm tra đã xóa thật sự chưa
        await WithUnitOfWorkAsync(async () =>
        {
            Customer? deleted = await _customerRepository.FindAsync(customerId);
            deleted.ShouldBeNull();
        });
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo khách hàng liên kết bảng giá không tồn tại.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_Exception_When_Create_Customer_With_NonExistent_PriceList()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Guid invalidPriceListId = Guid.NewGuid();

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _customerManager.CreateAsync(
                    "Customer Wrong PriceList",
                    "0903334445",
                    null,
                    null,
                    null,
                    CustomerType.Individual,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    0,
                    0,
                    invalidPriceListId
                );
            });
            ex.Code.ShouldBe("SupplyCoreERP:InvalidPriceList");
        });
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi deleting khách hàng với còn dư nợ.", feature: "Customer", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Deleting_Customer_With_Outstanding_Debt()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Customer customer = await _customerManager.CreateAsync(
                "Customer Temp To Delete Debt",
                "0901112224",
                null, null, null,
                CustomerType.Individual,
                null, null, null, null, null, null,
                debtLimit: 1000000m,
                paymentTermDays: 30
            );
            customer.AddDebt(50000m);
            await _customerRepository.InsertAsync(customer, autoSave: true);

            // Act & Assert
            BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _customerManager.DeleteAsync(customer.Id);
            });
            ex.Code.ShouldBe("SupplyCoreERP:CannotDeleteCustomerWithOutstandingDebt");
        });
    }
    [QATest(scenario: "Cập nhật khách hàng thành công.", feature: "Customer", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Update_Customer_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Customer customer = await _customerManager.CreateAsync(
                "Customer For Update",
                "0901112225",
                null, null, null,
                CustomerType.Individual,
                null, null, null, null, null, null
            );
            await _customerRepository.InsertAsync(customer, autoSave: true);

            // Act
            await _customerManager.UpdateAsync(
                customer,
                "Customer Updated Name",
                "0901112225",
                "cust_upd@test.com",
                "New Rep",
                Gender.Female,
                CustomerType.Organization,
                "MST-UPD-1",
                "123 New Address",
                null, null, null,
                "New Note",
                50000000m,
                45,
                TestDataConsts.PriceListOfficialId
            );

            // Assert
            Customer updated = await _customerRepository.GetAsync(customer.Id);
            updated.Name.ShouldBe("Customer Updated Name");
            updated.Email.ShouldBe("cust_upd@test.com");
            updated.RepresentativeName.ShouldBe("New Rep");
            updated.Gender.ShouldBe(Gender.Female);
            updated.Type.ShouldBe(CustomerType.Organization);
            updated.TaxCode.ShouldBe("MST-UPD-1");
            updated.Address.ShouldBe("123 New Address");
            updated.Note.ShouldBe("New Note");
            updated.DebtLimit.ShouldBe(50000000m);
            updated.PaymentTermDays.ShouldBe(45);
            updated.PriceListId.ShouldBe(TestDataConsts.PriceListOfficialId);
        });
    }
    [QATest(scenario: "Check mã code and tên Ném ngoại lệ business ngoại lệ khi mã code tồn tại.", feature: "Customer", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_CheckCodeAndName_Throw_BusinessException_When_Code_Exists()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act & Assert
            BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _customerManager.CheckCodeAndNameAsync("CUS-001", "Khách hàng mới");
            });
            ex.Code.ShouldBe("SupplyCoreERP:CustomerCodeAlreadyExists");
        });
    }
    [QATest(scenario: "Check mã code and tên Ném ngoại lệ business ngoại lệ khi tên tồn tại.", feature: "Customer", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_CheckCodeAndName_Throw_BusinessException_When_Name_Exists()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act & Assert
            BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _customerManager.CheckCodeAndNameAsync("CUS-002", "Khách Hàng A");
            });
            ex.Code.ShouldBe("SupplyCoreERP:CustomerNameAlreadyExists");
        });
    }
}
