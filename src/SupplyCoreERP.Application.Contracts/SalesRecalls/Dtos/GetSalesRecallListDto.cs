using System;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.SalesRecalls.Dtos;

public class GetSalesRecallListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public SalesRecallStatus? Status { get; set; }
}
