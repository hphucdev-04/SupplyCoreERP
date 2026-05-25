using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos;

public class GetSupplierListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }

}

