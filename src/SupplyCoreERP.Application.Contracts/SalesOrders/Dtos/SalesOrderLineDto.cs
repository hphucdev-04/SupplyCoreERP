using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.SalesOrders.Dtos;

public class SalesOrderLineDto : AuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }

    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }
    public string? BaseUnitName { get; set; }

    public int ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal DeliveredQuantity { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal TaxRate { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PriceAfterDiscount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalPrice { get; set; }
}
