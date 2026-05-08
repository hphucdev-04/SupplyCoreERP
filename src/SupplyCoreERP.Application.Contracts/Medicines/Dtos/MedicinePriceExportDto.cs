using System;
using MiniExcelLibs.Attributes;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicinePriceExportDto
{
    [ExcelColumnName("Mã thuốc")]
    [ExcelColumnWidth(20)]
    public string MedicineCode { get; set; }

    [ExcelColumnName("Tên thuốc")]
    [ExcelColumnWidth(30)]
    public string MedicineName { get; set; }

    [ExcelColumnName("Bảng giá")]
    [ExcelColumnWidth(25)]
    public string PriceListName { get; set; }

    [ExcelColumnName("Đơn vị tính")]
    [ExcelColumnWidth(15)]
    public string UnitName { get; set; }

    [ExcelColumnName("Giá bán")]
    [ExcelColumnWidth(15)]
    [ExcelFormat("#,##0")] // Format số tiền
    public decimal Price { get; set; }

    [ExcelColumnName("SL tối thiểu")]
    [ExcelColumnWidth(15)]
    public int MinQuantity { get; set; }

    [ExcelColumnName("Loại tiền")]
    [ExcelColumnWidth(10)]
    public string Currency { get; set; }
}
