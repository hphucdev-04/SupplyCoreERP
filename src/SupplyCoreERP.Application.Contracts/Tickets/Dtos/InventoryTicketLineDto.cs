using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos;

public class InventoryTicketLineDto : AuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? BaseUnitName { get; set; }

    public Guid? ReferenceDocumentLineId { get; set; }
    public Guid? UnitId { get; set; }
    public string? UnitName { get; set; }
    public int? ConversionFactor { get; set; }

    public decimal Quantity { get; set; }
    public decimal BaseQuantity { get; set; }
    public List<InventoryTicketDetailDto> Details { get; set; } = new List<InventoryTicketDetailDto>();
}
