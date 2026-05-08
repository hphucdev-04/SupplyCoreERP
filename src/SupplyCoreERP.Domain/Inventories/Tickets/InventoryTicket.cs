using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Warehouses;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Tickets;

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
    public virtual ICollection<InventoryTicketDetail> Details { get; protected set; }

    protected InventoryTicket() { Details = new List<InventoryTicketDetail>(); }

    public InventoryTicket(Guid id, string ticketNumber, TicketType type, Guid warehouseId, Guid? refDocId, string? refDocNumber, string? note) : base(id)
    {
        TicketNumber = ticketNumber;
        Type = type;
        WarehouseId = warehouseId;
        ReferenceDocumentId = refDocId;
        ReferenceDocumentNumber = refDocNumber;
        Note = note;
        Status = ApprovalStatus.Draft;
        Details = new List<InventoryTicketDetail>();
    }

    public void RequestApprove() => Status = ApprovalStatus.Pending;
    public void Execute() => Status = ApprovalStatus.Approved;
    public void Reject() => Status = ApprovalStatus.Rejected;
    public void UpdateNote(string? note) => Note = note;
}
