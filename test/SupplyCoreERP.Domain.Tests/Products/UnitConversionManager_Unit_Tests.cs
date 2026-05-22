using System;
using System.Linq;
using Shouldly;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Medicines;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Products;

public class UnitConversionManager_Unit_Tests
{
    private readonly UnitConversionManager _conversionManager;
    private readonly Guid _baseUnitId;
    private readonly Guid _subUnit1Id;
    private readonly Guid _subUnit2Id;
    private readonly Medicine _product;

    public UnitConversionManager_Unit_Tests()
    {
        _conversionManager = new UnitConversionManager();
        _baseUnitId = Guid.NewGuid();
        _subUnit1Id = Guid.NewGuid();
        _subUnit2Id = Guid.NewGuid();

        // Khởi tạo một đối tượng Medicine (kế thừa từ Product) làm dữ liệu thử nghiệm
        _product = new Medicine(
            Guid.NewGuid(),
            Guid.NewGuid(), // categoryId
            Guid.NewGuid(), // manufacturerId
            "MED-001",
            "Paracetamol 500mg",
            _baseUnitId,
            Guid.NewGuid(), // dosageFormId
            "VD-12345-20",
            UsageRoute.Oral,
            StorageCondition.Normal,
            isPrescriptionDrug: false
        );

        // Thêm các đơn vị quy đổi phụ
        // Unit 1: Vỉ (Blister) - level 1, conversionFactor = 10 (1 Vỉ = 10 Viên)
        _product.AddUnit(Guid.NewGuid(), _subUnit1Id, conversionFactor: 10, level: 1);

        // Unit 2: Hộp (Box) - level 2, conversionFactor = 10 (1 Hộp = 10 Vỉ -> Tuyệt đối: 10 * 10 = 100 Viên)
        _product.AddUnit(Guid.NewGuid(), _subUnit2Id, conversionFactor: 10, level: 2);
    }

    [Fact]
    public void ConvertToBaseQuantity_WithBaseUnit_ShouldReturnSameQuantity()
    {
        // Act
        decimal result = _conversionManager.ConvertToBaseQuantity(_product, _baseUnitId, 50);

        // Assert
        result.ShouldBe(50);
    }

    [Fact]
    public void ConvertToBaseQuantity_WithSubUnit_ShouldMultiplyFactor()
    {
        // Act
        decimal resultBlister = _conversionManager.ConvertToBaseQuantity(_product, _subUnit1Id, 5); // 5 Vỉ
        decimal resultBox = _conversionManager.ConvertToBaseQuantity(_product, _subUnit2Id, 3); // 3 Hộp

        // Assert
        resultBlister.ShouldBe(50); // 5 * 10 = 50 Viên
        resultBox.ShouldBe(300); // 3 * 100 = 300 Viên
    }

    [Fact]
    public void ConvertFromBaseQuantity_WithBaseUnit_ShouldReturnSameQuantity()
    {
        // Act
        decimal result = _conversionManager.ConvertFromBaseQuantity(_product, _baseUnitId, 75);

        // Assert
        result.ShouldBe(75);
    }

    [Fact]
    public void ConvertFromBaseQuantity_WithSubUnit_ShouldDivideAndRoundCorrectly()
    {
        // Act & Assert
        // 1. Chia hết: 50 Viên -> 5 Vỉ (hệ số 10)
        _conversionManager.ConvertFromBaseQuantity(_product, _subUnit1Id, 50).ShouldBe(5);

        // 2. Chia lẻ làm tròn chuẩn thương mại: 55 Viên -> 0.55 Hộp (hệ số 100)
        _conversionManager.ConvertFromBaseQuantity(_product, _subUnit2Id, 55).ShouldBe(0.55m);

        // 3. Phép chia lẻ tuần hoàn: 10 Viên -> 0.1 Hộp (hệ số 100)
        _conversionManager.ConvertFromBaseQuantity(_product, _subUnit2Id, 10).ShouldBe(0.1m);
    }

    [Fact]
    public void ConvertBetweenUnits_ShouldWorkCorrectly()
    {
        // Act: Đổi chéo từ 2 Hộp sang Vỉ: 2 Hộp = 200 Viên = 20 Vỉ
        decimal result = _conversionManager.ConvertBetweenUnits(_product, _subUnit2Id, _subUnit1Id, 2);

        // Assert
        result.ShouldBe(20);
    }

    [Fact]
    public void ConvertToBaseQuantity_WithUnregisteredUnit_ShouldThrowException()
    {
        // Arrange
        Guid unregisteredUnitId = Guid.NewGuid();

        // Act & Assert
        BusinessException exception = Should.Throw<BusinessException>(() =>
        {
            _conversionManager.ConvertToBaseQuantity(_product, unregisteredUnitId, 10);
        });

        exception.Code.ShouldBe("SupplyCoreERP:UnitNotFound");
    }

    [Fact]
    public void GetConversionFactor_ShouldReturnCorrectFactors()
    {
        // Act & Assert
        _conversionManager.GetConversionFactor(_product, _baseUnitId).ShouldBe(1);
        _conversionManager.GetConversionFactor(_product, _subUnit1Id).ShouldBe(10);
        _conversionManager.GetConversionFactor(_product, _subUnit2Id).ShouldBe(100);
    }

    [Fact]
    public void RemoveUnit_NotMaxLevel_ShouldThrowCannotDeleteLowerLevelUnit()
    {
        // Arrange & Act & Assert
        // Ở Constructor, _product có level 1 (_subUnit1Id) và level 2 (_subUnit2Id).
        // Xóa _subUnit1Id (Level 1) khi Level 2 vẫn còn tồn tại phải ném ngoại lệ.
        BusinessException exception = Should.Throw<BusinessException>(() =>
        {
            _product.RemoveUnit(_subUnit1Id);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotDeleteLowerLevelUnit");
        exception.Message.ShouldContain("cấp độ cao nhất trước");
    }

    [Fact]
    public void AddUnit_ShouldAutomaticallyAssignIncrementingLevels()
    {
        // Arrange
        var testProduct = new Medicine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-TST", "Test Auto Levels",
            _baseUnitId, Guid.NewGuid(), "VD-TST", UsageRoute.Oral, StorageCondition.Normal, false
        );

        var unitA = Guid.NewGuid();
        var unitB = Guid.NewGuid();
        var unitC = Guid.NewGuid();

        // Act
        testProduct.AddUnit(Guid.NewGuid(), unitA, 10, 999); // Truyền level 999 nhưng hệ thống phải tự gán level = 1
        testProduct.AddUnit(Guid.NewGuid(), unitB, 10, 999); // Tự gán level = 2
        testProduct.AddUnit(Guid.NewGuid(), unitC, 10, 999); // Tự gán level = 3

        // Assert
        testProduct.Units.FirstOrDefault(u => u.UnitId == unitA).Level.ShouldBe(1);
        testProduct.Units.FirstOrDefault(u => u.UnitId == unitB).Level.ShouldBe(2);
        testProduct.Units.FirstOrDefault(u => u.UnitId == unitC).Level.ShouldBe(3);
    }
}
