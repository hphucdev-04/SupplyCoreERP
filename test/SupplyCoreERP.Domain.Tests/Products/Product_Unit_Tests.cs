using System;
using Shouldly;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Medicines;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Products;

public class Product_Unit_Tests
{
    private readonly Guid _baseUnitId;
    private readonly Guid _subUnitId;
    private readonly Medicine _product;

    public Product_Unit_Tests()
    {
        _baseUnitId = Guid.NewGuid();
        _subUnitId = Guid.NewGuid();

        _product = new Medicine(
            Guid.NewGuid(),
            Guid.NewGuid(), // categoryId
            Guid.NewGuid(), // manufacturerId
            "MED-888",
            "Paracetamol 500mg",
            _baseUnitId,
            Guid.NewGuid(), // dosageFormId
            "VD-88888-25",
            UsageRoute.Oral,
            StorageCondition.Normal,
            isPrescriptionDrug: false
        );
    }

    [Fact]
    public void UpdateInfo_DuplicateBaseUnitInUnits_ShouldThrowDuplicateBaseUnitInUnits()
    {
        // Arrange: Thêm đơn vị phụ trùng với baseUnitId mới sắp đổi sang
        Guid newBaseUnitId = _subUnitId;
        _product.AddUnit(Guid.NewGuid(), _subUnitId, conversionFactor: 10, level: 1);

        // Act & Assert
        BusinessException exception = Should.Throw<BusinessException>(() =>
        {
            _product.UpdateInfo("Paracetamol New", Guid.NewGuid(), Guid.NewGuid(), newBaseUnitId);
        });

        exception.Code.ShouldBe("SupplyCoreERP:DuplicateBaseUnitInUnits");
    }

    [Fact]
    public void UpdateInfo_ValidBaseUnit_ShouldUpdateInfo()
    {
        // Arrange
        Guid newBaseUnitId = Guid.NewGuid();

        // Act
        _product.UpdateInfo("Paracetamol New", _product.CategoryId, _product.ManufacturerId, newBaseUnitId);

        // Assert
        _product.Name.ShouldBe("Paracetamol New");
        _product.BaseUnitId.ShouldBe(newBaseUnitId);
    }
}
