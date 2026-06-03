using System;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseReturnRequests.Dtos;

public class GetPurchaseReturnRequestListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? WarehouseId { get; set; }
    public PurchaseReturnRequestStatus? Status { get; set; }
}
