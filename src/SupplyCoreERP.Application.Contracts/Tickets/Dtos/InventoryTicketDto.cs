using System;
using System.Collections.Generic;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos;

public class InventoryTicketDto : FullAuditedEntityDto<Guid>
{
    public string TicketNumber { get; set; }
    public TicketType Type { get; set; }
    public ApprovalStatus Status { get; set; }

    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    public Guid? ReferenceDocumentId { get; set; }
    public string? ReferenceDocumentNumber { get; set; }
    public string? Note { get; set; }

    public List<InventoryTicketLineDto> Lines { get; set; } = new List<InventoryTicketLineDto>();
}

