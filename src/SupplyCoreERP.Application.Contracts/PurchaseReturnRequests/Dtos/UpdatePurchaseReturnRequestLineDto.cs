using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseReturnRequests.Dtos;

public class UpdatePurchaseReturnRequestLineDto
{
    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal DepreciationRate { get; set; }
}
