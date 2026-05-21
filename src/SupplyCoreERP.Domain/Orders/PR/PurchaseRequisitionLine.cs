using System;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Orders.PR;

public class PurchaseRequisitionLine : AuditedEntity<Guid>
{
    public Guid PurchaseRequisitionId { get; private set; }
    public virtual PurchaseRequisition PurchaseRequisition { get; protected set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }

    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }

    public decimal Quantity { get; private set; }
    public decimal OrderedQuantity { get; private set; }
    public string? Note { get; private set; }

    protected PurchaseRequisitionLine() { }

    public PurchaseRequisitionLine(
        Guid id,
        Guid purchaseRequisitionId,
        Guid productId,
        Guid unitId,
        decimal quantity,
        string? note) : base(id)
    {
        PurchaseRequisitionId = purchaseRequisitionId;
        ProductId = productId;
        UnitId = unitId;
        Quantity = quantity > 0 ? quantity : throw new UserFriendlyException("Số lượng phải lớn hơn 0.");
        Note = note;
        OrderedQuantity = 0;
    }

    public void UpdateInfo(decimal quantity, string? note)
    {
        Quantity = quantity > 0 ? quantity : throw new UserFriendlyException("Số lượng phải lớn hơn 0.");
        Note = note;
    }

    public void AddOrderedQuantity(decimal qty)
    {
        OrderedQuantity += qty;
        if (OrderedQuantity > Quantity)
        {
            // Có thể cho phép đặt quá số lượng yêu cầu tùy business, 
            // tạm thời chỉ ghi nhận thực tế.
        }
    }
}
