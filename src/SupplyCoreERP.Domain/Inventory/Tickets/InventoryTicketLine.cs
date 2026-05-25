using System;
using System.Collections.Generic;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Sales.Orders;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Tickets;

public class InventoryTicketLine : AuditedEntity<Guid>
{
    public Guid TicketId { get; private set; }
    public virtual InventoryTicket Ticket { get; protected set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }

    public Guid? PurchaseOrderLineId { get; private set; }
    public virtual PurchaseOrderLine? PurchaseOrderLine { get; protected set; }

    public Guid? SalesOrderLineId { get; private set; }
    public virtual SalesOrderLine? SalesOrderLine { get; protected set; }

    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }
    public int ConversionFactor { get; private set; }
    public decimal Quantity { get; private set; }

    public virtual ICollection<InventoryTicketDetail> Details { get; protected set; }

    protected InventoryTicketLine() { Details = new List<InventoryTicketDetail>(); }

    public InventoryTicketLine(
        Guid id,
        Guid ticketId,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        Guid? purchaseOrderLineId,
        decimal quantity,
        Guid? salesOrderLineId = null) : base(id)
    {
        TicketId = ticketId;
        ProductId = productId;
        UnitId = unitId;
        ConversionFactor = conversionFactor;
        PurchaseOrderLineId = purchaseOrderLineId;
        SalesOrderLineId = salesOrderLineId;
        Quantity = quantity;
        Details = new List<InventoryTicketDetail>();
    }

    public void UpdateQuantity(decimal quantity)
    {
        Quantity = quantity;
    }
}






