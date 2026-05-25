using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.SeedData;
using SupplyCoreERP.Suppliers;
using SupplyCoreERP.Suppliers.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Partner.Suppliers;

public abstract class SupplierAppService_Integration_Tests<TStartupModule> : SupplyCoreERPApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ISupplierAppService _supplierAppService;

    protected SupplierAppService_Integration_Tests()
    {
        _supplierAppService = GetRequiredService<ISupplierAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Suppliers()
    {
        // Act
        PagedResultDto<SupplierDto> result = await _supplierAppService.GetListAsync(new GetSupplierListDto
        {
            MaxResultCount = 10,
            SkipCount = 0
        });

        // Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(x => x.Id == TestDataConsts.SupplierAId);
    }

    [Fact]
    public async Task Should_Create_Supplier_With_Valid_DTO()
    {
        // Act
        SupplierDetailDto result = await _supplierAppService.CreateAsync(new CreateUpdateSupplierDto
        {
            Name = "Supplier New E2E",
            TaxCode = "MST-NEW-E2E",
            PhoneNumber = "0909000111",
            Email = "e2e@supplier.com",
            RepresentativeName = "Rep E2E",
            Gender = Gender.Male,
            Note = "Note E2E",
            Address = "123 Le Loi",
            DebtLimit = 100000000m,
            PaymentTermDays = 15
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Supplier New E2E");
        result.Code.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Update_Supplier_Successfully()
    {
        // Act
        SupplierDetailDto result = await _supplierAppService.UpdateAsync(
            TestDataConsts.SupplierAId,
            new CreateUpdateSupplierDto
            {
                Name = "Nhà Cung Cấp A Cập Nhật",
                TaxCode = "MST-123456",
                PhoneNumber = "0901234567",
                Email = "supplier_a_new@test.com",
                RepresentativeName = "Nguyen Van A New",
                Gender = Gender.Male,
                Note = "Note A New",
                Address = "123 Nguyen Hue",
                DebtLimit = 600000000m,
                PaymentTermDays = 45
            }
        );

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Nhà Cung Cấp A Cập Nhật");
        result.DebtLimit.ShouldBe(600000000m);
    }
}
