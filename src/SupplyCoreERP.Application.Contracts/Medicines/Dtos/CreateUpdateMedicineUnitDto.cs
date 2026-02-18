using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Medicines.Dtos
{
	public class CreateUpdateMedicineUnitDto
	{
		[Required]
		public Guid UnitId { get; set; } // Khi Update, trường này chỉ để tham chiếu, không cho sửa ID

		[Range(2, int.MaxValue, ErrorMessage = "Hệ số quy đổi phải lớn hơn 1")]
		public int ConversionFactor { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Level phải từ 1 trở lên")]
		public int Level { get; set; }
	}
}
