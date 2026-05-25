using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class CreatePurchaseRequisitionDto
{
    [Required]
    public Guid WarehouseId { get; set; }
    [Required]
    public DateTime RequestedDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Note { get; set; }
}

