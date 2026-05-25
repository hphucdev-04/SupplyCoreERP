using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Suppliers.Dtos;

public class CreateUpdateSupplierProductConditionDto
{
    public Guid? Id { get; set; } // Null khi thÃªm má»›i, cÃ³ giÃ¡ trá»‹ khi cáº­p nháº­t dÃ²ng cÅ©

    [Required]
    public Guid UnitId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Há»‡ sá»‘ quy Ä‘á»•i pháº£i lá»›n hÆ¡n 0.")]
    public int ConversionFactor { get; set; } = 1;

    [Range(0, double.MaxValue, ErrorMessage = "GiÃ¡ chuáº©n khÃ´ng Ä‘Æ°á»£c Ã¢m.")]
    public decimal StandardPrice { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "Sá»‘ lÆ°á»£ng Ä‘áº·t hÃ ng tá»‘i thiá»ƒu pháº£i lá»›n hÆ¡n 0.")]
    public decimal MinOrderQuantity { get; set; } = 1;

    [Range(0, 100)]
    public decimal OverDeliveryTolerancePct { get; set; }

    [Range(0, 100)]
    public decimal UnderDeliveryTolerancePct { get; set; }
}

