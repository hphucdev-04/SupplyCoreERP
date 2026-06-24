using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Medicines.Dtos;

public class CreateUpdateMedicineIngredientDto
{
    [Required]
    public Guid ActiveIngredientId { get; set; }

    /// <summary>
    /// Hàm lượng hoạt chất, dạng chuỗi tự do. Ví dụ: "500mg", "250mg/5ml", "10%".
    /// </summary>
    [MaxLength(50)]
    public string? Strength { get; set; }
}

