using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Warehouses.Dtos;

public class TransferBinDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid SourceBinId { get; set; }

    [Required]
    public Guid TargetBinId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid ProductBatchId { get; set; }

    [Required]
    [Range(0.000001, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public decimal Quantity { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Hệ số quy đổi phải từ 1 trở lên.")]
    public decimal ConversionFactor { get; set; }
}
