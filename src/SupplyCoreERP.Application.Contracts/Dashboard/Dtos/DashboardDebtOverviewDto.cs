using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardDebtOverviewDto
{
    public decimal TotalReceivableDebt { get; set; }
    public decimal TotalPayableDebt { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalSuppliers { get; set; }
}
