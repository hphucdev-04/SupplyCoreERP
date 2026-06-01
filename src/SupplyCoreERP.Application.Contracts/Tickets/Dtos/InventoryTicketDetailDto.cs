using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos;

public class InventoryTicketDetailDto : FullAuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? BaseUnitName { get; set; }

    public Guid ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public string? BatchCode { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? RegistrationNumber { get; set; }
    public Guid BinId { get; set; }
    public string? BinCode { get; set; }

    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }

    public decimal Quantity { get; set; }
    public int ConversionFactor { get; set; }
    public decimal BaseQuantity => Quantity * ConversionFactor;
}

