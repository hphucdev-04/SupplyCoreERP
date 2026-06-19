using System;
using System.Collections.Generic;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardSummaryDto
{
    public int TotalWarehouses { get; set; }
    public int TotalMedicines { get; set; }
    public decimal AverageCapacityPercent { get; set; }
    public int ExpiredAlertCount { get; set; }
    
    public List<DashboardDailyTrendDto> DailyTrends { get; set; } = new();
    public List<DashboardCategoryDistributionDto> CategoryDistribution { get; set; } = new();
    public List<DashboardWarehouseCapacityDto> WarehouseCapacities { get; set; } = new();
    public List<DashboardExpiredBatchDto> ExpiredBatches { get; set; } = new();
}
