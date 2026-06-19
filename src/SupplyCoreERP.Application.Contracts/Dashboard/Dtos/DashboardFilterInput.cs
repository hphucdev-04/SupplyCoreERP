using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardFilterInput
{
    public Guid? WarehouseId { get; set; }
    public int? Days { get; set; }
    public Guid? CategoryId { get; set; }
}
