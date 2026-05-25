using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos;

public class SupplierProductDto : EntityDto<Guid>
{
    public Guid SupplierId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } // Tá»± Ä‘á»™ng map tá»« Product.Name
    public string ProductCode { get; set; } // Tá»± Ä‘á»™ng map tá»« Product.Code (náº¿u cÃ³)

    public Guid DefaultUnitId { get; set; }
    public string DefaultUnitName { get; set; } // Tá»± Ä‘á»™ng map tá»« DefaultUnit.Name

    public int LeadTimeDays { get; set; }

    public bool IsPreferred { get; set; }
    public bool IsActive { get; set; }
    public string? Note { get; set; }

    public List<SupplierProductConditionDto> Conditions { get; set; } = new();
}

