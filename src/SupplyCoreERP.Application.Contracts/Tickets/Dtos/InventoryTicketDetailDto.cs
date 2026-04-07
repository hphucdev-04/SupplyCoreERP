using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos
{
	public class InventoryTicketDetailDto : FullAuditedEntityDto<Guid>
	{
		public Guid TicketId { get; set; }

		public Guid ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? ProductCode { get; set; }

		/// <summary>Tên BaseUnit của sản phẩm (Viên, Cái...) để hiển thị bên cạnh BaseQuantity.</summary>
		public string? BaseUnitName { get; set; }

		public Guid ProductBatchId { get; set; }
		public string? BatchNumber { get; set; }
		public DateTime? ExpiryDate { get; set; }

		public Guid BinId { get; set; }
		public string? BinCode { get; set; }

		/// <summary>Đơn vị người dùng đã chọn khi tạo phiếu (Vỉ, Hộp...).</summary>
		public Guid UnitId { get; set; }
		public string? UnitName { get; set; }

		/// <summary>Số lượng theo đơn vị đã chọn. Ví dụ: 5 (Hộp).</summary>
		public decimal Quantity { get; set; }

		/// <summary>Tỉ lệ quy đổi snapshot. Ví dụ: 50 nếu 1 Hộp = 50 Viên.</summary>
		public int ConversionFactor { get; set; }

		/// <summary>
		/// Số lượng đã quy về BaseUnit = Quantity × ConversionFactor.
		/// Đây là con số thực sự tác động lên InventoryBalance.
		/// </summary>
		public decimal BaseQuantity => Quantity * ConversionFactor;
	}
}