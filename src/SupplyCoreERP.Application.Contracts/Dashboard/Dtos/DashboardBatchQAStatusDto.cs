namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardBatchQAStatusDto
{
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
