using System;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Products;
using SupplyCoreERP.Warehouses;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Tickets;

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

    /// <summary>Đơn vị người dùng nhập liệu (Viên, Vỉ, Hộp...).</summary>
    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }

    /// <summary>
    /// Số lượng theo đơn vị người dùng chọn. Ví dụ: 5 (Hộp).
    /// Dùng để hiển thị trên phiếu.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Snapshot tỉ lệ quy đổi tại thời điểm tạo phiếu.
    /// Ví dụ: 1 Hộp = 50 Viên → ConversionFactor = 50.
    /// Luôn = 1 nếu đơn vị nhập là BaseUnit.
    /// </summary>
    public int ConversionFactor { get; private set; }

    /// <summary>
    /// Số lượng quy về BaseUnit. Đây là giá trị dùng để cập nhật InventoryBalance.
    /// Ví dụ: Quantity=5 Hộp, ConversionFactor=50 → BaseQuantity=250 Viên.
    /// </summary>
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
            throw new ArgumentException("ConversionFactor phải lớn hơn 0!");
        }

        TicketLineId = ticketLineId;
        ProductId = productId;
        ProductBatchId = batchId;
        BinId = binId;
        UnitId = unitId;
        ConversionFactor = conversionFactor;
        Quantity = qty;
    }

    /// <summary>
    /// Cập nhật số lượng thực xuất/nhập (theo đơn vị đã chọn).
    /// BaseQuantity tự động được tính lại.
    /// </summary>
    public void UpdateActualQuantity(decimal qty) =>
        Quantity = qty >= 0 ? qty : throw new ArgumentException("Số lượng không hợp lệ!");
}
