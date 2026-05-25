using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Procurement.PurchaseOrders;

public class PurchaseOrder : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }

    public Guid SupplierId { get; private set; }
    public virtual Supplier Supplier { get; private set; }

    public Guid? PurchaseRequisitionId { get; private set; }
    public virtual PurchaseRequisition? PurchaseRequisition { get; private set; }

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
        DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note,
        Guid? purchaseRequisitionId = null) : base(id)
    {
        Code = code;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        PurchaseRequisitionId = purchaseRequisitionId;
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
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Chỉ có thể cập nhật đơn hàng khi đơn hàng đang ở trạng thái Nháp hoặc Chờ duyệt.");
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
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Chỉ có thể thêm dòng hàng khi đơn hàng đang ở trạng thái Nháp hoặc Chờ duyệt.");
        }

        PurchaseOrderLine line = new(id, Id, productId, unitId, conversionFactor, quantity, unitPrice, taxRate);
        Lines.Add(line);

        RecalculateTotal();
        return line;
    }

    public void UpdateLine(Guid lineId, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Chỉ có thể cập nhật dòng hàng khi đơn hàng đang ở trạng thái Nháp hoặc Chờ duyệt.");
        }

        PurchaseOrderLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng hàng.");
        }

        line.UpdateInfo(quantity, unitPrice, taxRate);
        RecalculateTotal();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Chỉ có thể xóa dòng hàng khi đơn hàng đang ở trạng thái Nháp hoặc Chờ duyệt.");
        }

        PurchaseOrderLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng hàng.");
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
            throw new BusinessException("SupplyCoreERP:OrderHasNoLines", "Đơn hàng chưa có sản phẩm nào!");
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
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Chỉ có thể bắt đầu nhận hàng khi đơn hàng đang ở trạng thái Đã duyệt!");
        }

        Status = PurchaseOrderStatus.Receiving;
    }
    public void Complete()
    {
        Status = PurchaseOrderStatus.Completed;
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






