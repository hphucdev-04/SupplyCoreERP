namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardCategoryDistributionDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal Percentage { get; set; }
}
