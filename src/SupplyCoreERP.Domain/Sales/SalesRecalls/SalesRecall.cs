using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Sales.SalesRecalls;

public class SalesRecall : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public string RecallDecisionNumber { get; private set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }
    public Guid? ProductBatchId { get; private set; }
    public virtual ProductBatch? ProductBatch { get; protected set; }
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; protected set; }
    public DateTime RecallDate { get; private set; }
    public RecallLevel Level { get; private set; }
    public DateTime Deadline { get; private set; }
    public SalesRecallStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Note { get; private set; }
    public virtual ICollection<SalesRecallLine> Lines { get; private set; }

    public bool IsOverdue => DateTime.Now > Deadline && Status != SalesRecallStatus.Completed;

    protected SalesRecall()
    {
        Lines = new List<SalesRecallLine>();
    }

    public SalesRecall(
        Guid id,
        string code,
        string recallDecisionNumber,
        Guid productId,
        Guid? productBatchId,
        Guid warehouseId,
        DateTime recallDate,
        RecallLevel level,
        string? note) : base(id)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code));
        RecallDecisionNumber = Check.NotNullOrWhiteSpace(recallDecisionNumber, nameof(recallDecisionNumber));
        ProductId = productId;
        ProductBatchId = productBatchId;
        WarehouseId = warehouseId;
        RecallDate = recallDate;
        Note = note;
        Status = SalesRecallStatus.Draft;
        TotalAmount = 0;
        Lines = new List<SalesRecallLine>();

        SetLevelAndCalculateDeadline(level);
    }

    public void UpdateInfo(Guid warehouseId, DateTime recallDate, RecallLevel level, string recallDecisionNumber, string? note)
    {
        if (Status != SalesRecallStatus.Draft && Status != SalesRecallStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể cập nhật thông tin chứng từ khi đang ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        WarehouseId = warehouseId;
        RecallDate = recallDate;
        RecallDecisionNumber = Check.NotNullOrWhiteSpace(recallDecisionNumber, nameof(recallDecisionNumber));
        Note = note;

        SetLevelAndCalculateDeadline(level);
    }

    public void AddLine(Guid id, Guid customerId, Guid salesOrderId, Guid unitId, int conversionFactor, decimal quantity, decimal originalUnitPrice, decimal taxRate)
    {
        if (Status != SalesRecallStatus.Draft && Status != SalesRecallStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể thêm chi tiết khách hàng thu hồi khi chứng từ ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        if (Lines.Any(x => x.CustomerId == customerId && x.SalesOrderId == salesOrderId))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateLine", "Khách hàng với đơn hàng bán này đã được khai báo thu hồi!");
        }

        SalesRecallLine line = new(id, Id, customerId, salesOrderId, unitId, conversionFactor, quantity, originalUnitPrice, taxRate);
        Lines.Add(line);
        RecalculateTotals();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != SalesRecallStatus.Draft && Status != SalesRecallStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể xóa dòng chi tiết khi chứng từ ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        SalesRecallLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng chi tiết thu hồi.");
        }

        Lines.Remove(line);
        RecalculateTotals();
    }

    public void UpdateLine(Guid lineId, decimal quantity)
    {
        if (Status != SalesRecallStatus.Draft && Status != SalesRecallStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể sửa số lượng khi chứng từ ở trạng thái Nháp hoặc Chờ duyệt!");
        }

        SalesRecallLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy chi tiết khách hàng thu hồi!");
        }

        line.UpdateQuantity(quantity);
        RecalculateTotals();
    }

    public void SendToApprove()
    {
        if (Status != SalesRecallStatus.Draft)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể gửi duyệt quyết định ở trạng thái Nháp!");
        }

        if (!Lines.Any())
        {
            throw new BusinessException("SupplyCoreERP:EmptyLines", "Quyết định thu hồi chưa có thông tin chi tiết khách hàng hoàn trả!");
        }

        Status = SalesRecallStatus.PendingApproval;
    }

    public void Approve()
    {
        if (Status != SalesRecallStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể duyệt quyết định đang ở trạng thái Chờ duyệt!");
        }

        Status = SalesRecallStatus.Approved;
    }

    public void Reject()
    {
        if (Status != SalesRecallStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể từ chối quyết định đang ở trạng thái Chờ duyệt!");
        }

        Status = SalesRecallStatus.Rejected;
    }

    public void StartRecalling()
    {
        if (Status != SalesRecallStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể bắt đầu thu hồi khi quyết định đã được duyệt!");
        }

        Status = SalesRecallStatus.Recalling;
    }

    public void Complete()
    {
        if (Status != SalesRecallStatus.Approved && Status != SalesRecallStatus.Recalling)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể hoàn tất quyết định đã được duyệt hoặc đang thu hồi!");
        }

        Status = SalesRecallStatus.Completed;
    }

    private void SetLevelAndCalculateDeadline(RecallLevel level)
    {
        Level = level;
        Deadline = level switch
        {
            RecallLevel.Level1 => RecallDate.Date.AddDays(3),
            RecallLevel.Level2 => RecallDate.Date.AddDays(15),
            RecallLevel.Level3 => RecallDate.Date.AddDays(30),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }

    private void RecalculateTotals()
    {
        TotalAmount = Lines.Sum(x => x.FinalPrice);
    }
}
