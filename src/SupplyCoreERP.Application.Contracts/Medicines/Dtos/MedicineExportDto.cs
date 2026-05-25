using System;
using MiniExcelLibs.Attributes; // <--- ÄÃ£ cÃ i á»Ÿ BÆ°á»›c 1 nÃªn sáº½ háº¿t lá»—i

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineExportDto
{
    [ExcelColumnName("MÃ£ thuá»‘c")]
    [ExcelColumnWidth(20)]
    public string Code { get; set; }

    [ExcelColumnName("TÃªn thuá»‘c")]
    [ExcelColumnWidth(30)]
    public string Name { get; set; }

    [ExcelColumnName("NhÃ³m hÃ ng")]
    [ExcelColumnWidth(20)]
    public string Category { get; set; }

    [ExcelColumnName("NhÃ  sáº£n xuáº¥t")]
    [ExcelColumnWidth(25)]
    public string Manufacturer { get; set; }

    [ExcelColumnName("Xuáº¥t xá»©")]
    [ExcelColumnWidth(15)]
    public string OriginCountry { get; set; }

    [ExcelColumnName("ÄÆ¡n vá»‹ cÆ¡ báº£n")]
    [ExcelColumnWidth(15)]
    public string BaseUnit { get; set; }

    [ExcelColumnName("Dáº¡ng bÃ o cháº¿")]
    [ExcelColumnWidth(20)]
    public string DosageForm { get; set; }

    [ExcelColumnName("Sá»‘ Ä‘Äƒng kÃ½")]
    [ExcelColumnWidth(20)]
    public string RegistrationNumber { get; set; }

    [ExcelColumnName("ÄÆ°á»ng dÃ¹ng")]
    [ExcelColumnWidth(20)]
    public string UsageRoute { get; set; }

    [ExcelColumnName("Äiá»u kiá»‡n báº£o quáº£n")]
    [ExcelColumnWidth(25)]
    public string StorageCondition { get; set; }

    [ExcelColumnName("Thuá»‘c kÃª Ä‘Æ¡n")]
    [ExcelColumnWidth(15)]
    public string IsPrescriptionDrug { get; set; }

    // --- DANH SÃCH CON 
    [ExcelColumnName("Hoáº¡t cháº¥t")]
    [ExcelColumnWidth(40)]
    public string Ingredients { get; set; }

    [ExcelColumnName("ÄÆ¡n vá»‹ quy Ä‘á»•i")]
    [ExcelColumnWidth(30)]
    public string Units { get; set; }

    // --- TRáº NG THÃI ---
    [ExcelColumnName("Tráº¡ng thÃ¡i")]
    [ExcelColumnWidth(15)]
    public string Status { get; set; }

    [ExcelColumnName("Hoáº¡t Ä‘á»™ng")]
    [ExcelColumnWidth(15)]
    public string IsActive { get; set; }

    [ExcelColumnName("NgÃ y táº¡o")]
    [ExcelColumnWidth(20)]
    [ExcelFormat("dd/MM/yyyy HH:mm")]
    public DateTime CreationTime { get; set; }
}

