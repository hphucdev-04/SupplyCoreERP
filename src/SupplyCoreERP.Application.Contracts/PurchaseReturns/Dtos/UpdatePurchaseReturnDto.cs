using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseReturns.Dtos;

public class UpdatePurchaseReturnDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
