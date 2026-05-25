using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Medicines.Dtos;

public class CreateUpdateMedicineUnitDto
{
    [Required]
    public Guid UnitId { get; set; } // Khi Update, trÆ°á»ng nÃ y chá»‰ Ä‘á»ƒ tham chiáº¿u, khÃ´ng cho sá»­a ID

    [Range(2, int.MaxValue, ErrorMessage = "Há»‡ sá»‘ quy Ä‘á»•i pháº£i lá»›n hÆ¡n 1")]
    public int ConversionFactor { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Level pháº£i tá»« 1 trá»Ÿ lÃªn")]
    public int Level { get; set; }
}

