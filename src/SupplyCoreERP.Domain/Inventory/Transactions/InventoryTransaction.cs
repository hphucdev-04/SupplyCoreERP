using System;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Transactions;

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

    // Đơn vị giao dịch
    public Guid? UnitId { get; private set; }
    public string? UnitName { get; private set; }

    public InventoryTransactionType TransactionType { get; private set; }
    public decimal QuantityChanged { get; private set; }
    public decimal BalanceAfterTransaction { get; private set; }
    public Guid? ReferenceDocumentId { get; private set; }
    public string? ReferenceDocumentNumber { get; private set; }

    public string? Note { get; private set; }

    // Thông tin snapshot đối tác & chứng từ gốc
    public Guid? PartnerId { get; private set; }
    public string? PartnerName { get; private set; }
    public Guid? SourceDocumentId { get; private set; }
    public string? SourceDocumentNumber { get; private set; }
    public Guid? CorrelationId { get; private set; }

    protected InventoryTransaction() { }

    public InventoryTransaction(
        Guid id, Guid warehouseId, Guid binId, Guid prodId, Guid batchId,
        InventoryTransactionType type, decimal qtyChanged, decimal balanceAfter,
        Guid? refDocId, string? refDocNumber, string? note,
        Guid? partnerId = null, string? partnerName = null,
        Guid? sourceDocId = null, string? sourceDocNumber = null,
        Guid? correlationId = null,
        Guid? unitId = null, string? unitName = null) : base(id)
    {
        WarehouseId = warehouseId;
        BinId = binId;
        ProductId = prodId;
        ProductBatchId = batchId;
        UnitId = unitId;
        UnitName = unitName;
        TransactionType = type;
        QuantityChanged = qtyChanged;
        BalanceAfterTransaction = balanceAfter;
        ReferenceDocumentId = refDocId;
        ReferenceDocumentNumber = refDocNumber;
        Note = note;
        PartnerId = partnerId;
        PartnerName = partnerName;
        SourceDocumentId = sourceDocId;
        SourceDocumentNumber = sourceDocNumber;
        CorrelationId = correlationId;
    }
}






