using System;
using SupplyCoreERP;
using System.Linq;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Partner.Suppliers;

public class SupplierProduct_Unit_Tests
{
    private static readonly Guid SupplierId = Guid.NewGuid();
    private static readonly Guid ProductId = Guid.NewGuid();
    private static readonly Guid UnitId = Guid.NewGuid();

    #region Constructor / UpdateInfo
    [QATest(scenario: "Tạo mới nhà cung cấp sản phẩm với hợp lệ tham số.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Create_SupplierProduct_With_Valid_Parameters()
    {
        var id = Guid.NewGuid();

        var sp = new SupplierProduct(id, SupplierId, ProductId, UnitId, 7, true, "Test note");

        sp.Id.ShouldBe(id);
        sp.SupplierId.ShouldBe(SupplierId);
        sp.ProductId.ShouldBe(ProductId);
        sp.DefaultUnitId.ShouldBe(UnitId);
        sp.LeadTimeDays.ShouldBe(7);
        sp.IsPreferred.ShouldBeTrue();
        sp.IsActive.ShouldBeTrue();
        sp.Note.ShouldBe("Test note");
        sp.Conditions.ShouldNotBeNull();
        sp.Conditions.Count.ShouldBe(0);
    }
    [QATest(scenario: "Clamp bị âm lead time days to bằng 0.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Clamp_Negative_LeadTimeDays_To_Zero()
    {
        var sp = new SupplierProduct(Guid.NewGuid(), SupplierId, ProductId, UnitId, -5);

        sp.LeadTimeDays.ShouldBe(0);
    }
    [QATest(scenario: "Cập nhật info.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Update_Info()
    {
        SupplierProduct sp = CreateSampleProduct();
        var newUnitId = Guid.NewGuid();

        sp.UpdateInfo(newUnitId, 14, true, "Updated note");

        sp.DefaultUnitId.ShouldBe(newUnitId);
        sp.LeadTimeDays.ShouldBe(14);
        sp.IsPreferred.ShouldBeTrue();
        sp.Note.ShouldBe("Updated note");
    }
    [QATest(scenario: "Clamp bị âm lead time days on Cập nhật info.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Clamp_Negative_LeadTimeDays_On_UpdateInfo()
    {
        SupplierProduct sp = CreateSampleProduct();

        sp.UpdateInfo(Guid.NewGuid(), -3, false, null);

        sp.LeadTimeDays.ShouldBe(0);
    }

    #endregion

    #region AddCondition / RemoveCondition
    [QATest(scenario: "Thêm condition.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Add_Condition()
    {
        SupplierProduct sp = CreateSampleProduct();
        SupplierProductCondition condition = CreateCondition(sp.Id, UnitId, 1, 100_000m, 10m);

        sp.AddCondition(condition);

        sp.Conditions.Count.ShouldBe(1);
        sp.Conditions.First().ShouldBe(condition);
    }
    [QATest(scenario: "Loại bỏ existing condition.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Remove_Existing_Condition()
    {
        SupplierProduct sp = CreateSampleProduct();
        SupplierProductCondition condition = CreateCondition(sp.Id, UnitId, 1, 100_000m, 10m);
        sp.AddCondition(condition);

        sp.RemoveCondition(condition.Id);

        sp.Conditions.Count.ShouldBe(0);
    }
    [QATest(scenario: "Do nothing khi Loại bỏ non existent condition.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Do_Nothing_When_Remove_NonExistent_Condition()
    {
        SupplierProduct sp = CreateSampleProduct();
        SupplierProductCondition condition = CreateCondition(sp.Id, UnitId, 1, 100_000m, 10m);
        sp.AddCondition(condition);

        sp.RemoveCondition(Guid.NewGuid()); // non-existent

        sp.Conditions.Count.ShouldBe(1);
    }

    #endregion

    #region SetPreferred / SetActive
    [QATest(scenario: "Set ưu tiên.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Set_Preferred()
    {
        SupplierProduct sp = CreateSampleProduct();
        sp.IsPreferred.ShouldBeFalse();

        sp.SetPreferred(true);
        sp.IsPreferred.ShouldBeTrue();

        sp.SetPreferred(false);
        sp.IsPreferred.ShouldBeFalse();
    }
    [QATest(scenario: "Set hoạt động.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Set_Active()
    {
        SupplierProduct sp = CreateSampleProduct();
        sp.IsActive.ShouldBeTrue();

        sp.SetActive(false);
        sp.IsActive.ShouldBeFalse();

        sp.SetActive(true);
        sp.IsActive.ShouldBeTrue();
    }

    #endregion

    #region ValidateConditions
    [QATest(scenario: "Pass Validate điều kiện khi Empty.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Pass_ValidateConditions_When_Empty()
    {
        SupplierProduct sp = CreateSampleProduct();

        // Không throw
        sp.ValidateConditions();
    }
    [QATest(scenario: "Pass validate conditions với hợp lệ conditions.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Pass_ValidateConditions_With_Valid_Conditions()
    {
        SupplierProduct sp = CreateSampleProduct();
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 1, 100_000m, 10m));
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 1, 90_000m, 100m));  // MOQ cao hơn, giá thấp hơn

        sp.ValidateConditions();

        // Không throw = pass
    }
    [QATest(scenario: "Ném ngoại lệ khi inconsistent conversion factors.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_InconsistentConversionFactors()
    {
        SupplierProduct sp = CreateSampleProduct();
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 1, 100_000m, 10m));   // factor = 1
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 50, 90_000m, 100m));  // factor = 50, same unit

        Assert.Throws<BusinessException>(() => sp.ValidateConditions())
            .Code.ShouldBe("SupplyCoreERP:InconsistentConversionFactors");
    }
    [QATest(scenario: "Ném ngoại lệ khi trùng lặp min order quantity.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_DuplicateMinOrderQuantity()
    {
        SupplierProduct sp = CreateSampleProduct();
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 1, 100_000m, 10m));
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 1, 90_000m, 10m));  // same MOQ = 10

        Assert.Throws<BusinessException>(() => sp.ValidateConditions())
            .Code.ShouldBe("SupplyCoreERP:DuplicateMinOrderQuantity");
    }
    [QATest(scenario: "Ném ngoại lệ khi inconsistent pricing.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_InconsistentPricing()
    {
        SupplierProduct sp = CreateSampleProduct();
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 1, 90_000m, 10m));
        sp.AddCondition(CreateCondition(sp.Id, UnitId, 1, 100_000m, 100m));  // MOQ cao hơn nhưng giá CAO hơn

        Assert.Throws<BusinessException>(() => sp.ValidateConditions())
            .Code.ShouldBe("SupplyCoreERP:InconsistentPricing");
    }

    #endregion

    #region Helpers

    private static SupplierProduct CreateSampleProduct()
    {
        return new SupplierProduct(Guid.NewGuid(), SupplierId, ProductId, UnitId, 5);
    }

    private static SupplierProductCondition CreateCondition(
        Guid supplierProductId, Guid unitId, int conversionFactor,
        decimal standardPrice, decimal minOrderQuantity)
    {
        return new SupplierProductCondition(
            Guid.NewGuid(), supplierProductId, unitId, conversionFactor, standardPrice, minOrderQuantity);
    }

    #endregion
}
