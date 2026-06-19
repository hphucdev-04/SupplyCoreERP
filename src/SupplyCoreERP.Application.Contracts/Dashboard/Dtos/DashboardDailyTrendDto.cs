namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardDailyTrendDto
{
    public string Date { get; set; } = string.Empty;
    public decimal ImportQuantity { get; set; }
    public decimal ExportQuantity { get; set; }
}
