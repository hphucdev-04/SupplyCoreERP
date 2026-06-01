using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Warehouses;

namespace SupplyCoreERP.Inventory.Tickets.Events;

public class InventoryTicketExecutedDomainEvent
{
    public Guid TicketId { get; }
    public TicketType TicketType { get; }
    public Guid? ReferenceDocumentId { get; }
    public List<InventoryTicketLineEto> Lines { get; }

    public InventoryTicketExecutedDomainEvent(Guid ticketId, TicketType ticketType, Guid? referenceDocumentId, List<InventoryTicketLineEto> lines)
    {
        TicketId = ticketId;
        TicketType = ticketType;
        ReferenceDocumentId = referenceDocumentId;
        Lines = lines;
    }
}

public class InventoryTicketLineEto
{
    public Guid ProductId { get; set; }
    public Guid UnitId { get; set; }
    public decimal Quantity { get; set; }
    public Guid? ReferenceDocumentLineId { get; set; }
}
