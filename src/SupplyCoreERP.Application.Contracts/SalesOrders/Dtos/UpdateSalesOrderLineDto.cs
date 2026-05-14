using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.SalesOrders.Dtos;

public class UpdateSalesOrderLineDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    [Range(0, 100)]
    public decimal DiscountRate { get; set; }
    [Range(0, 100)]
    public decimal TaxRate { get; set; }
}
