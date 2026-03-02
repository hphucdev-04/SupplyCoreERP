using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos
{
	public class GetInventoryBalanceListDto : PagedAndSortedResultRequestDto
	{
		public Guid? WarehouseId { get; set; }
		public Guid? BinId { get; set; }
		public Guid? ProductId { get; set; }
		public string? BatchNumber { get; set; }
		public bool? IsNearExpiry { get; set; } // Lọc thuốc sắp hết hạn (VD: Còn dưới 6 tháng)
		public bool? HideZeroQuantity { get; set; } = true; // Mặc định ẩn các kệ đã hết hàng
	}
}