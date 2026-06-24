using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardOverviewDto
{
    public int TotalWarehouses { get; set; }
    public int TotalMedicines { get; set; }
    public decimal AverageCapacityPercent { get; set; }
    public int ExpiredAlertCount { get; set; }

    // KPI tài chính mới từ SO và PO
    public decimal TotalRevenue { get; set; }
    public decimal TotalProcurement { get; set; }

    // Nghiệp vụ bổ sung
    public decimal TotalSalesRecall { get; set; }
    public decimal TotalPurchaseReturn { get; set; }
    public decimal TotalReservedVolume { get; set; }
    public decimal TotalAvailableVolume { get; set; }
}
