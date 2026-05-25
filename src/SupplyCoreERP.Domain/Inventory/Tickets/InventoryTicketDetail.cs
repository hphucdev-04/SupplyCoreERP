using System;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Tickets;

public class InventoryTicketDetail : AuditedEntity<Guid>
{
    public Guid TicketLineId { get; private set; }
    public virtual InventoryTicketLine TicketLine { get; protected set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }
    public Guid ProductBatchId { get; private set; }
    public virtual ProductBatch ProductBatch { get; protected set; }
    public Guid BinId { get; private set; }
    public virtual Bin Bin { get; protected set; }


    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }
    public decimal Quantity { get; private set; }
    public int ConversionFactor { get; private set; }
    public decimal BaseQuantity => Quantity * ConversionFactor;

    protected InventoryTicketDetail() { }

    public InventoryTicketDetail(
        Guid id,
        Guid ticketLineId,
        Guid productId,
        Guid batchId,
        Guid binId,
        Guid unitId,
        int conversionFactor,
        decimal qty) : base(id)
    {
        if (conversionFactor <= 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidConversionFactor", "ConversionFactor phải lớn hơn 0!");
        }

        TicketLineId = ticketLineId;
        ProductId = productId;
        ProductBatchId = batchId;
        BinId = binId;
        UnitId = unitId;
        ConversionFactor = conversionFactor;
        Quantity = qty;
    }

    public void UpdateActualQuantity(decimal qty) =>
        Quantity = qty >= 0 ? qty : throw new BusinessException("SupplyCoreERP:InvalidQuantity", "Số lượng không hợp lệ!");
}






