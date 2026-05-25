using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos;

public class SupplierProductConditionDto : EntityDto<Guid>
{
    public Guid SupplierProductId { get; set; }

    public Guid UnitId { get; set; }
    public string UnitName { get; set; } // Tá»± Ä‘á»™ng map tá»« Unit.Name

    public int ConversionFactor { get; set; }

    public decimal StandardPrice { get; set; }
    public decimal LastPurchasePrice { get; set; }

    public decimal MinOrderQuantity { get; set; }
    public decimal OverDeliveryTolerancePct { get; set; }
    public decimal UnderDeliveryTolerancePct { get; set; }
}

