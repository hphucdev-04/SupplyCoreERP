using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos;

public class InventoryBalanceDto : FullAuditedEntityDto<Guid>
{
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    public Guid? BinId { get; set; }
    public string? BinCode { get; set; }

    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }

    public Guid ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }

    public string? BaseUnitName { get; set; }

    public decimal Quantity { get; set; }
    public decimal LockedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
}

