using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseReturns.Dtos;

public class CreatePurchaseReturnDto
{
    [Required]
    public Guid PurchaseOrderId { get; set; }

    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
