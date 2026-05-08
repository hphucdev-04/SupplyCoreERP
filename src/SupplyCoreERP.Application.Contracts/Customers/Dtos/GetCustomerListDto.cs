using Volo.Abp.Application.Dtos;


namespace SupplyCoreERP.Customers.Dtos;

public class GetCustomerListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
