using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Partner.Suppliers;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Procurement.PurchaseReturnRequests;

public class PurchaseReturnRequest : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public Guid SupplierId { get; private set; }
    public virtual Supplier Supplier { get; private set; }
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; private set; }
    public PurchaseReturnType ReturnType { get; private set; }
    public DateTime RequestDate { get; private set; }
    public PurchaseReturnRequestStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Note { get; private set; }
    public virtual ICollection<PurchaseReturnRequestLine> Lines { get; private set; }

    protected PurchaseReturnRequest()
    {
        Lines = new List<PurchaseReturnRequestLine>();
    }

    public PurchaseReturnRequest(
        Guid id,
        string code,
        Guid supplierId,
        Guid warehouseId,
        PurchaseReturnType returnType,
        DateTime requestDate,
        string? note) : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code));
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        ReturnType = returnType;
        RequestDate = requestDate;
        Note = note;
        Status = PurchaseReturnRequestStatus.Draft;
        SubTotal = 0;
        TaxAmount = 0;
        TotalAmount = 0;
        Lines = new List<PurchaseReturnRequestLine>();
    }

    public void UpdateInfo(Guid warehouseId, PurchaseReturnType returnType, DateTime requestDate, string? note)
    {
        if (Status != PurchaseReturnRequestStatus.Draft && Status != PurchaseReturnRequestStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể cập nhật thông tin yêu cầu khi đang ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        WarehouseId = warehouseId;
        ReturnType = returnType;
        RequestDate = requestDate;
        Note = note;

        // Nếu chuyển sang loại bể vỡ, tự động cập nhật khấu hao các dòng về 0
        if (ReturnType == PurchaseReturnType.Defective)
        {
            foreach (PurchaseReturnRequestLine line in Lines)
            {
                line.UpdateInfo(line.Quantity, 0); // Khấu hao bắt buộc = 0
            }
        }
        RecalculateTotals();
    }

    public void AddLine(
        Guid id,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        Guid purchaseOrderId,
        Guid purchaseOrderLineId,
        decimal quantity,
        decimal originalUnitPrice,
        decimal depreciationRate,
        decimal taxRate)
    {
        if (Status != PurchaseReturnRequestStatus.Draft && Status != PurchaseReturnRequestStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể thêm dòng hàng khi yêu cầu ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        if (Lines.Any(x => x.PurchaseOrderLineId == purchaseOrderLineId))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateLine", "Dòng đơn hàng này đã tồn tại trong yêu cầu xuất trả!");
        }

        // Kiểm tra khấu hao theo loại hình trả hàng
        if (ReturnType == PurchaseReturnType.Defective && depreciationRate != 0)
        {
            throw new BusinessException("SupplyCoreERP:DefectiveCannotHaveDepreciation", "Hàng lỗi bể vỡ do nhà cung cấp không được phép tính khấu hao!");
        }

        var line = new PurchaseReturnRequestLine(
            id,
            Id,
            productId,
            unitId,
            conversionFactor,
            purchaseOrderId,
            purchaseOrderLineId,
            quantity,
            originalUnitPrice,
            depreciationRate,
            taxRate
        );

        Lines.Add(line);
        RecalculateTotals();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != PurchaseReturnRequestStatus.Draft && Status != PurchaseReturnRequestStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể xóa dòng hàng khi yêu cầu ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        PurchaseReturnRequestLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng chi tiết yêu cầu.");
        }

        Lines.Remove(line);
        RecalculateTotals();
    }

    public void UpdateLine(Guid lineId, decimal quantity, decimal depreciationRate)
    {
        if (Status != PurchaseReturnRequestStatus.Draft && Status != PurchaseReturnRequestStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể sửa dòng hàng khi yêu cầu ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        PurchaseReturnRequestLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng chi tiết yêu cầu!");
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
        if (Status != PurchaseReturnRequestStatus.Draft)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể gửi duyệt yêu cầu ở trạng thái Nháp!");
        }

        if (!Lines.Any())
        {
            throw new BusinessException("SupplyCoreERP:EmptyLines", "Yêu cầu trả hàng chưa có chi tiết dòng hàng!");
        }

        Status = PurchaseReturnRequestStatus.PendingApproval;
    }

    public void Approve()
    {
        if (Status != PurchaseReturnRequestStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể duyệt yêu cầu đang ở trạng thái Chờ duyệt!");
        }

        Status = PurchaseReturnRequestStatus.Approved;
    }

    public void Reject()
    {
        if (Status != PurchaseReturnRequestStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể từ chối yêu cầu đang ở trạng thái Chờ duyệt!");
        }

        Status = PurchaseReturnRequestStatus.Rejected;
    }

    public void MarkAsProcessed()
    {
        if (Status != PurchaseReturnRequestStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể xử lý hoàn tất yêu cầu khi đã được duyệt!");
        }

        Status = PurchaseReturnRequestStatus.Processed;
    }

    private void RecalculateTotals()
    {
        SubTotal = Lines.Sum(x => x.TotalPrice);
        TaxAmount = Lines.Sum(x => x.TaxAmount);
        TotalAmount = Lines.Sum(x => x.FinalPrice);
    }
}
