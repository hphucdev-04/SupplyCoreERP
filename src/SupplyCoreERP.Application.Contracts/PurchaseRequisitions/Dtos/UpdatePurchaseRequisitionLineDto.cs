using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class UpdatePurchaseRequisitionLineDto
{
    [Required]
    [Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }
    public string? Note { get; set; }
}

