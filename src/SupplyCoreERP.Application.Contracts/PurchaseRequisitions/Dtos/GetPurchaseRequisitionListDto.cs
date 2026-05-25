using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseRequisitions.Dtos;

public class GetPurchaseRequisitionListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public PurchaseRequisitionStatus? Status { get; set; }
}

