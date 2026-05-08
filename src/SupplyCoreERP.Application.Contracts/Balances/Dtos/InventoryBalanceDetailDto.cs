using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Balances.Dtos;

public class InventoryBalanceDetailDto : InventoryBalanceDto
{
    public string? WarehouseAddress { get; set; }
    public string? CityName { get; set; }
    public string? AreaName { get; set; }

    // Thông tin sâu của Thuốc & Lô
    public string? ProductCode { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? SupplierName { get; set; }
}
