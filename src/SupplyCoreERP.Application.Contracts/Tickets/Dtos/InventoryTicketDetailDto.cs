using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos;

public class InventoryTicketDetailDto : FullAuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }

    /// <summary>TÃªn BaseUnit cá»§a sáº£n pháº©m (ViÃªn, CÃ¡i...) Ä‘á»ƒ hiá»ƒn thá»‹ bÃªn cáº¡nh BaseQuantity.</summary>
    public string? BaseUnitName { get; set; }

    public Guid ProductBatchId { get; set; }
    public string? BatchNumber { get; set; }
    public string? BatchCode { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? RegistrationNumber { get; set; }
    public Guid BinId { get; set; }
    public string? BinCode { get; set; }

    /// <summary>ÄÆ¡n vá»‹ ngÆ°á»i dÃ¹ng Ä‘Ã£ chá»n khi táº¡o phiáº¿u (Vá»‰, Há»™p...).</summary>
    public Guid UnitId { get; set; }
    public string? UnitName { get; set; }

    /// <summary>Sá»‘ lÆ°á»£ng theo Ä‘Æ¡n vá»‹ Ä‘Ã£ chá»n. VÃ­ dá»¥: 5 (Há»™p).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Tá»‰ lá»‡ quy Ä‘á»•i snapshot. VÃ­ dá»¥: 50 náº¿u 1 Há»™p = 50 ViÃªn.</summary>
    public int ConversionFactor { get; set; }

    /// <summary>
    /// Sá»‘ lÆ°á»£ng Ä‘Ã£ quy vá» BaseUnit = Quantity Ã— ConversionFactor.
    /// ÄÃ¢y lÃ  con sá»‘ thá»±c sá»± tÃ¡c Ä‘á»™ng lÃªn InventoryBalance.
    /// </summary>
    public decimal BaseQuantity => Quantity * ConversionFactor;
}

