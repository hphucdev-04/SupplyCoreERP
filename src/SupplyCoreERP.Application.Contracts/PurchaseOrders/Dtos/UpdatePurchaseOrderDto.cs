using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.PurchaseOrders.Dtos;

public class UpdatePurchaseOrderDto
{
    [Required]
    public Guid WarehouseId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DueDate { get; set; }
    [MaxLength(1000)]
    public string? Note { get; set; }
}

