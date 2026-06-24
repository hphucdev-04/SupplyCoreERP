using System;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SupplyCoreERP.Inventory.Balances;

public class InventoryBinBalance : Entity<Guid>
{
    public Guid InventoryBalanceId { get; private set; }
    public Guid BinId { get; private set; }
    public virtual Bin Bin { get; protected set; }

    public decimal Quantity { get; private set; }
    public decimal LockedQuantity { get; private set; }
    public decimal AvailableQuantity => Quantity - LockedQuantity;

    protected InventoryBinBalance() { }

    internal InventoryBinBalance(Guid id, Guid inventoryBalanceId, Guid binId, decimal qty = 0) : base(id)
    {
        InventoryBalanceId = inventoryBalanceId;
        BinId = binId;
        Quantity = qty;
        LockedQuantity = 0;
    }

    internal void AddStock(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidAmount", "Số lượng cộng thêm không được âm.");
        }
        Quantity += amount;
    }

    internal void RemoveStock(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidAmount", "Số lượng trừ đi không được âm.");
        }
        if (AvailableQuantity < amount)
        {
            throw new BusinessException("SupplyCoreERP:OutOfStock", "Không đủ tồn kho khả dụng tại vị trí kệ này!");
        }
        Quantity -= amount;
    }

    internal void LockStock(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidAmount", "Số lượng khóa không được âm.");
        }
        if (AvailableQuantity < amount)
        {
            throw new BusinessException("SupplyCoreERP:StockNotAvailable", "Không đủ tồn kho khả dụng tại kệ này để giữ hàng!");
        }
        LockedQuantity += amount;
    }

    internal void UnlockStock(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidAmount", "Số lượng nhả khóa không được âm.");
        }
        LockedQuantity -= amount;
        if (LockedQuantity < 0)
        {
            LockedQuantity = 0;
        }
    }
}
