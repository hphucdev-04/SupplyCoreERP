using System;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos;

public class GetInventoryTicketListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public TicketType? Type { get; set; }
    public ApprovalStatus? Status { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? ReferenceDocumentId { get; set; }
}
