using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.SalesOrders.Dtos;

public class UpdateSalesOrderDto
{
    [Required] public Guid WarehouseId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? DueDate { get; set; }
    [MaxLength(1000)] public string? Note { get; set; }
}
