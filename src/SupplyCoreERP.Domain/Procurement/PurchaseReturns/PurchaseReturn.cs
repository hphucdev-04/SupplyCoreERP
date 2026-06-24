using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturnRequests;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Procurement.PurchaseReturns;

public class PurchaseReturn : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public virtual PurchaseOrder PurchaseOrder { get; private set; }
    public Guid SupplierId { get; private set; }
    public virtual Supplier Supplier { get; private set; }
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; private set; }
    public PurchaseReturnType ReturnType { get; private set; }
    public DateTime ReturnDate { get; private set; }
    public PurchaseReturnStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Note { get; private set; }
    public Guid? PurchaseReturnRequestId { get; private set; }
    public virtual PurchaseReturnRequest? PurchaseReturnRequest { get; private set; }
    public virtual ICollection<PurchaseReturnLine> Lines { get; private set; }

    public void SetRequestRelation(Guid requestId)
    {
        PurchaseReturnRequestId = requestId;
    }

    protected PurchaseReturn()
    {
        Lines = new List<PurchaseReturnLine>();
    }

    public PurchaseReturn(
        Guid id,
        string code,
        Guid purchaseOrderId,
        Guid supplierId,
        Guid warehouseId,
        PurchaseReturnType returnType,
        DateTime returnDate,
        string? note) : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code));
        PurchaseOrderId = purchaseOrderId;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        ReturnType = returnType;
        ReturnDate = returnDate;
        Note = note;
        Status = PurchaseReturnStatus.Draft;
        SubTotal = 0;
        TaxAmount = 0;
        TotalAmount = 0;
        Lines = new List<PurchaseReturnLine>();
    }

    public void UpdateInfo(Guid warehouseId, PurchaseReturnType returnType, DateTime returnDate, string? note)
    {
        if (Status != PurchaseReturnStatus.Draft && Status != PurchaseReturnStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể cập nhật thông tin chứng từ khi đang ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        WarehouseId = warehouseId;
        ReturnType = returnType;
        ReturnDate = returnDate;
        Note = note;

        // Nếu chuyển sang loại bể vỡ, tự động cập nhật khấu hao các dòng về 0
        if (ReturnType == PurchaseReturnType.Defective)
        {
            foreach (PurchaseReturnLine line in Lines)
            {
                line.UpdateInfo(line.Quantity, 0); // Khấu hao bắt buộc = 0
            }
        }
        RecalculateTotals();
    }

    public void AddLine(Guid id, Guid purchaseOrderLineId, Guid productId, Guid unitId, int conversionFactor, decimal quantity, decimal originalUnitPrice, decimal depreciationRate, decimal taxRate)
    {
        if (Status != PurchaseReturnStatus.Draft && Status != PurchaseReturnStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể thêm dòng hàng khi chứng từ ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        if (Lines.Any(x => x.PurchaseOrderLineId == purchaseOrderLineId))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateLine", "Dòng đơn hàng này đã tồn tại trong phiếu xuất trả!");
        }

        // Kiểm tra khấu hao theo loại hình trả hàng
        if (ReturnType == PurchaseReturnType.Defective && depreciationRate != 0)
        {
            throw new BusinessException("SupplyCoreERP:DefectiveCannotHaveDepreciation", "Hàng lỗi bể vỡ do nhà cung cấp không được phép tính khấu hao!");
        }

        PurchaseReturnLine line = new(id, Id, purchaseOrderLineId, productId, unitId, conversionFactor, quantity, originalUnitPrice, depreciationRate, taxRate);
        Lines.Add(line);
        RecalculateTotals();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != PurchaseReturnStatus.Draft && Status != PurchaseReturnStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể xóa dòng hàng khi chứng từ ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        PurchaseReturnLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng hàng.");
        }

        Lines.Remove(line);
        RecalculateTotals();
    }

    public void UpdateLine(Guid lineId, decimal quantity, decimal depreciationRate)
    {
        if (Status != PurchaseReturnStatus.Draft && Status != PurchaseReturnStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể sửa dòng hàng khi chứng từ ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        PurchaseReturnLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng chứng từ xuất trả!");
        }

        // Kiểm tra khấu hao theo loại hình trả hàng
        if (ReturnType == PurchaseReturnType.Defective && depreciationRate != 0)
        {
            throw new BusinessException("SupplyCoreERP:DefectiveCannotHaveDepreciation", "Hàng lỗi bể vỡ do nhà cung cấp không được phép tính khấu hao!");
        }

        line.UpdateInfo(quantity, depreciationRate);
        RecalculateTotals();
    }

    public void SendToApprove()
    {
        if (Status != PurchaseReturnStatus.Draft)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể gửi duyệt phiếu ở trạng thái Nháp!");
        }

        if (!Lines.Any())
        {
            throw new BusinessException("SupplyCoreERP:EmptyLines", "Phiếu xuất trả chưa có chi tiết dòng hàng!");
        }

        Status = PurchaseReturnStatus.PendingApproval;
    }

    public void Approve()
    {
        if (Status != PurchaseReturnStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể duyệt phiếu đang ở trạng thái Chờ duyệt!");
        }

        Status = PurchaseReturnStatus.Approved;
    }

    public void Reject()
    {
        if (Status != PurchaseReturnStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể từ chối phiếu đang ở trạng thái Chờ duyệt!");
        }

        Status = PurchaseReturnStatus.Rejected;
    }

    public void StartReturning()
    {
        if (Status != PurchaseReturnStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể bắt đầu xuất trả khi chứng từ đã được duyệt!");
        }

        Status = PurchaseReturnStatus.Returning;
    }

    public void Complete()
    {
        if (Status != PurchaseReturnStatus.Approved && Status != PurchaseReturnStatus.Returning)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể hoàn tất chứng từ đã được duyệt hoặc đang xuất hàng!");
        }

        Status = PurchaseReturnStatus.Completed;
    }

    private void RecalculateTotals()
    {
        SubTotal = Lines.Sum(x => x.TotalPrice);
        TaxAmount = Lines.Sum(x => x.TaxAmount);
        TotalAmount = Lines.Sum(x => x.FinalPrice);
    }
}
