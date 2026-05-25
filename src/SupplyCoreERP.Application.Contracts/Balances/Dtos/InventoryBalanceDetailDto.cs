using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Balances.Dtos;

public class InventoryBalanceDetailDto : InventoryBalanceDto
{
    public string? WarehouseAddress { get; set; }
    public string? CityName { get; set; }
    public string? AreaName { get; set; }

    // ThÃ´ng tin sÃ¢u cá»§a Thuá»‘c & LÃ´
    public string? ProductCode { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? SupplierName { get; set; }
}

