using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class ConvertToPurchaseOrderDto
{
    [Required]
    public List<PurchaseOrderAllocationDto> Allocations { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string? Note { get; set; }
}

