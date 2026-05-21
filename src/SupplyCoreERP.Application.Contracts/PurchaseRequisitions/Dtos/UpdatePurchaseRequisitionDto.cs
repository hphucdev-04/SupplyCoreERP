using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class UpdatePurchaseRequisitionDto
{
    [Required]
    public Guid WarehouseId { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Note { get; set; }
}
