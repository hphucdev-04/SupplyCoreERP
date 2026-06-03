using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.SalesRecalls.Dtos;

public class AddSalesRecallLineDto
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public Guid SalesOrderId { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ConversionFactor { get; set; }

    [Required]
    [Range(0.0001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal OriginalUnitPrice { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal TaxRate { get; set; }
}
