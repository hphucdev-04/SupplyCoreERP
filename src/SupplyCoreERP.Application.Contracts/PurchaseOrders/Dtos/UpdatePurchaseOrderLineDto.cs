using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.PurchaseOrders.Dtos;

public class UpdatePurchaseOrderLineDto
{
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
}

