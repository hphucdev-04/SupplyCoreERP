using System;
using System.ComponentModel.DataAnnotations;
using SupplyCoreERP.Enums.Orders;

namespace SupplyCoreERP.PurchaseReturnRequests.Dtos;

public class CreatePurchaseReturnRequestDto
{
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public PurchaseReturnType ReturnType { get; set; }

    [Required]
    public DateTime RequestDate { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
