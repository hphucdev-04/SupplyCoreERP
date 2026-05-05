using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Suppliers.Dtos
{
    public class CreateUpdateSupplierProductDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid DefaultUnitId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Hệ số quy đổi phải lớn hơn 0.")]
        public int DefaultConversionFactor { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Giá chuẩn không được âm.")]
        public decimal StandardPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thời gian giao hàng không được âm.")]
        public int LeadTimeDays { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "Số lượng đặt hàng tối thiểu phải lớn hơn 0.")]
        public decimal MinOrderQuantity { get; set; } = 1;

        [Range(0, 100)]
        public decimal OverDeliveryTolerancePct { get; set; }

        [Range(0, 100)]
        public decimal UnderDeliveryTolerancePct { get; set; }

        public bool IsPreferred { get; set; }

        public string? Note { get; set; }
    }
}
