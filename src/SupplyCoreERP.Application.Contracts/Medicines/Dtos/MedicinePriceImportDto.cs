using MiniExcelLibs.Attributes;

namespace SupplyCoreERP.Medicines.Dtos
{
	public class MedicinePriceImportDto
	{
		[ExcelColumnName("Mã thuốc")]
		public string MedicineCode { get; set; }

		[ExcelColumnName("Bảng giá")]
		public string PriceListName { get; set; }

		[ExcelColumnName("Đơn vị tính")]
		public string UnitName { get; set; }

		[ExcelColumnName("Giá bán")]
		public decimal Price { get; set; }

		[ExcelColumnName("SL tối thiểu")]
		public int MinQuantity { get; set; }
	}
}