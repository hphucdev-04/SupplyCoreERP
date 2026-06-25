using System;
using Shouldly;
using SupplyCoreERP.Catalog.Medicines;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Catalog.Products;

public class UnitConversionManager_Unit_Tests
{
    private readonly UnitConversionManager _unitConversionManager;
    private readonly Guid _baseUnitId = Guid.NewGuid();
    private readonly Guid _blisterUnitId = Guid.NewGuid();
    private readonly Guid _boxUnitId = Guid.NewGuid();

    public UnitConversionManager_Unit_Tests()
    {
        _unitConversionManager = new UnitConversionManager();
    }

    [QATest(scenario: "Tính toán hệ số quy đổi tuyệt đối cho đơn vị nhiều tầng.", feature: "UnitConversion", layer: "Domain", priority: "High")]
    [Fact]
    public void Should_Calculate_Absolute_Conversion_Factor_For_Multi_Level_Units()
    {
        // Arrange
        Medicine product = CreateSampleProduct();

        // Act & Assert
        // Đơn vị gốc = 1
        _unitConversionManager.GetConversionFactor(product, _baseUnitId).ShouldBe(1);

        // Đơn vị phụ cấp 1 = 10
        _unitConversionManager.GetConversionFactor(product, _blisterUnitId).ShouldBe(10);

        // Đơn vị phụ cấp 2 = 100
        _unitConversionManager.GetConversionFactor(product, _boxUnitId).ShouldBe(100);
    }

    [QATest(scenario: "Quy đổi số lượng từ đơn vị phụ sang đơn vị gốc.", feature: "UnitConversion", layer: "Domain", priority: "High")]
    [Fact]
    public void Should_Convert_To_Base_Quantity_Correctly()
    {
        // Arrange
        Medicine product = CreateSampleProduct();

        // Act & Assert
        // Quy đổi từ đơn vị gốc (12 Viên -> 12 Viên)
        _unitConversionManager.ConvertToBaseQuantity(product, _baseUnitId, 12m).ShouldBe(12m);

        // Quy đổi từ đơn vị cấp 1 (3 Vỉ * 10 -> 30 Viên)
        _unitConversionManager.ConvertToBaseQuantity(product, _blisterUnitId, 3m).ShouldBe(30m);

        // Quy đổi từ đơn vị cấp 2 (5 Hộp * 100 -> 500 Viên)
        _unitConversionManager.ConvertToBaseQuantity(product, _boxUnitId, 5m).ShouldBe(500m);
    }

    [QATest(scenario: "Quy đổi số lượng từ đơn vị gốc sang đơn vị phụ và làm tròn.", feature: "UnitConversion", layer: "Domain", priority: "High")]
    [Fact]
    public void Should_Convert_From_Base_Quantity_Correctly()
    {
        // Arrange
        Medicine product = CreateSampleProduct();

        // Act & Assert
        // 350 Viên -> 3.5 Hộp (350 / 100)
        _unitConversionManager.ConvertFromBaseQuantity(product, _boxUnitId, 350m).ShouldBe(3.5m);

        // 255 Viên -> 2.55 Hộp (255 / 100)
        _unitConversionManager.ConvertFromBaseQuantity(product, _boxUnitId, 255m).ShouldBe(2.55m);

        // 15 Viên -> 1.5 Vỉ (15 / 10)
        _unitConversionManager.ConvertFromBaseQuantity(product, _blisterUnitId, 15m).ShouldBe(1.5m);
    }

    [QATest(scenario: "Quy đổi chéo giữa hai đơn vị tính bất kỳ của sản phẩm.", feature: "UnitConversion", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Convert_Between_Units_Correctly()
    {
        // Arrange
        Medicine product = CreateSampleProduct();

        // Act & Assert
        // 2 Hộp -> Vỉ: 2 Hộp = 200 Viên -> 20 Vỉ (200 / 10)
        _unitConversionManager.ConvertBetweenUnits(product, _boxUnitId, _blisterUnitId, 2m).ShouldBe(20m);

        // 15 Vỉ -> Hộp: 15 Vỉ = 150 Viên -> 1.5 Hộp (150 / 100)
        _unitConversionManager.ConvertBetweenUnits(product, _blisterUnitId, _boxUnitId, 15m).ShouldBe(1.5m);
    }

    [QATest(scenario: "Tính toán thể tích chiếm dụng kho dựa trên đơn vị tính.", feature: "UnitConversion", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Calculate_Volume_Correctly()
    {
        // Arrange
        // BaseUnitVolume = 0.05m
        Medicine product = CreateSampleProduct(0.05m);

        // Act & Assert
        // 1. Tính cho đơn vị gốc (10 Viên * 0.05 -> 0.5)
        _unitConversionManager.CalculateVolume(product, _baseUnitId, 10m).ShouldBe(0.5m);

        // 2. Tính cho đơn vị phụ tự động nhân dồn hệ số (10 Vỉ * 10 (Hệ số) * 0.05 (Thể tích gốc) -> 5.0)
        _unitConversionManager.CalculateVolume(product, _blisterUnitId, 10m).ShouldBe(5.0m);

        // 3. Tính cho đơn vị phụ được cấu hình thể tích riêng (Ví dụ BOX được thiết lập Volume = 8.0 trong AddUnit)
        _unitConversionManager.CalculateVolume(product, _boxUnitId, 2m).ShouldBe(16.0m); // 2 * 8.0 = 16.0
    }

    [QATest(scenario: "Ném ngoại lệ business khi quy đổi đơn vị không thuộc cấu hình sản phẩm.", feature: "UnitConversion", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_Exception_When_Unit_Not_Belong_To_Product()
    {
        // Arrange
        Medicine product = CreateSampleProduct();
        Guid invalidUnitId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
        {
            _unitConversionManager.GetConversionFactor(product, invalidUnitId);
        }).Code.ShouldBe("SupplyCoreERP:UnitNotFound");
    }

    [QATest(scenario: "Ném ngoại lệ business khi quy đổi sang đơn vị gốc với đơn vị nguồn rỗng.", feature: "UnitConversion", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_Exception_When_Source_Unit_Is_Empty()
    {
        // Arrange
        Medicine product = CreateSampleProduct();

        // Act & Assert
        BusinessException ex = Assert.Throws<BusinessException>(() =>
        {
            _unitConversionManager.ConvertToBaseQuantity(product, Guid.Empty, 10m);
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidUnitId");
        ex.Message.ShouldContain("Đơn vị tính nguồn không hợp lệ");
    }

    [QATest(scenario: "Ném ngoại lệ business khi quy đổi từ đơn vị gốc sang đơn vị phụ với đơn vị đích rỗng.", feature: "UnitConversion", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_Exception_When_Target_Unit_Is_Empty()
    {
        // Arrange
        Medicine product = CreateSampleProduct();

        // Act & Assert
        BusinessException ex = Assert.Throws<BusinessException>(() =>
        {
            _unitConversionManager.ConvertFromBaseQuantity(product, Guid.Empty, 100m);
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidUnitId");
        ex.Message.ShouldContain("Đơn vị tính đích không hợp lệ");
    }

    private Medicine CreateSampleProduct(decimal baseUnitVolume = 0.1m)
    {
        Medicine product = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "MED-UNIT-TEST",
            "Thuoc de test quy doi",
            _baseUnitId,
            Guid.NewGuid(),
            "SDK-UNIT-TEST",
            Enums.Medicines.UsageRoute.Oral,
            Enums.Medicines.StorageCondition.Normal,
            false,
            baseUnitVolume
        );

        // Thêm đơn vị phụ cấp 1 (Vỉ): Level = 1, ConversionFactor = 10 (1 vỉ = 10 viên), Volume = 0 
        product.AddUnit(Guid.NewGuid(), _blisterUnitId, 10, 1, 0);

        // Thêm đơn vị phụ cấp 2 (Hộp): Level = 2, ConversionFactor = 10 (1 hộp = 10 vỉ = 100 viên), Volume = 8.0 
        product.AddUnit(Guid.NewGuid(), _boxUnitId, 10, 2, 8.0m);

        return product;
    }
}
