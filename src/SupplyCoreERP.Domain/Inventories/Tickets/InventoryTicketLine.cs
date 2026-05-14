using System;
using System.Collections.Generic;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.Products;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Tickets;

public class InventoryTicketLine : FullAuditedEntity<Guid>
{
    public Guid TicketId { get; private set; }
    public virtual InventoryTicket Ticket { get; protected set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }

    /// <summary>
    /// Liên kết với dòng đơn mua hàng (nếu có).
    /// Giúp truy vết việc nhập hàng theo PO.
    /// </summary>
    public Guid? PurchaseOrderLineId { get; private set; }
    public virtual PurchaseOrderLine? PurchaseOrderLine { get; protected set; }

    /// <summary>
    /// Đơn vị tính (thường kế thừa từ PO hoặc BaseUnit của Product).
    /// </summary>
    public Guid UnitId { get; private set; }

    /// <summary>
    /// Hệ số quy đổi tại thời điểm tạo phiếu.
    /// </summary>
    public int ConversionFactor { get; private set; }

    /// <summary>
    /// Tổng số lượng của sản phẩm này trong phiếu kho (theo đơn vị cơ bản).
    /// Bằng tổng BaseQuantity của các Details bên dưới.
    /// </summary>
    public decimal Quantity { get; private set; }

    public virtual ICollection<InventoryTicketDetail> Details { get; protected set; }

    protected InventoryTicketLine() { Details = new List<InventoryTicketDetail>(); }

    public InventoryTicketLine(Guid id, Guid ticketId, Guid productId, Guid unitId, int conversionFactor, Guid? purchaseOrderLineId, decimal quantity) : base(id)
    {
        TicketId = ticketId;
        ProductId = productId;
        UnitId = unitId;
        ConversionFactor = conversionFactor;
        PurchaseOrderLineId = purchaseOrderLineId;
        Quantity = quantity;
        Details = new List<InventoryTicketDetail>();
    }

    public void UpdateQuantity(decimal quantity)
    {
        Quantity = quantity;
    }
}
