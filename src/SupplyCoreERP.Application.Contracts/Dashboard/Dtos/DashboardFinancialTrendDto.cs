using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardFinancialTrendDto
{
    public string Date { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
    public decimal ProcurementAmount { get; set; }
}
