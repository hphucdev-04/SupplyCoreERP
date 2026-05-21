using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class AddPurchaseRequisitionLineDto
{
    [Required]
    public Guid ProductId { get; set; }
    [Required]
    public Guid UnitId { get; set; }
    [Required]
    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
    public string? Note { get; set; }
}
