using System;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Procurement.PurchaseOrders;

public class PurchaseOrderLine : AuditedEntity<Guid>
{
    public Guid PurchaseOrderId { get; private set; }
    public virtual PurchaseOrder PurchaseOrder { get; protected set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }

    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }

    public int ConversionFactor { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal BaseQuantity => Quantity * ConversionFactor;

    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }

    public decimal TotalPrice => Quantity * UnitPrice;
    public decimal TaxAmount => TotalPrice * (TaxRate / 100);
    public decimal FinalPrice => TotalPrice + TaxAmount;
    public decimal ReceivedQuantity { get; private set; }

    protected PurchaseOrderLine() { }

    public PurchaseOrderLine(
        Guid id, Guid purchaseOrderId, Guid productId, Guid unitId,
        int conversionFactor, decimal quantity, decimal unitPrice, decimal taxRate) : base(id)
    {
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        UnitId = unitId;
        ConversionFactor = conversionFactor > 0 ? conversionFactor : throw new BusinessException("SupplyCoreERP:InvalidConversionFactor", "Hệ số quy đổi không hợp lệ.");
        Quantity = quantity > 0 ? quantity : throw new BusinessException("SupplyCoreERP:InvalidQuantity", "Số lượng phải lớn hơn 0.");
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new BusinessException("SupplyCoreERP:InvalidUnitPrice", "Đơn giá không được âm.");
        TaxRate = taxRate >= 0 ? taxRate : throw new BusinessException("SupplyCoreERP:InvalidTaxRate", "Thuế suất không được âm.");
        ReceivedQuantity = 0;
    }

    public void UpdateInfo(decimal quantity, decimal unitPrice, decimal taxRate)
    {
        Quantity = quantity > 0 ? quantity : throw new BusinessException("SupplyCoreERP:InvalidQuantity", "Số lượng phải lớn hơn 0.");
        UnitPrice = unitPrice >= 0 ? unitPrice : throw new BusinessException("SupplyCoreERP:InvalidUnitPrice", "Đơn giá không được âm.");
        TaxRate = taxRate >= 0 ? taxRate : throw new BusinessException("SupplyCoreERP:InvalidTaxRate", "Thuế suất không được âm.");
    }

    public void AddReceivedQuantity(decimal qty)
    {
        ReceivedQuantity += qty;
    }
}






