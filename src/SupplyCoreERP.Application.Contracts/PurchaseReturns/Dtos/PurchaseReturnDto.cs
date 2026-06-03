using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseReturns.Dtos;

public class PurchaseReturnDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }

    public Guid PurchaseOrderId { get; set; }
    public string? PurchaseOrderCode { get; set; }

    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? SupplierCode { get; set; }

    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseCode { get; set; }

    public DateTime ReturnDate { get; set; }
    public PurchaseReturnStatus Status { get; set; }

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }

    public List<PurchaseReturnLineDto> Lines { get; set; } = new List<PurchaseReturnLineDto>();
    public List<PurchaseReturnRelatedTicketDto> RelatedTickets { get; set; } = new List<PurchaseReturnRelatedTicketDto>();
}

public class PurchaseReturnRelatedTicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; }
    public TicketType Type { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}
