using System;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Balances;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Warehouses;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Balances;

public class InventoryReservation : CreationAuditedEntity<Guid>
{
    // Ai là người tạo ra việc giữ chỗ này? 
    public Guid ReferenceDocumentId { get; private set; }
    public string ReferenceDocumentNumber { get; private set; }

    // Giữa chỗ món nào và ở đâu?
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; private set; }
    public Guid BinId { get; private set; }
    public virtual Bin Bin { get; private set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; private set; }
    public Guid ProductBatchId { get; private set; }
    public virtual ProductBatch ProductBatch { get; private set; }

    // Đơn vị giao dịch
    public Guid? UnitId { get; private set; }
    public string? UnitName { get; private set; }

    // Số lượng giữ chỗ?
    public decimal ReservedQuantity { get; private set; }

    // Trạng thái của giữ chỗ?
    public ReservationStatus Status { get; private set; }

    // Thông tin snapshot đối tác & chứng từ gốc
    public Guid? PartnerId { get; private set; }
    public string? PartnerName { get; private set; }
    public Guid? SourceDocumentId { get; private set; }
    public string? SourceDocumentNumber { get; private set; }

    protected InventoryReservation() { }

    public InventoryReservation(
        Guid id, Guid refDocId, string refDocNumber,
        Guid warehouseId, Guid binId, Guid productId, Guid batchId,
        decimal reservedQty, Guid? partnerId = null, string? partnerName = null,
        Guid? sourceDocId = null, string? sourceDocNumber = null,
        Guid? unitId = null, string? unitName = null) : base(id)
    {
        ReferenceDocumentId = refDocId;
        ReferenceDocumentNumber = refDocNumber;
        WarehouseId = warehouseId;
        BinId = binId;
        ProductId = productId;
        ProductBatchId = batchId;
        UnitId = unitId;
        UnitName = unitName;
        ReservedQuantity = reservedQty;
        Status = ReservationStatus.Active;
        PartnerId = partnerId;
        PartnerName = partnerName;
        SourceDocumentId = sourceDocId;
        SourceDocumentNumber = sourceDocNumber;
    }
    public void IncreaseQuantity(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Số lượng tăng phải lớn hơn 0");
        }

        ReservedQuantity += amount;

        // Lỡ trước đó đã bị Cancelled do giảm về 0, giờ tăng lại thì Active lại
        if (Status == ReservationStatus.Cancelled)
        {
            Status = ReservationStatus.Active;
        }
    }

    public void DecreaseQuantity(decimal amount)
    {
        ReservedQuantity -= amount;
        if (ReservedQuantity <= 0)
        {
            ReservedQuantity = 0;
            Cancel(); // Đổi trạng thái thành Cancelled nếu trả hết
        }
    }

    public void Complete() => Status = ReservationStatus.Completed;
    public void Cancel() => Status = ReservationStatus.Cancelled;
}






