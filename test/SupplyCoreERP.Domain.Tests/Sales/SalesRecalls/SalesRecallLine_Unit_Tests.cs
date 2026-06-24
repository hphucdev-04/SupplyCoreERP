using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Sales.SalesRecalls;

public class SalesRecallLine_Unit_Tests
{
    private readonly Guid _salesRecallId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Guid _salesOrderId = Guid.NewGuid();
    private readonly Guid _unitId = Guid.NewGuid();

    [QATest(scenario: "Khởi tạo dòng thu hồi hàng bán hợp lệ.", feature: "SalesRecall", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Initialize_SalesRecallLine_Correctly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        decimal quantity = 100m;
        decimal originalUnitPrice = 50000m;
        decimal taxRate = 10m;

        // Act
        SalesRecallLine line = new(
            id, _salesRecallId, _customerId, _salesOrderId,
            _unitId, 1, quantity, originalUnitPrice, taxRate
        );

        // Assert
        line.Id.ShouldBe(id);
        line.SalesRecallId.ShouldBe(_salesRecallId);
        line.CustomerId.ShouldBe(_customerId);
        line.SalesOrderId.ShouldBe(_salesOrderId);
        line.UnitId.ShouldBe(_unitId);
        line.ConversionFactor.ShouldBe(1);
        line.Quantity.ShouldBe(quantity);
        line.OriginalUnitPrice.ShouldBe(originalUnitPrice);
        line.TaxRate.ShouldBe(taxRate);
        line.RecalledQuantity.ShouldBe(0m);
        line.TotalPrice.ShouldBe(5000000m); // 100 * 50000
        line.TaxAmount.ShouldBe(500000m);    // 5000000 * 10%
        line.FinalPrice.ShouldBe(5500000m);   // 5000000 + 500000
    }

    [QATest(scenario: "Cộng dồn số lượng thu hồi hợp lệ.", feature: "SalesRecall", layer: "Domain", priority: "High")]
    [Fact]
    public void Should_Add_Recalled_Quantity_Successfully()
    {
        // Arrange
        SalesRecallLine line = CreateSampleLine(100m);

        // Act
        line.AddRecalledQuantity(40m);
        line.AddRecalledQuantity(30m);

        // Assert
        line.RecalledQuantity.ShouldBe(70m);
    }

    [QATest(scenario: "Ném ngoại lệ khi cộng dồn số lượng thu hồi bị âm.", feature: "SalesRecall", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_Exception_When_Adding_Negative_Quantity()
    {
        // Arrange
        SalesRecallLine line = CreateSampleLine(100m);

        // Act & Assert
        BusinessException ex = Assert.Throws<BusinessException>(() =>
        {
            line.AddRecalledQuantity(-5m);
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidQuantity");
    }

    [QATest(scenario: "Ném ngoại lệ khi tổng số lượng thu hồi tích lũy vượt quá yêu cầu thu hồi.", feature: "SalesRecall", layer: "Domain", priority: "High")]
    [Fact]
    public void Should_Throw_Exception_When_Recalled_Quantity_Exceeds_Required()
    {
        // Arrange
        SalesRecallLine line = CreateSampleLine(100m);
        line.AddRecalledQuantity(80m); // Đã thu hồi 80

        // Act & Assert
        // Tiếp tục thu hồi thêm 30 (tổng 110 > 100) -> kỳ vọng ném lỗi
        BusinessException ex = Assert.Throws<BusinessException>(() =>
        {
            line.AddRecalledQuantity(30m);
        });
        ex.Code.ShouldBe("SupplyCoreERP:ExceedsRecallQuantity");
        ex.Message.ShouldContain("không được vượt quá số lượng yêu cầu thu hồi");
    }

    private SalesRecallLine CreateSampleLine(decimal quantity)
    {
        return new SalesRecallLine(
            Guid.NewGuid(),
            _salesRecallId,
            _customerId,
            _salesOrderId,
            _unitId,
            1,
            quantity,
            10000m,
            10m
        );
    }
}
