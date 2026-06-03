using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseReturns.Dtos;

public class AddPurchaseReturnLineDto
{
    [Required]
    public Guid PurchaseOrderLineId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ConversionFactor { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal OriginalUnitPrice { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal DepreciationRate { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal TaxRate { get; set; }
}
