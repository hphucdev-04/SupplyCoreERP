using System;
using System.Collections.Generic;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Tickets;

public class InventoryTicketLine : AuditedEntity<Guid>
{
    public Guid TicketId { get; private set; }
    public virtual InventoryTicket Ticket { get; protected set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }

    public Guid? ReferenceDocumentLineId { get; private set; }

    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }
    public int ConversionFactor { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal BaseQuantity => Quantity * ConversionFactor;

    public virtual ICollection<InventoryTicketDetail> Details { get; protected set; }

    protected InventoryTicketLine() { Details = new List<InventoryTicketDetail>(); }

    public InventoryTicketLine(
        Guid id,
        Guid ticketId,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        Guid? referenceDocumentLineId,
        decimal quantity) : base(id)
    {
        TicketId = ticketId;
        ProductId = productId;
        UnitId = unitId;
        ConversionFactor = conversionFactor;
        ReferenceDocumentLineId = referenceDocumentLineId;
        Quantity = quantity;
        Details = new List<InventoryTicketDetail>();
    }

    public void UpdateQuantity(decimal quantity)
    {
        Quantity = quantity;
    }
}







