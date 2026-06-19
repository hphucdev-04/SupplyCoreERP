namespace SupplyCoreERP.Dashboard.Dtos;

public class DashboardPhysicalMovementTrendDto
{
    public string Date { get; set; } = string.Empty;
    public decimal InboundVolume { get; set; }
    public decimal OutboundVolume { get; set; }
}
