using System;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Balances;

public class InventoryBalance : FullAuditedAggregateRoot<Guid>
{
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; protected set; }
    public Guid BinId { get; private set; }
    public virtual Bin Bin { get; protected set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }
    public Guid ProductBatchId { get; private set; }
    public virtual ProductBatch ProductBatch { get; protected set; }

    public decimal Quantity { get; private set; }
    public decimal LockedQuantity { get; private set; }
    public decimal AvailableQuantity => Quantity - LockedQuantity;

    protected InventoryBalance() { }

    public InventoryBalance(Guid id, Guid warehouseId, Guid binId, Guid productId, Guid batchId, decimal qty = 0) : base(id)
    {
        WarehouseId = warehouseId;
        BinId = binId;
        ProductId = productId;
        ProductBatchId = batchId;
        Quantity = qty;
        LockedQuantity = 0;
    }

    public void AddStock(decimal amount) => Quantity += amount;

    public void RemoveStock(decimal amount)
    {
        if (AvailableQuantity < amount)
        {
            throw new BusinessException("SupplyCoreERP:OutOfStock", "Không đủ tồn kho để thực hiện thao tác!");
        }

        Quantity -= amount;
    }

    public void LockStock(decimal amount)
    {
        if (AvailableQuantity < amount)
        {
            throw new BusinessException("SupplyCoreERP:StockNotAvailable", "Không đủ tồn kho để giữ hàng!");
        }

        LockedQuantity += amount;
    }

    public void UnlockStock(decimal amount)
    {
        LockedQuantity -= amount;
        if (LockedQuantity < 0)
        {
            LockedQuantity = 0;
        }
    }
}






