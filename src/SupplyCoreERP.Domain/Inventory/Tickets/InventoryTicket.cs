using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets.Events;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Tickets;

public class InventoryTicket : FullAuditedAggregateRoot<Guid>
{
    public string TicketNumber { get; private set; }
    public TicketType Type { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; protected set; }
    public Guid? ReferenceDocumentId { get; private set; }
    public string? ReferenceDocumentNumber { get; private set; }
    public string? Note { get; private set; }
    public virtual ICollection<InventoryTicketLine> Lines { get; protected set; }

    protected InventoryTicket()
    {
        Lines = new List<InventoryTicketLine>();
    }

    public InventoryTicket(Guid id, string ticketNumber, TicketType type, Guid warehouseId, Guid? refDocId, string? refDocNumber, string? note) : base(id)
    {
        TicketNumber = ticketNumber;
        Type = type;
        WarehouseId = warehouseId;
        ReferenceDocumentId = refDocId;
        ReferenceDocumentNumber = refDocNumber;
        Note = note;
        Status = ApprovalStatus.Draft;
        Lines = new List<InventoryTicketLine>();
    }

    public void RequestApprove()
    {
        if (Status != ApprovalStatus.Draft)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicketStatus", "Chỉ các phiếu ở trạng thái Nháp mới có thể được gửi phê duyệt.");
        }

        Status = ApprovalStatus.Pending;
    }

    public void Execute(List<InventoryTicketLine> ticketLines)
    {
        if (Status != ApprovalStatus.Pending)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicketStatus", "Chỉ các phiếu ở trạng thái Chờ duyệt mới có thể được thực hiện.");
        }

        Status = ApprovalStatus.Approved;

        var lineEtos = new List<Events.InventoryTicketLineEto>();
        foreach (InventoryTicketLine line in ticketLines)
        {
            lineEtos.Add(new Events.InventoryTicketLineEto
            {
                ProductId = line.ProductId,
                UnitId = line.UnitId,
                Quantity = line.Quantity,
                ReferenceDocumentLineId = line.ReferenceDocumentLineId
            });
        }

        AddLocalEvent(new InventoryTicketExecutedDomainEvent(Id, Type, ReferenceDocumentId, lineEtos));
    }
    public void Reject()
    {
        if (Status != ApprovalStatus.Pending)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicketStatus", "Chỉ các phiếu đang chờ phê duyệt mới có thể bị từ chối.");
        }

        Status = ApprovalStatus.Rejected;
    }
    public void UpdateNote(string? note) => Note = note;
}






