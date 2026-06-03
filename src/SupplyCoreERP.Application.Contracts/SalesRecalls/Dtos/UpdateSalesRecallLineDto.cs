using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.SalesRecalls.Dtos;

public class UpdateSalesRecallLineDto
{
    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }
}
