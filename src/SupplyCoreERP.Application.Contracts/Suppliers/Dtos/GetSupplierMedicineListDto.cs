using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos;

public class GetSupplierMedicineListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}

