using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Customers;
using SupplyCoreERP.Customers.Dtos;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.SeedData;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Partner.Customers;

public abstract class CustomerAppService_Integration_Tests<TStartupModule> : SupplyCoreERPApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ICustomerAppService _customerAppService;

    protected CustomerAppService_Integration_Tests()
    {
        _customerAppService = GetRequiredService<ICustomerAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Customers()
    {
        // Act
        PagedResultDto<CustomerDto> result = await _customerAppService.GetListAsync(new GetCustomerListDto
        {
            MaxResultCount = 10,
            SkipCount = 0
        });

        // Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(x => x.Id == TestDataConsts.CustomerAId);
    }

    [Fact]
    public async Task Should_Create_Customer_With_Valid_DTO()
    {
        // Act
        CustomerDetailDto result = await _customerAppService.CreateAsync(new CreateUpdateCustomerDto
        {
            Name = "Customer New E2E",
            PhoneNumber = "0989000111",
            Email = "e2e@customer.com",
            RepresentativeName = "Rep E2E",
            Gender = Gender.Female,
            Type = CustomerType.Individual,
            TaxCode = "MST-NEW-E2E",
            Address = "123 Ly Thuong Kiet",
            Note = "Note E2E",
            DebtLimit = 100000000m,
            PaymentTermDays = 15,
            PriceListId = TestDataConsts.PriceListOfficialId
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Customer New E2E");
        result.Code.ShouldNotBeNullOrWhiteSpace();
        result.PriceListId.ShouldBe(TestDataConsts.PriceListOfficialId);
    }

    [Fact]
    public async Task Should_Update_Customer_Successfully()
    {
        // Act
        CustomerDetailDto result = await _customerAppService.UpdateAsync(
            TestDataConsts.CustomerAId,
            new CreateUpdateCustomerDto
            {
                Name = "Khách Hàng A Cập Nhật",
                PhoneNumber = "0909999999",
                Email = "customer_a_new@test.com",
                RepresentativeName = "Nguyen Van B New",
                Gender = Gender.Male,
                Type = CustomerType.Organization,
                TaxCode = "MST-CUS-123",
                Address = "456 Le Loi New",
                Note = "Note A New",
                DebtLimit = 400000000m,
                PaymentTermDays = 45,
                PriceListId = TestDataConsts.PriceListOfficialId
            }
        );

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Khách Hàng A Cập Nhật");
        result.DebtLimit.ShouldBe(400000000m);
    }
}
