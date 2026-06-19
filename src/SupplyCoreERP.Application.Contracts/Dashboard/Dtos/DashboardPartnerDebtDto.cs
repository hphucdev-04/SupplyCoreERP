using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardPartnerDebtDto
{
    public string PartnerCode { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public decimal CurrentDebt { get; set; }
}
