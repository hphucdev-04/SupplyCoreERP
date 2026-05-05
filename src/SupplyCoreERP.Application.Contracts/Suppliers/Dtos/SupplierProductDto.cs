using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos
{
    public class SupplierProductDto : EntityDto<Guid>
    {
        public Guid SupplierId { get; set; }

        public Guid ProductId { get; set; }
        public string ProductName { get; set; } // Tự động map từ Product.Name
        public string ProductCode { get; set; } // Tự động map từ Product.Code (nếu có)

        public Guid DefaultUnitId { get; set; }
        public string DefaultUnitName { get; set; } // Tự động map từ DefaultUnit.Name

        public int DefaultConversionFactor { get; set; }

        public decimal StandardPrice { get; set; }
        public decimal LastPurchasePrice { get; set; }

        public int LeadTimeDays { get; set; }
        public decimal MinOrderQuantity { get; set; }
        public decimal OverDeliveryTolerancePct { get; set; }
        public decimal UnderDeliveryTolerancePct { get; set; }

        public bool IsPreferred { get; set; }
        public bool IsActive { get; set; }
        public string? Note { get; set; }
    }
}
