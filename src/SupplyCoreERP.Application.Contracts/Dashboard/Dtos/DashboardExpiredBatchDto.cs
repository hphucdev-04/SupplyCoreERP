using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardExpiredBatchDto
{
    public string MedicineName { get; set; } = string.Empty;
    public string BatchNumber { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int DaysRemaining { get; set; }
}
