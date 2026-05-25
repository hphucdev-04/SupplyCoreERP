using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Tickets.Dtos;

public class AddTicketDetailDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid ProductBatchId { get; set; }

    [Required]
    public Guid BinId { get; set; }

    /// <summary>
    /// ÄÆ¡n vá»‹ ngÆ°á»i dÃ¹ng chá»n (ViÃªn, Vá»‰, Há»™p...).
    /// Pháº£i lÃ  BaseUnitId hoáº·c má»™t ProductUnit há»£p lá»‡ cá»§a sáº£n pháº©m.
    /// </summary>
    [Required]
    public Guid UnitId { get; set; }

    /// <summary>
    /// Tá»‰ lá»‡ quy Ä‘á»•i vá» BaseUnit, snapshot táº¡i thá»i Ä‘iá»ƒm táº¡o.
    /// Truyá»n 1 náº¿u UnitId lÃ  BaseUnit.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "ConversionFactor pháº£i >= 1")]
    public int ConversionFactor { get; set; } = 1;

    /// <summary>Sá»‘ lÆ°á»£ng theo Ä‘Æ¡n vá»‹ Ä‘Ã£ chá»n.</summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Sá»‘ lÆ°á»£ng pháº£i lá»›n hÆ¡n 0")]
    public decimal Quantity { get; set; }
}

