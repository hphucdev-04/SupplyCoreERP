using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardWarehouseCapacityDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public decimal OccupiedVolume { get; set; }
    public decimal ReservedVolume { get; set; }
    public decimal AvailableVolume { get; set; }
    public decimal SafeMaxVolume { get; set; }
    public decimal CapacityPercent { get; set; }
}
