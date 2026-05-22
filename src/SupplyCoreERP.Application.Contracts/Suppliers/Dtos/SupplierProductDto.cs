using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos;

public class SupplierProductDto : EntityDto<Guid>
{
    public Guid SupplierId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } // Tự động map từ Product.Name
    public string ProductCode { get; set; } // Tự động map từ Product.Code (nếu có)

    public Guid DefaultUnitId { get; set; }
    public string DefaultUnitName { get; set; } // Tự động map từ DefaultUnit.Name

    public int LeadTimeDays { get; set; }

    public bool IsPreferred { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }

    public List<SupplierProductConditionDto> Conditions { get; set; } = new();
}
