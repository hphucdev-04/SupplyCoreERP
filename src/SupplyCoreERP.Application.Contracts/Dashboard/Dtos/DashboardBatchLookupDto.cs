using System;

namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardBatchLookupDto
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
}
