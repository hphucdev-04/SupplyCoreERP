using SupplyCoreERP.Enums.Warehouses;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Warehouses.Dtos
{
	public class GetWarehouseListDto : PagedAndSortedResultRequestDto
	{
		public string? Filter { get; set; }
		public ApprovalStatus? Status { get; set; }
		public bool? IsActive { get; set; }
	}
}