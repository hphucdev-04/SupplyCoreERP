using System;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineIngredientDto
{
    public Guid ActiveIngredientId { get; set; }
    public string ActiveIngredientName { get; set; }
    public string ActiveIngredientCode { get; set; }

    /// <summary>
    /// Hàm lượng hoạt chất, dạng chuỗi tự do. Ví dụ: "500mg", "250mg/5ml", "10%".
    /// </summary>
    public string? Strength { get; set; }
}

