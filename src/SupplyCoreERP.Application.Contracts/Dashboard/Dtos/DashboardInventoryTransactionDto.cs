using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardInventoryTransactionDto
{
    public string TransactionTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
