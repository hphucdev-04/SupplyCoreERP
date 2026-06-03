using System;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Procurement.PurchaseReturns;

public class PurchaseReturnLine : AuditedEntity<Guid>
{
    public Guid PurchaseReturnId { get; private set; }
    public virtual PurchaseReturn PurchaseReturn { get; protected set; }

    public Guid PurchaseOrderLineId { get; private set; }

    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }

    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }

    public int ConversionFactor { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal BaseQuantity => Quantity * ConversionFactor;

    public decimal OriginalUnitPrice { get; private set; }
    public decimal DepreciationRate { get; private set; }
    public decimal ReturnUnitPrice => OriginalUnitPrice * (1 - DepreciationRate / 100);
    public decimal TaxRate { get; private set; }

    public decimal TotalPrice => Quantity * ReturnUnitPrice;
    public decimal TaxAmount => TotalPrice * (TaxRate / 100);
    public decimal FinalPrice => TotalPrice + TaxAmount;

    protected PurchaseReturnLine() { }

    public PurchaseReturnLine(
        Guid id,
        Guid purchaseReturnId,
        Guid purchaseOrderLineId,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal originalUnitPrice,
        decimal depreciationRate,
        decimal taxRate) : base(id)
    {
        PurchaseReturnId = purchaseReturnId;
        PurchaseOrderLineId = purchaseOrderLineId;
        ProductId = productId;
        UnitId = unitId;
        ConversionFactor = conversionFactor > 0 ? conversionFactor : throw new BusinessException("SupplyCoreERP:InvalidConversionFactor", "Hệ số quy đổi phải lớn hơn 0!");
        OriginalUnitPrice = originalUnitPrice >= 0 ? originalUnitPrice : throw new BusinessException("SupplyCoreERP:InvalidUnitPrice", "Đơn giá không được âm!");
        TaxRate = taxRate >= 0 ? taxRate : throw new BusinessException("SupplyCoreERP:InvalidTaxRate", "Thuế suất không được âm!");

        SetQuantity(quantity);
        SetDepreciationRate(depreciationRate);
    }

    public void UpdateInfo(decimal quantity, decimal depreciationRate)
    {
        SetQuantity(quantity);
        SetDepreciationRate(depreciationRate);
    }

    private void SetQuantity(decimal quantity)
    {
        Quantity = quantity > 0 ? quantity : throw new BusinessException("SupplyCoreERP:InvalidQuantity", "Số lượng xuất trả phải lớn hơn 0!");
    }

    private void SetDepreciationRate(decimal rate)
    {
        DepreciationRate = (rate >= 0 && rate <= 100) ? rate : throw new BusinessException("SupplyCoreERP:InvalidDepreciationRate", "Tỷ lệ khấu hao phải từ 0% đến 100%!");
    }
}
