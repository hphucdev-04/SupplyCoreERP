using System;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Partner.Customers;
using SupplyCoreERP.Sales.Orders;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Sales.SalesRecalls;

public class SalesRecallLine : AuditedEntity<Guid>
{
    public Guid SalesRecallId { get; private set; }
    public virtual SalesRecall SalesRecall { get; protected set; }

    public Guid CustomerId { get; private set; }
    public virtual Customer Customer { get; protected set; }

    public Guid SalesOrderId { get; private set; }
    public virtual SalesOrder SalesOrder { get; protected set; }

    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }

    public int ConversionFactor { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal BaseQuantity => Quantity * ConversionFactor;
    public decimal RecalledQuantity { get; private set; }

    public decimal OriginalUnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }

    public decimal TotalPrice => Quantity * OriginalUnitPrice;
    public decimal TaxAmount => TotalPrice * (TaxRate / 100);
    public decimal FinalPrice => TotalPrice + TaxAmount;

    protected SalesRecallLine() { }

    public SalesRecallLine(
        Guid id,
        Guid salesRecallId,
        Guid customerId,
        Guid salesOrderId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal originalUnitPrice,
        decimal taxRate) : base(id)
    {
        SalesRecallId = salesRecallId;
        CustomerId = customerId;
        SalesOrderId = salesOrderId;
        UnitId = unitId;
        ConversionFactor = conversionFactor > 0 ? conversionFactor : throw new BusinessException("SupplyCoreERP:InvalidConversionFactor", "Hệ số quy đổi phải lớn hơn 0!");
        OriginalUnitPrice = originalUnitPrice >= 0 ? originalUnitPrice : throw new BusinessException("SupplyCoreERP:InvalidUnitPrice", "Đơn giá không được âm!");
        TaxRate = taxRate >= 0 ? taxRate : throw new BusinessException("SupplyCoreERP:InvalidTaxRate", "Thuế suất không được âm!");
        RecalledQuantity = 0;

        SetQuantity(quantity);
    }

    public void UpdateQuantity(decimal quantity)
    {
        SetQuantity(quantity);
    }

    public void AddRecalledQuantity(decimal quantity)
    {
        if (quantity < 0)
        {
            throw new BusinessException("SupplyCoreERP:InvalidQuantity", "Số lượng cộng thêm không được âm!");
        }
        if (RecalledQuantity + quantity > Quantity)
        {
            throw new BusinessException(
                "SupplyCoreERP:ExceedsRecallQuantity",
                $"Số lượng thu hồi thực tế tích lũy ({RecalledQuantity + quantity}) không được vượt quá số lượng yêu cầu thu hồi ({Quantity})!"
            );
        }
        RecalledQuantity += quantity;
    }

    private void SetQuantity(decimal quantity)
    {
        Quantity = quantity > 0 ? quantity : throw new BusinessException("SupplyCoreERP:InvalidQuantity", "Số lượng thu hồi phải lớn hơn 0!");
    }
}
