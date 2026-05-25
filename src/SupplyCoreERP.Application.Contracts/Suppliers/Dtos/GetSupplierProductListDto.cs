using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos;

public class GetSupplierProductListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsPreferred { get; set; }
    public bool? IsActive { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}

