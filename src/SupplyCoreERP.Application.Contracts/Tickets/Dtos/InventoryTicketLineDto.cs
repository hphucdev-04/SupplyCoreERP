using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos;

public class InventoryTicketLineDto : FullAuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }

    public Guid? PurchaseOrderLineId { get; set; }
    public Guid? SalesOrderLineId { get; set; }
    public Guid? UnitId { get; set; }
    public string? UnitName { get; set; }
    public int? ConversionFactor { get; set; }

    public decimal Quantity { get; set; }

    public List<InventoryTicketDetailDto> Details { get; set; } = new List<InventoryTicketDetailDto>();
}
