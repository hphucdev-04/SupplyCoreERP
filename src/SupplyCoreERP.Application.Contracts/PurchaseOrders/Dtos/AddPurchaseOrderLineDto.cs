using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.PurchaseOrders.Dtos;

public class AddPurchaseOrderLineDto
{
    public Guid ProductId { get; set; }
    public Guid UnitId { get; set; }
    public int ConversionFactor { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
}

