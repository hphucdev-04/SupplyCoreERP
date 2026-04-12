using SupplyCoreERP.Enums.Balances;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos
{
	public class GetInventoryReservationListDto : PagedAndSortedResultRequestDto
	{
		// Tìm theo Đơn hàng / Phiếu kho
		public Guid? ReferenceDocumentId { get; set; }
		public string? ReferenceDocumentNumber { get; set; }

		// Tìm theo Tồn kho
		public Guid? WarehouseId { get; set; }
		public Guid? BinId { get; set; }
		public Guid? ProductId { get; set; }
		public Guid? ProductBatchId { get; set; }

		// Trạng thái giữ chỗ
		public ReservationStatus? Status { get; set; }
	}
}
