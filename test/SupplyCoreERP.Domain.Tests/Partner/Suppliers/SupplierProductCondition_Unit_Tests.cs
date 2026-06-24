using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Partner.Suppliers;

public class SupplierProductCondition_Unit_Tests
{
    private static readonly Guid SupplierProductId = Guid.NewGuid();
    private static readonly Guid UnitId = Guid.NewGuid();

    #region Constructor
    [QATest(scenario: "Tạo mới condition với hợp lệ tham số.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Create_Condition_With_Valid_Parameters()
    {
        Guid id = Guid.NewGuid();

        SupplierProductCondition condition = new(
            id, SupplierProductId, UnitId,
            conversionFactor: 50,
            standardPrice: 100_000m,
            minOrderQuantity: 10m);

        condition.Id.ShouldBe(id);
        condition.SupplierProductId.ShouldBe(SupplierProductId);
        condition.UnitId.ShouldBe(UnitId);
        condition.ConversionFactor.ShouldBe(50);
        condition.StandardPrice.ShouldBe(100_000m);
        condition.LastPurchasePrice.ShouldBe(100_000m); // initialized from StandardPrice
        condition.MinOrderQuantity.ShouldBe(10m);
    }
    [QATest(scenario: "Ném ngoại lệ khi conversion factor không hợp lệ.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_ConversionFactor_Invalid()
    {
        Assert.Throws<BusinessException>(() =>
            new SupplierProductCondition(Guid.NewGuid(), SupplierProductId, UnitId, 0, 100_000m, 10m))
            .Code.ShouldBe("SupplyCoreERP:InvalidConversionFactor");

        Assert.Throws<BusinessException>(() =>
            new SupplierProductCondition(Guid.NewGuid(), SupplierProductId, UnitId, -1, 100_000m, 10m))
            .Code.ShouldBe("SupplyCoreERP:InvalidConversionFactor");
    }
    [QATest(scenario: "Ném ngoại lệ khi standard price bị âm.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_StandardPrice_Negative()
    {
        Assert.Throws<BusinessException>(() =>
            new SupplierProductCondition(Guid.NewGuid(), SupplierProductId, UnitId, 1, -1m, 10m))
            .Code.ShouldBe("SupplyCoreERP:InvalidStandardPrice");
    }
    [QATest(scenario: "Allow standard price bằng 0.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Allow_StandardPrice_Zero()
    {
        SupplierProductCondition condition = new(
            Guid.NewGuid(), SupplierProductId, UnitId, 1, 0m, 10m);

        condition.StandardPrice.ShouldBe(0m);
    }
    [QATest(scenario: "Ném ngoại lệ khi min order quantity không hợp lệ.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_MinOrderQuantity_Invalid()
    {
        Assert.Throws<BusinessException>(() =>
            new SupplierProductCondition(Guid.NewGuid(), SupplierProductId, UnitId, 1, 100_000m, 0m))
            .Code.ShouldBe("SupplyCoreERP:InvalidMinOrderQuantity");

        Assert.Throws<BusinessException>(() =>
            new SupplierProductCondition(Guid.NewGuid(), SupplierProductId, UnitId, 1, 100_000m, -5m))
            .Code.ShouldBe("SupplyCoreERP:InvalidMinOrderQuantity");
    }

    #endregion

    #region UpdateCondition
    [QATest(scenario: "Cập nhật condition thành công.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Update_Condition_Successfully()
    {
        SupplierProductCondition condition = CreateSampleCondition();

        condition.UpdateCondition(80_000m, 50m);

        condition.StandardPrice.ShouldBe(80_000m);
        condition.MinOrderQuantity.ShouldBe(50m);
    }
    [QATest(scenario: "Ném ngoại lệ khi Cập nhật condition với không hợp lệ values.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_UpdateCondition_With_Invalid_Values()
    {
        SupplierProductCondition condition = CreateSampleCondition();

        Assert.Throws<BusinessException>(() =>
            condition.UpdateCondition(-1m, 10m))
            .Code.ShouldBe("SupplyCoreERP:InvalidStandardPrice");

        Assert.Throws<BusinessException>(() =>
            condition.UpdateCondition(100_000m, 0m))
            .Code.ShouldBe("SupplyCoreERP:InvalidMinOrderQuantity");
    }

    #endregion

    #region UpdateLastPurchasePrice
    [QATest(scenario: "Cập nhật last purchase price thành công.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Update_LastPurchasePrice_Successfully()
    {
        SupplierProductCondition condition = CreateSampleCondition();

        condition.UpdateLastPurchasePrice(95_000m);

        condition.LastPurchasePrice.ShouldBe(95_000m);
    }
    [QATest(scenario: "Ném ngoại lệ khi Cập nhật last purchase price bị âm.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_When_UpdateLastPurchasePrice_Negative()
    {
        SupplierProductCondition condition = CreateSampleCondition();

        Assert.Throws<BusinessException>(() =>
            condition.UpdateLastPurchasePrice(-1m))
            .Code.ShouldBe("SupplyCoreERP:InvalidLastPurchasePrice");
    }

    #endregion

    #region Helpers

    private static SupplierProductCondition CreateSampleCondition()
    {
        return new SupplierProductCondition(
            Guid.NewGuid(), SupplierProductId, UnitId, 1, 100_000m, 10m);
    }

    #endregion
}
