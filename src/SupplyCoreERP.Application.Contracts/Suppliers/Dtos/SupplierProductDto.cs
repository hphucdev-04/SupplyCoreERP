using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos;

public class SupplierProductDto : EntityDto<Guid>
{
    public Guid SupplierId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductCode { get; set; }

    public Guid DefaultUnitId { get; set; }
    public string DefaultUnitName { get; set; }

    public int LeadTimeDays { get; set; }

    public bool IsPreferred { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }

    public List<SupplierProductConditionDto> Conditions { get; set; } = new();
}

