using MiniExcelLibs.Attributes;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineImportDto
{
    [ExcelColumnName("Mã thuốc")]
    public string TempCode { get; set; }

    [ExcelColumnName("Tên thuốc")]
    public string Name { get; set; }

    [ExcelColumnName("Nhóm hàng")]
    public string Category { get; set; }

    [ExcelColumnName("Nhà sản xuất")]
    public string Manufacturer { get; set; }

    [ExcelColumnName("Đơn vị cơ bản")]
    public string BaseUnit { get; set; }

    [ExcelColumnName("Dạng bào chế")]
    public string DosageForm { get; set; }

    [ExcelColumnName("Số đăng ký")]
    public string RegistrationNumber { get; set; }

    [ExcelColumnName("Đường dùng")]
    public string UsageRoute { get; set; }

    [ExcelColumnName("Điều kiện bảo quản")]
    public string StorageCondition { get; set; }

    [ExcelColumnName("Thuốc kê đơn")]
    public string IsPrescriptionDrug { get; set; }

    [ExcelColumnName("Hoạt chất")]
    public string Ingredients { get; set; }

    [ExcelColumnName("Đơn vị quy đổi")]
    public string Units { get; set; }

}
