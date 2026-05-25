using System;
using SupplyCoreERP.Enums.Warehouses;

namespace SupplyCoreERP.PurchaseOrders.Dtos;

public class RelatedTicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; }
    public TicketType Type { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}

