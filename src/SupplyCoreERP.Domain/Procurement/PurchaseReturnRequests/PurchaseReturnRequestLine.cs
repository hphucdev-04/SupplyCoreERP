

using System;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Procurement.PurchaseReturnRequests;

public class PurchaseReturnRequestLine : AuditedEntity<Guid>
{
    public Guid PurchaseReturnRequestId { get; private set; }
    public virtual PurchaseReturnRequest PurchaseReturnRequest { get; protected set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }
    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }
    public int ConversionFactor { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid PurchaseOrderLineId { get; private set; }
    public PurchaseReturnType ReturnType { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal BaseQuantity { get; private set; }
    public decimal OriginalUnitPrice { get; private set; }
    public decimal DepreciationRate { get; private set; }
    public decimal ReturnUnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }

    public decimal TotalPrice { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal FinalPrice { get; private set; }

    protected PurchaseReturnRequestLine()
    {
    }

    public PurchaseReturnRequestLine(
        Guid id,
        Guid purchaseReturnRequestId,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        Guid purchaseOrderId,
        Guid purchaseOrderLineId,
        decimal quantity,
        decimal originalUnitPrice,
        decimal depreciationRate,
        decimal taxRate,
        PurchaseReturnType returnType) : base(id)
    {
        PurchaseReturnRequestId = purchaseReturnRequestId;
        ProductId = productId;
        UnitId = unitId;
        ConversionFactor = conversionFactor;
        PurchaseOrderId = purchaseOrderId;
        PurchaseOrderLineId = purchaseOrderLineId;
        OriginalUnitPrice = originalUnitPrice;
        TaxRate = taxRate;
        ReturnType = returnType;

        UpdateInfo(quantity, depreciationRate, returnType);
    }

    public void UpdateInfo(decimal quantity, decimal depreciationRate, PurchaseReturnType returnType)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Số lượng xuất trả phải lớn hơn 0!", nameof(quantity));
        }

        if (depreciationRate < 0 || depreciationRate > 100)
        {
            throw new ArgumentException("Tỷ lệ khấu hao phải nằm trong khoảng từ 0% đến 100%!", nameof(depreciationRate));
        }

        ReturnType = returnType;
        Quantity = quantity;
        DepreciationRate = ReturnType == PurchaseReturnType.Defective ? 0 : depreciationRate;
        BaseQuantity = Quantity * ConversionFactor;

        // Tính toán các thông số tài chính
        ReturnUnitPrice = OriginalUnitPrice * (1 - DepreciationRate / 100);
        TotalPrice = ReturnUnitPrice * Quantity;
        TaxAmount = TotalPrice * TaxRate / 100;
        FinalPrice = TotalPrice + TaxAmount;
    }
}
