using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseReturnRequests.Dtos;

public class PurchaseReturnRequestDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseCode { get; set; }
    public DateTime RequestDate { get; set; }
    public PurchaseReturnRequestStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public List<PurchaseReturnRequestLineDto> Lines { get; set; } = new List<PurchaseReturnRequestLineDto>();
    public List<PurchaseReturnRequestRelatedTicketDto> RelatedTickets { get; set; } = new List<PurchaseReturnRequestRelatedTicketDto>();
}

public class PurchaseReturnRequestRelatedTicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; }
    public int Type { get; set; } // Liên kết đến Ticket con hoặc phiếu PurchaseReturn
    public int Status { get; set; }
    public DateTime CreationTime { get; set; }
}
