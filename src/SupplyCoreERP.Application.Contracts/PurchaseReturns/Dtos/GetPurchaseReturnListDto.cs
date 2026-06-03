using System;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseReturns.Dtos;

public class GetPurchaseReturnListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? WarehouseId { get; set; }
    public PurchaseReturnStatus? Status { get; set; }
}
