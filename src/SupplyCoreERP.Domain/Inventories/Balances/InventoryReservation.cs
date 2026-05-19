using System;
using SupplyCoreERP.Enums.Balances;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Products;
using SupplyCoreERP.Warehouses;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Balances;

public class InventoryReservation : CreationAuditedEntity<Guid>
{
    // Ai là người giữ chỗ?
    public Guid ReferenceDocumentId { get; private set; } // Có thể là TicketId hoặc SalesOrderId
    public string ReferenceDocumentNumber { get; private set; }

    // Giữ chỗ món hàng nào, ở đâu?
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse Warehouse { get; private set; }
    public Guid BinId { get; private set; }
    public virtual Bin Bin { get; private set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; private set; }
    public Guid ProductBatchId { get; private set; }
    public virtual ProductBatch ProductBatch { get; private set; }

    // Số lượng giữ là bao nhiêu?
    public decimal ReservedQuantity { get; private set; }

    // Trạng thái giữ chỗ
    public ReservationStatus Status { get; private set; } // Enum: Active (Đang giữ), Completed (Đã xuất hàng), Cancelled (Đã nhả ra)
    protected InventoryReservation() { }

    public InventoryReservation(
        Guid id, Guid refDocId, string refDocNumber,
        Guid warehouseId, Guid binId, Guid productId, Guid batchId,
        decimal reservedQty) : base(id)
    {
        ReferenceDocumentId = refDocId;
        ReferenceDocumentNumber = refDocNumber;
        WarehouseId = warehouseId;
        BinId = binId;
        ProductId = productId;
        ProductBatchId = batchId;
        ReservedQuantity = reservedQty;
        Status = ReservationStatus.Active;
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
