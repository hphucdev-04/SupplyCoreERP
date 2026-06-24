using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Suppliers.Dtos;

public class CreateUpdateSupplierProductConditionDto
{
    public Guid? Id { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Hệ số quy đổi phải lớn hơn 0.")]
    public int ConversionFactor { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "Giá chuẩn không được âm.")]
    public decimal StandardPrice { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Số lượng đặt hàng tối thiểu phải lớn hơn 0.")]
    public decimal MinOrderQuantity { get; set; } = 1;
}

