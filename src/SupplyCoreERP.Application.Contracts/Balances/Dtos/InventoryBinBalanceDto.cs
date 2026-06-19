using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos;

public class InventoryBinBalanceDto : EntityDto<Guid>
{
    public Guid BinId { get; set; }
    public string? BinCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal LockedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
}
