using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.SalesRecalls.Dtos;

public class SalesRecallLineDto : AuditedEntityDto<Guid>
{
    public Guid CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }

    public Guid SalesOrderId { get; set; }
    public string? SalesOrderCode { get; set; }

    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }

    public int ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }

    public decimal OriginalUnitPrice { get; set; }
    public decimal TaxRate { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalPrice { get; set; }
}
