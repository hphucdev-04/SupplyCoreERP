using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventories.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Orders.PR;

public class PurchaseRequisition : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public DateTime RequestedDate { get; private set; }
    public DateTime? RequiredDate { get; private set; }
    public PurchaseRequisitionStatus Status { get; private set; }
    public string? Note { get; private set; }

    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; protected set; }

    public virtual ICollection<PurchaseRequisitionLine> Lines { get; private set; }

    protected PurchaseRequisition()
    {
        Lines = new List<PurchaseRequisitionLine>();
    }

    public PurchaseRequisition(
        Guid id,
        string code,
        Guid warehouseId,
        DateTime requestedDate,
        DateTime? requiredDate,
        string? note) : base(id)
    {
        Code = code;
        WarehouseId = warehouseId;
        RequestedDate = requestedDate;
        RequiredDate = requiredDate;
        Note = note;
        Status = PurchaseRequisitionStatus.Draft;
        Lines = new List<PurchaseRequisitionLine>();
    }

    public void UpdateInfo(Guid warehouseId, DateTime? requiredDate, string? note)
    {
        if (Status != PurchaseRequisitionStatus.Draft && Status != PurchaseRequisitionStatus.Rejected)
        {
            throw new UserFriendlyException("Chỉ có thể sửa yêu cầu khi đang ở trạng thái Nháp hoặc Bị từ chối.");
        }

        WarehouseId = warehouseId;
        RequiredDate = requiredDate;
        Note = note;
    }

    #region Line Management
    public PurchaseRequisitionLine AddLine(Guid id, Guid productId, Guid unitId, decimal quantity, string? note)
    {
        if (Status != PurchaseRequisitionStatus.Draft && Status != PurchaseRequisitionStatus.Rejected)
        {
            throw new UserFriendlyException("Chỉ được thêm sản phẩm khi yêu cầu đang Nháp hoặc Bị từ chối.");
        }

        var line = new PurchaseRequisitionLine(id, Id, productId, unitId, quantity, note);
        Lines.Add(line);
        return line;
    }

    public void UpdateLine(Guid lineId, decimal quantity, string? note)
    {
        if (Status != PurchaseRequisitionStatus.Draft && Status != PurchaseRequisitionStatus.Rejected)
        {
            throw new UserFriendlyException("Chỉ được sửa sản phẩm khi yêu cầu đang Nháp hoặc Bị từ chối.");
        }

        PurchaseRequisitionLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new UserFriendlyException("Không tìm thấy dòng yêu cầu.");
        }

        line.UpdateInfo(quantity, note);
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != PurchaseRequisitionStatus.Draft && Status != PurchaseRequisitionStatus.Rejected)
        {
            throw new UserFriendlyException("Chỉ được xóa sản phẩm khi yêu cầu đang Nháp hoặc Bị từ chối.");
        }

        PurchaseRequisitionLine? line = Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new UserFriendlyException("Không tìm thấy dòng yêu cầu.");
        }

        Lines.Remove(line);
    }
    #endregion

    #region Workflow
    public void SendToApprove()
    {
        if (!Lines.Any())
        {
            throw new UserFriendlyException("Yêu cầu chưa có sản phẩm nào!");
        }

        if (Status != PurchaseRequisitionStatus.Draft && Status != PurchaseRequisitionStatus.Rejected)
        {
            throw new UserFriendlyException("Trạng thái hiện tại không cho phép gửi duyệt.");
        }
        Status = PurchaseRequisitionStatus.PendingApproval;
    }

    public void Approve()
    {
        if (Status != PurchaseRequisitionStatus.PendingApproval)
        {
            throw new UserFriendlyException("Yêu cầu chưa được gửi duyệt.");
        }
        Status = PurchaseRequisitionStatus.Approved;
    }

    public void Reject()
    {
        if (Status != PurchaseRequisitionStatus.PendingApproval)
        {
            throw new UserFriendlyException("Chỉ có thể từ chối yêu cầu đang chờ duyệt.");
        }
        Status = PurchaseRequisitionStatus.Rejected;
    }

    public void UpdateOrderingStatus()
    {
        if (!Lines.Any())
        {
            return;
        }

        bool allOrdered = Lines.All(x => x.OrderedQuantity >= x.Quantity);
        bool anyOrdered = Lines.Any(x => x.OrderedQuantity > 0);

        if (allOrdered)
        {
            Status = PurchaseRequisitionStatus.Ordered;
        }
        else if (anyOrdered)
        {
            Status = PurchaseRequisitionStatus.PartialOrdered;
        }
        else
        {
            Status = PurchaseRequisitionStatus.Approved;
        }
    }
    #endregion
}
