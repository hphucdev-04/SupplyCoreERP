using System;
using System.ComponentModel.DataAnnotations;
using SupplyCoreERP.Enums.Orders;

namespace SupplyCoreERP.SalesRecalls.Dtos;

public class UpdateSalesRecallDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public DateTime RecallDate { get; set; }

    [Required]
    public RecallLevel Level { get; set; }

    [Required]
    [MaxLength(256)]
    public string RecallDecisionNumber { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
