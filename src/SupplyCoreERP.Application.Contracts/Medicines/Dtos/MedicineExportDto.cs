using System;
using MiniExcelLibs.Attributes; // <--- Đã cài ở Bước 1 nên sẽ hết lỗi

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineExportDto
{
    [ExcelColumnName("Mã thuốc")]
    [ExcelColumnWidth(20)]
    public string Code { get; set; }

    [ExcelColumnName("Tên thuốc")]
    [ExcelColumnWidth(30)]
    public string Name { get; set; }

    [ExcelColumnName("Nhóm hàng")]
    [ExcelColumnWidth(20)]
    public string Category { get; set; }

    [ExcelColumnName("Nhà sản xuất")]
    [ExcelColumnWidth(25)]
    public string Manufacturer { get; set; }

    [ExcelColumnName("Xuất xứ")]
    [ExcelColumnWidth(15)]
    public string OriginCountry { get; set; }

    [ExcelColumnName("Đơn vị cơ bản")]
    [ExcelColumnWidth(15)]
    public string BaseUnit { get; set; }

    [ExcelColumnName("Dạng bào chế")]
    [ExcelColumnWidth(20)]
    public string DosageForm { get; set; }

    [ExcelColumnName("Số đăng ký")]
    [ExcelColumnWidth(20)]
    public string RegistrationNumber { get; set; }

    [ExcelColumnName("Đường dùng")]
    [ExcelColumnWidth(20)]
    public string UsageRoute { get; set; }

    [ExcelColumnName("Điều kiện bảo quản")]
    [ExcelColumnWidth(25)]
    public string StorageCondition { get; set; }

    [ExcelColumnName("Thuốc kê đơn")]
    [ExcelColumnWidth(15)]
    public string IsPrescriptionDrug { get; set; }

    // --- DANH SÁCH CON 
    [ExcelColumnName("Hoạt chất")]
    [ExcelColumnWidth(40)]
    public string Ingredients { get; set; }

    [ExcelColumnName("Đơn vị quy đổi")]
    [ExcelColumnWidth(30)]
    public string Units { get; set; }

    // --- TRẠNG THÁI ---
    [ExcelColumnName("Trạng thái")]
    [ExcelColumnWidth(15)]
    public string Status { get; set; }

    [ExcelColumnName("Hoạt động")]
    [ExcelColumnWidth(15)]
    public string IsActive { get; set; }

    [ExcelColumnName("Ngày tạo")]
    [ExcelColumnWidth(20)]
    [ExcelFormat("dd/MM/yyyy HH:mm")]
    public DateTime CreationTime { get; set; }
}
