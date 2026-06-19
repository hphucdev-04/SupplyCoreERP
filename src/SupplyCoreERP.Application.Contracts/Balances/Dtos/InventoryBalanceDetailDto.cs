using System;
using System.Collections.Generic;

namespace SupplyCoreERP.Balances.Dtos;

public class InventoryBalanceDetailDto : InventoryBalanceDto
{
    public string? WarehouseAddress { get; set; }
    public string? CityName { get; set; }
    public string? AreaName { get; set; }
    public string? ProductCode { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? SupplierName { get; set; }

    public List<InventoryBinBalanceDto> BinBalances { get; set; } = new();
    public List<InventoryReservationDto> Reservations { get; set; } = new();
}

