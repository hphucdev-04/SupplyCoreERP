using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Tickets.Dtos
{
	public class AddTicketDetailDto
	{
		[Required]
		public Guid ProductId { get; set; }

		[Required]
		public Guid ProductBatchId { get; set; }

		[Required]
		public Guid BinId { get; set; }

		/// <summary>
		/// Đơn vị người dùng chọn (Viên, Vỉ, Hộp...).
		/// Phải là BaseUnitId hoặc một ProductUnit hợp lệ của sản phẩm.
		/// </summary>
		[Required]
		public Guid UnitId { get; set; }

		/// <summary>
		/// Tỉ lệ quy đổi về BaseUnit, snapshot tại thời điểm tạo.
		/// Truyền 1 nếu UnitId là BaseUnit.
		/// </summary>
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "ConversionFactor phải >= 1")]
		public int ConversionFactor { get; set; } = 1;

		/// <summary>Số lượng theo đơn vị đã chọn.</summary>
		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
		public decimal Quantity { get; set; }
	}
}