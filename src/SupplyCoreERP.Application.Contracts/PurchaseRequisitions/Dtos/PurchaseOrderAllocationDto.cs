using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class PurchaseOrderAllocationDto
{
    [Required]
    public Guid RequisitionLineId { get; set; }
    [Required]
    public Guid SupplierId { get; set; }
    [Required]
    public Guid WarehouseId { get; set; }
    [Required]
    public decimal Quantity { get; set; }
}
