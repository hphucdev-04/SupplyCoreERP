using MiniExcelLibs.Attributes;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineImportDto
{
    [ExcelColumnName("MÃ£ thuá»‘c")]
    public string TempCode { get; set; }

    [ExcelColumnName("TÃªn thuá»‘c")]
    public string Name { get; set; }

    [ExcelColumnName("NhÃ³m hÃ ng")]
    public string Category { get; set; }

    [ExcelColumnName("NhÃ  sáº£n xuáº¥t")]
    public string Manufacturer { get; set; }

    [ExcelColumnName("ÄÆ¡n vá»‹ cÆ¡ báº£n")]
    public string BaseUnit { get; set; }

    [ExcelColumnName("Dáº¡ng bÃ o cháº¿")]
    public string DosageForm { get; set; }

    [ExcelColumnName("Sá»‘ Ä‘Äƒng kÃ½")]
    public string RegistrationNumber { get; set; }

    [ExcelColumnName("ÄÆ°á»ng dÃ¹ng")]
    public string UsageRoute { get; set; }

    [ExcelColumnName("Äiá»u kiá»‡n báº£o quáº£n")]
    public string StorageCondition { get; set; }

    [ExcelColumnName("Thuá»‘c kÃª Ä‘Æ¡n")]
    public string IsPrescriptionDrug { get; set; }

    [ExcelColumnName("Hoáº¡t cháº¥t")]
    public string Ingredients { get; set; }

    [ExcelColumnName("ÄÆ¡n vá»‹ quy Ä‘á»•i")]
    public string Units { get; set; }

}

