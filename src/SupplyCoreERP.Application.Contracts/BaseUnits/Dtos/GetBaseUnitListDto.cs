using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.BaseUnits.Dtos;

public class GetBaseUnitListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
