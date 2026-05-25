using MiniExcelLibs.Attributes;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicinePriceImportDto
{
    [ExcelColumnName("MÃ£ thuá»‘c")]
    public string MedicineCode { get; set; }

    [ExcelColumnName("Báº£ng giÃ¡")]
    public string PriceListName { get; set; }

    [ExcelColumnName("ÄÆ¡n vá»‹ tÃ­nh")]
    public string UnitName { get; set; }

    [ExcelColumnName("GiÃ¡ bÃ¡n")]
    public decimal Price { get; set; }

    [ExcelColumnName("SL tá»‘i thiá»ƒu")]
    public int MinQuantity { get; set; }
}

