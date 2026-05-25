using System;
using MiniExcelLibs.Attributes;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicinePriceExportDto
{
    [ExcelColumnName("MÃ£ thuá»‘c")]
    [ExcelColumnWidth(20)]
    public string MedicineCode { get; set; }

    [ExcelColumnName("TÃªn thuá»‘c")]
    [ExcelColumnWidth(30)]
    public string MedicineName { get; set; }

    [ExcelColumnName("Báº£ng giÃ¡")]
    [ExcelColumnWidth(25)]
    public string PriceListName { get; set; }

    [ExcelColumnName("ÄÆ¡n vá»‹ tÃ­nh")]
    [ExcelColumnWidth(15)]
    public string UnitName { get; set; }

    [ExcelColumnName("GiÃ¡ bÃ¡n")]
    [ExcelColumnWidth(15)]
    [ExcelFormat("#,##0")] // Format sá»‘ tiá»n
    public decimal Price { get; set; }

    [ExcelColumnName("SL tá»‘i thiá»ƒu")]
    [ExcelColumnWidth(15)]
    public int MinQuantity { get; set; }

    [ExcelColumnName("Loáº¡i tiá»n")]
    [ExcelColumnWidth(10)]
    public string Currency { get; set; }
}

