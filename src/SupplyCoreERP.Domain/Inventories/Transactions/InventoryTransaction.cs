using System;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Products;
using SupplyCoreERP.Warehouses;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Transactions;

public class InventoryTransaction : CreationAuditedAggregateRoot<Guid>
{
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; protected set; }
    public Guid BinId { get; private set; }
    public virtual Bin Bin { get; protected set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }
    public Guid ProductBatchId { get; private set; }
    public virtual ProductBatch ProductBatch { get; protected set; }

    public InventoryTransactionType TransactionType { get; private set; }
    public decimal QuantityChanged { get; private set; }
    public decimal BalanceAfterTransaction { get; private set; }
    public Guid? ReferenceDocumentId { get; private set; }
    public string? ReferenceDocumentNumber { get; private set; }

    public string? Note { get; private set; }

    protected InventoryTransaction() { }

    public InventoryTransaction(Guid id, Guid warehouseId, Guid binId, Guid prodId, Guid batchId, InventoryTransactionType type, decimal qtyChanged, decimal balanceAfter, Guid? refDocId, string? refDocNumber, string? note) : base(id)
    {
        WarehouseId = warehouseId;
        BinId = binId;
        ProductId = prodId;
        ProductBatchId = batchId;
        TransactionType = type;
        QuantityChanged = qtyChanged;
        BalanceAfterTransaction = balanceAfter;
        ReferenceDocumentId = refDocId;
        ReferenceDocumentNumber = refDocNumber;
        Note = note;
    }
}
