using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Suppliers;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Orders.PO;

public class PurchaseOrder : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }

    public Guid SupplierId { get; private set; }
    public virtual Supplier Supplier { get; private set; }

    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? DueDate { get; private set; }

    public PurchaseOrderStatus Status { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Note { get; private set; }

    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; private set; }

    public virtual ICollection<PurchaseOrderLine> Lines { get; private set; }
    protected PurchaseOrder() { Lines = new List<PurchaseOrderLine>(); }

    public PurchaseOrder(
        Guid id, string code, Guid supplierId, Guid warehouseId,
        DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note) : base(id)
    {
        Code = code;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DueDate = dueDate;
        Note = note;
        Status = PurchaseOrderStatus.Draft;
        SubTotal = 0;
        TaxAmount = 0;
        TotalAmount = 0;
        Lines = new List<PurchaseOrderLine>();
    }
    public void UpdateInfo(Guid warehouseId, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Chỉ có thể sửa đơn hàng khi đang ở trạng thái Nháp hoặc Chờ duyệt.");
        }

        WarehouseId = warehouseId;
        ExpectedDeliveryDate = expectedDeliveryDate;
        DueDate = dueDate;
        Note = note;
    }

    #region PurchaseOrder Line
    public PurchaseOrderLine AddLine(Guid id, Guid productId, Guid unitId, int conversionFactor, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Chỉ được thêm dòng hàng khi đơn đang Nháp hoặc Chờ duyệt.");
        }

        var line = new PurchaseOrderLine(id, Id, productId, unitId, conversionFactor, quantity, unitPrice, taxRate);
        Lines.Add(line);

        RecalculateTotal();
        return line;
    }

    public void UpdateLine(Guid lineId, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Không thể sửa dòng hàng khi đơn đã duyệt.");
        }

        PurchaseOrderLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new UserFriendlyException("Không tìm thấy dòng hàng.");
        }

        line.UpdateInfo(quantity, unitPrice, taxRate);
        RecalculateTotal();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Không thể xóa dòng hàng khi đơn hàng đã duyệt.");
        }

        PurchaseOrderLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new UserFriendlyException("Không tìm thấy dòng hàng.");
        }

        Lines.Remove(line);
        RecalculateTotal();
    }
    #endregion

    #region Work flow
    public void SendToApprove()
    {
        if (!Lines.Any())
        {
            throw new UserFriendlyException("Đơn hàng chưa có sản phẩm nào!");
        }

        Status = PurchaseOrderStatus.PendingApproval;
    }

    public void Approve()
    {
        Status = PurchaseOrderStatus.Approved;
    }
    public void StartReceiving()
    {
        if (Status != PurchaseOrderStatus.Approved)
        {
            throw new UserFriendlyException(
                "Chỉ có thể bắt đầu nhận hàng khi đơn đang ở trạng thái Đã duyệt!");
        }

        Status = PurchaseOrderStatus.Receiving;
    }
    public void Complete()
    {
        Status = PurchaseOrderStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == PurchaseOrderStatus.Completed)
        {
            throw new UserFriendlyException("Đơn hàng đã hoàn tất, không thể hủy!");
        }

        if (Status == PurchaseOrderStatus.Receiving)
        {
            throw new UserFriendlyException("Đơn hàng đang nhập kho, yêu cầu Kho xóa phiếu trước!");
        }

        Status = PurchaseOrderStatus.Canceled;
    }
    #endregion

    #region Helper
    private void RecalculateTotal()
    {
        SubTotal = Lines.Sum(x => x.TotalPrice);
        TaxAmount = Lines.Sum(x => x.TaxAmount);
        TotalAmount = SubTotal + TaxAmount;
    }
    #endregion
}
