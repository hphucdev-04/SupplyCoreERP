using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseReturns.Dtos;

public class UpdatePurchaseReturnLineDto
{
    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal DepreciationRate { get; set; }
}
