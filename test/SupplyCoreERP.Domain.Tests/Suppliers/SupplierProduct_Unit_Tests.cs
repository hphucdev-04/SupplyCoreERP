using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Suppliers;

public class SupplierProduct_Unit_Tests
{
    private readonly Guid _supplierProductId = Guid.NewGuid();
    private readonly Guid _supplierId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Guid _unitId = Guid.NewGuid();

    [Fact]
    public void ValidateConditions_WithValidConditions_ShouldNotThrow()
    {
        // Arrange
        var supplierProduct = new SupplierProduct(
            _supplierProductId,
            _supplierId,
            _productId,
            _unitId,
            5,
            true,
            "SP-CODE-001"
        );

        var condition1 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 1000m, 10m);
        var condition2 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 900m, 50m);
        var condition3 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 800m, 100m);

        supplierProduct.AddCondition(condition1);
        supplierProduct.AddCondition(condition2);
        supplierProduct.AddCondition(condition3);

        // Act & Assert
        Should.NotThrow(() => supplierProduct.ValidateConditions());
    }

    [Fact]
    public void ValidateConditions_InconsistentConversionFactor_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var supplierProduct = new SupplierProduct(
            _supplierProductId,
            _supplierId,
            _productId,
            _unitId,
            5,
            true,
            "SP-CODE-001"
        );

        // Khác hệ số quy đổi cho cùng một đơn vị tính _unitId
        var condition1 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 1000m, 10m);
        var condition2 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 10, 900m, 50m);

        supplierProduct.AddCondition(condition1);
        supplierProduct.AddCondition(condition2);

        // Act & Assert
        UserFriendlyException exception = Should.Throw<UserFriendlyException>(() => supplierProduct.ValidateConditions());
        exception.Message.ShouldContain("sử dụng chung một hệ số quy đổi");
    }

    [Fact]
    public void ValidateConditions_DuplicateMinOrderQuantity_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var supplierProduct = new SupplierProduct(
            _supplierProductId,
            _supplierId,
            _productId,
            _unitId,
            5,
            true,
            "SP-CODE-001"
        );

        // Trùng mốc MOQ là 10m
        var condition1 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 1000m, 10m);
        var condition2 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 900m, 10m);

        supplierProduct.AddCondition(condition1);
        supplierProduct.AddCondition(condition2);

        // Act & Assert
        UserFriendlyException exception = Should.Throw<UserFriendlyException>(() => supplierProduct.ValidateConditions());
        exception.Message.ShouldContain("mốc số lượng đặt tối thiểu là");
    }

    [Fact]
    public void ValidateConditions_ViolatingPriceRuleB_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var supplierProduct = new SupplierProduct(
            _supplierProductId,
            _supplierId,
            _productId,
            _unitId,
            5,
            true,
            "SP-CODE-001"
        );

        // Vi phạm Quy tắc B: MOQ lớn hơn (50) lại có giá đắt hơn (1100m so với 1000m)
        var condition1 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 1000m, 10m);
        var condition2 = new SupplierProductCondition(Guid.NewGuid(), _supplierProductId, _unitId, 1, 1100m, 50m);

        supplierProduct.AddCondition(condition1);
        supplierProduct.AddCondition(condition2);

        // Act & Assert
        UserFriendlyException exception = Should.Throw<UserFriendlyException>(() => supplierProduct.ValidateConditions());
        exception.Message.ShouldContain("phải nhỏ hơn hoặc bằng mức giá của mốc số lượng nhỏ hơn");
    }
}
