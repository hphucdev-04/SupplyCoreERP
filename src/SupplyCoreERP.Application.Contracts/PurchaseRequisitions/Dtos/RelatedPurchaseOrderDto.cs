using System;
using SupplyCoreERP.Enums.Orders;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class RelatedPurchaseOrderDto
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string SupplierName { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreationTime { get; set; }
}
