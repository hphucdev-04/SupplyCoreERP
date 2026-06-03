using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.SalesRecalls.Dtos;

public class SalesRecallDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string RecallDecisionNumber { get; set; }

    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }

    public Guid? ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }

    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string? WarehouseCode { get; set; }

    public DateTime RecallDate { get; set; }
    public RecallLevel Level { get; set; }
    public DateTime Deadline { get; set; }
    public SalesRecallStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }

    public bool IsOverdue { get; set; }

    public List<SalesRecallLineDto> Lines { get; set; } = new List<SalesRecallLineDto>();
    public List<SalesRecallRelatedTicketDto> RelatedTickets { get; set; } = new List<SalesRecallRelatedTicketDto>();
}

public class SalesRecallRelatedTicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; }
    public TicketType Type { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime CreationTime { get; set; }
}
