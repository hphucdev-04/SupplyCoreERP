using System;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseReturnRequests.Dtos;

public class PurchaseReturnRequestLineDto : AuditedEntityDto<Guid>
{
    public Guid PurchaseReturnRequestId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }
    public int ConversionFactor { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string? PurchaseOrderCode { get; set; }
    public Guid PurchaseOrderLineId { get; set; }
    public PurchaseReturnType ReturnType { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }

    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public decimal OriginalUnitPrice { get; set; }
    public decimal DepreciationRate { get; set; }
    public decimal ReturnUnitPrice { get; set; }
    public decimal TaxRate { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal FinalPrice { get; set; }
}
