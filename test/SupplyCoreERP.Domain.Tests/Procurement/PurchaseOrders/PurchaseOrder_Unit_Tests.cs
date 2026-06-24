using System;
using System.Linq;
using Shouldly;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Procurement.PurchaseOrders;

public class PurchaseOrder_Unit_Tests
{
    private readonly Guid _supplierId = Guid.NewGuid();
    private readonly Guid _warehouseId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Guid _unitId = Guid.NewGuid();

    [QATest(scenario: "Tạo mới PurchaseOrder với tham số hợp lệ.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Initialize_PurchaseOrder_Correctly()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        string code = "PO-2026-0001";
        DateTime orderDate = DateTime.Now;
        DateTime expectedDeliveryDate = orderDate.AddDays(3);
        DateTime dueDate = orderDate.AddDays(30);
        string note = "Ghi chu don hang test";

        // Act
        PurchaseOrder order = new(
            id, code, _supplierId, _warehouseId,
            orderDate, expectedDeliveryDate, dueDate, note
        );

        // Assert
        order.Id.ShouldBe(id);
        order.Code.ShouldBe(code);
        order.SupplierId.ShouldBe(_supplierId);
        order.WarehouseId.ShouldBe(_warehouseId);
        order.OrderDate.ShouldBe(orderDate);
        order.ExpectedDeliveryDate.ShouldBe(expectedDeliveryDate);
        order.DueDate.ShouldBe(dueDate);
        order.Note.ShouldBe(note);
        order.Status.ShouldBe(PurchaseOrderStatus.Draft);
        order.SubTotal.ShouldBe(0);
        order.TaxAmount.ShouldBe(0);
        order.TotalAmount.ShouldBe(0);
        order.Lines.ShouldBeEmpty();
    }

    [QATest(scenario: "Cập nhật thông tin đơn hàng.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Update_PurchaseOrder_Info()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();
        Guid newWarehouseId = Guid.NewGuid();
        DateTime newExpectedDelivery = DateTime.Now.AddDays(5);
        DateTime newDue = DateTime.Now.AddDays(45);
        string newNote = "Ghi chu cap nhat";

        // Act
        order.UpdateInfo(newWarehouseId, newExpectedDelivery, newDue, newNote);

        // Assert
        order.WarehouseId.ShouldBe(newWarehouseId);
        order.ExpectedDeliveryDate.ShouldBe(newExpectedDelivery);
        order.DueDate.ShouldBe(newDue);
        order.Note.ShouldBe(newNote);
    }

    [QATest(scenario: "Thêm dòng hàng vào PurchaseOrder và tự động tính toán tổng tiền.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public void Should_Add_PurchaseOrderLine_And_Recalculate_Totals()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();
        Guid lineId = Guid.NewGuid();

        // Act
        PurchaseOrderLine line = order.AddLine(lineId, _productId, _unitId, 1, 10m, 150000m, 10m);

        // Assert
        order.Lines.Count.ShouldBe(1);
        order.Lines.First().ShouldBe(line);

        line.Id.ShouldBe(lineId);
        line.PurchaseOrderId.ShouldBe(order.Id);
        line.ProductId.ShouldBe(_productId);
        line.UnitId.ShouldBe(_unitId);
        line.ConversionFactor.ShouldBe(1);
        line.Quantity.ShouldBe(10m);
        line.UnitPrice.ShouldBe(150000m);
        line.TaxRate.ShouldBe(10m);
        line.ReceivedQuantity.ShouldBe(0);

        // Tự động tính toán tiền trên line
        line.TotalPrice.ShouldBe(1500000m); // 10 * 150k
        line.TaxAmount.ShouldBe(150000m);   // 1500k * 10%
        line.FinalPrice.ShouldBe(1650000m);  // 1500k + 150k

        // Tự động tính toán tổng tiền trên PO
        order.SubTotal.ShouldBe(1500000m);
        order.TaxAmount.ShouldBe(150000m);
        order.TotalAmount.ShouldBe(1650000m);
    }

    [QATest(scenario: "Cập nhật số lượng, đơn giá và thuế suất dòng hàng.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public void Should_Update_PurchaseOrderLine_And_Recalculate_Totals()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();
        Guid lineId = Guid.NewGuid();
        order.AddLine(lineId, _productId, _unitId, 1, 10m, 150000m, 10m);

        // Act
        order.UpdateLine(lineId, 20m, 140000m, 5m);

        // Assert
        PurchaseOrderLine line = order.Lines.First();
        line.Quantity.ShouldBe(20m);
        line.UnitPrice.ShouldBe(140000m);
        line.TaxRate.ShouldBe(5m);

        line.TotalPrice.ShouldBe(2800000m); // 20 * 140k
        line.TaxAmount.ShouldBe(140000m);   // 2800k * 5%
        line.FinalPrice.ShouldBe(2940000m);

        order.SubTotal.ShouldBe(2800000m);
        order.TaxAmount.ShouldBe(140000m);
        order.TotalAmount.ShouldBe(2940000m);
    }

    [QATest(scenario: "Xóa dòng hàng khỏi PurchaseOrder.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Remove_PurchaseOrderLine_And_Recalculate_Totals()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();
        Guid lineId1 = Guid.NewGuid();
        Guid lineId2 = Guid.NewGuid();
        order.AddLine(lineId1, _productId, _unitId, 1, 10m, 100000m, 10m); // 1.1M
        order.AddLine(lineId2, _productId, _unitId, 1, 5m, 200000m, 10m);  // 1.1M

        order.TotalAmount.ShouldBe(2200000m);

        // Act
        order.RemoveLine(lineId1);

        // Assert
        order.Lines.Count.ShouldBe(1);
        order.Lines.Any(x => x.Id == lineId1).ShouldBeFalse();
        order.TotalAmount.ShouldBe(1100000m); // Chỉ còn line 2
    }

    [QATest(scenario: "Ném ngoại lệ business khi thêm dòng hàng với giá trị biên không hợp lệ.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_BusinessException_When_Adding_Line_With_Invalid_Values()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();

        // Act & Assert
        // Số lượng <= 0
        var exQty = Assert.Throws<BusinessException>(() =>
        {
            order.AddLine(Guid.NewGuid(), _productId, _unitId, 1, 0m, 100000m, 10m);
        });
        exQty.Code.ShouldBe("SupplyCoreERP:InvalidQuantity");

        // Đơn giá âm
        var exPrice = Assert.Throws<BusinessException>(() =>
        {
            order.AddLine(Guid.NewGuid(), _productId, _unitId, 1, 10m, -1000m, 10m);
        });
        exPrice.Code.ShouldBe("SupplyCoreERP:InvalidUnitPrice");

        // Thuế suất âm
        var exTax = Assert.Throws<BusinessException>(() =>
        {
            order.AddLine(Guid.NewGuid(), _productId, _unitId, 1, 10m, 100000m, -5m);
        });
        exTax.Code.ShouldBe("SupplyCoreERP:InvalidTaxRate");

        // Hệ số quy đổi <= 0
        var exFactor = Assert.Throws<BusinessException>(() =>
        {
            order.AddLine(Guid.NewGuid(), _productId, _unitId, 0, 10m, 100000m, 10m);
        });
        exFactor.Code.ShouldBe("SupplyCoreERP:InvalidConversionFactor");
    }

    [QATest(scenario: "Ném ngoại lệ business khi chỉnh sửa đơn hàng có trạng thái không được phép.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_BusinessException_When_Modifying_Order_In_Non_Editable_Status()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();
        order.AddLine(Guid.NewGuid(), _productId, _unitId, 1, 10m, 100000m, 10m);
        order.SendToApprove();
        order.Approve(); // Chuyển sang Approved (đã duyệt)

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
        {
            order.AddLine(Guid.NewGuid(), _productId, _unitId, 1, 5m, 100000m, 10m);
        }).Code.ShouldBe("SupplyCoreERP:InvalidOrderStatus");

        Assert.Throws<BusinessException>(() =>
        {
            order.UpdateInfo(Guid.NewGuid(), null, null, "Note");
        }).Code.ShouldBe("SupplyCoreERP:InvalidOrderStatus");
    }

    [QATest(scenario: "Ném ngoại lệ business khi gửi duyệt đơn hàng không có dòng hàng nào.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_BusinessException_When_Sending_Empty_Order_To_Approve()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();

        // Act & Assert
        var ex = Assert.Throws<BusinessException>(() =>
        {
            order.SendToApprove();
        });
        ex.Code.ShouldBe("SupplyCoreERP:OrderHasNoLines");
    }

    [QATest(scenario: "Chuyển trạng thái đơn hàng theo workflow đúng tuần tự.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Transition_OrderStatus_Correctly_In_Workflow()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();
        order.AddLine(Guid.NewGuid(), _productId, _unitId, 1, 10m, 100000m, 10m);

        order.Status.ShouldBe(PurchaseOrderStatus.Draft);

        // SendToApprove
        order.SendToApprove();
        order.Status.ShouldBe(PurchaseOrderStatus.PendingApproval);

        // Approve
        order.Approve();
        order.Status.ShouldBe(PurchaseOrderStatus.Approved);

        // StartReceiving
        order.StartReceiving();
        order.Status.ShouldBe(PurchaseOrderStatus.Receiving);

        // Complete
        order.Complete();
        order.Status.ShouldBe(PurchaseOrderStatus.Completed);
    }

    [QATest(scenario: "Ném ngoại lệ business khi bắt đầu nhận hàng nhưng trạng thái đơn không phải Approved.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_BusinessException_When_StartReceiving_From_Invalid_Status()
    {
        // Arrange
        PurchaseOrder order = CreateSampleOrder();
        order.AddLine(Guid.NewGuid(), _productId, _unitId, 1, 10m, 100000m, 10m);

        // Đang ở Draft
        Assert.Throws<BusinessException>(() =>
        {
            order.StartReceiving();
        }).Code.ShouldBe("SupplyCoreERP:InvalidOrderStatus");
    }

    private PurchaseOrder CreateSampleOrder()
    {
        return new PurchaseOrder(
            Guid.NewGuid(),
            "PO-TEST-001",
            _supplierId,
            _warehouseId,
            DateTime.Now,
            DateTime.Now.AddDays(3),
            DateTime.Now.AddDays(30),
            "Ghi chu mau"
        );
    }
}
