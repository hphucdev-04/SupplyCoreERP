using System;
using System.Collections.Generic;
using System.Linq;
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
    
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }
    
    public Guid ProductBatchId { get; private set; }
    public virtual ProductBatch ProductBatch { get; protected set; }

    public decimal Quantity { get; private set; }
    public decimal LockedQuantity { get; private set; }
    public decimal AvailableQuantity => Quantity - LockedQuantity;

    public virtual ICollection<InventoryBinBalance> BinBalances { get; protected set; }

    protected InventoryBalance()
    {
        BinBalances = new List<InventoryBinBalance>();
    }

    public InventoryBalance(Guid id, Guid warehouseId, Guid productId, Guid batchId) : base(id)
    {
        WarehouseId = warehouseId;
        ProductId = productId;
        ProductBatchId = batchId;
        Quantity = 0;
        LockedQuantity = 0;
        BinBalances = new List<InventoryBinBalance>();
    }

    public void AddStock(Guid binId, decimal amount, Guid newBinBalanceId)
    {
        var binBalance = BinBalances.FirstOrDefault(x => x.BinId == binId);
        if (binBalance == null)
        {
            binBalance = new InventoryBinBalance(newBinBalanceId, Id, binId, amount);
            BinBalances.Add(binBalance);
        }
        else
        {
            binBalance.AddStock(amount);
        }
        Quantity += amount;
    }

    public void RemoveStock(Guid binId, decimal amount)
    {
        var binBalance = BinBalances.FirstOrDefault(x => x.BinId == binId)
            ?? throw new BusinessException("SupplyCoreERP:BinBalanceNotFound", "Không tìm thấy tồn kho tại vị trí kệ này!");

        binBalance.RemoveStock(amount);
        Quantity -= amount;

        if (binBalance.Quantity == 0 && binBalance.LockedQuantity == 0)
        {
            BinBalances.Remove(binBalance);
        }
    }

    public void LockStock(Guid binId, decimal amount)
    {
        var binBalance = BinBalances.FirstOrDefault(x => x.BinId == binId)
            ?? throw new BusinessException("SupplyCoreERP:StockNotAvailable", "Không có tồn kho khả dụng tại vị trí kệ này để giữ hàng!");

        binBalance.LockStock(amount);
        LockedQuantity += amount;
    }

    public void UnlockStock(Guid binId, decimal amount)
    {
        var binBalance = BinBalances.FirstOrDefault(x => x.BinId == binId);
        if (binBalance != null)
        {
            decimal oldLocked = binBalance.LockedQuantity;
            binBalance.UnlockStock(amount);
            decimal actualUnlocked = oldLocked - binBalance.LockedQuantity;
            
            LockedQuantity -= actualUnlocked;
            if (LockedQuantity < 0)
            {
                LockedQuantity = 0;
            }

            if (binBalance.Quantity == 0 && binBalance.LockedQuantity == 0)
            {
                BinBalances.Remove(binBalance);
            }
        }
    }
}






