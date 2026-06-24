using System;
using SupplyCoreERP.Catalog.ActiveIngredients;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SupplyCoreERP.Catalog.Medicines;

public class MedicineIngredient : Entity<Guid>
{
    public Guid MedicineId { get; private set; }
    public Guid ActiveIngredientId { get; private set; }
    public virtual ActiveIngredient ActiveIngredient { get; private set; }

    /// <summary>
    /// Hàm lượng hoạt chất, dạng chuỗi tự do. Ví dụ: "500mg", "250mg/5ml", "10%".
    /// </summary>
    public string? Strength { get; private set; }

    private MedicineIngredient() { }

    public MedicineIngredient(Guid medicineId, Guid ingredientId, string? strength = null)
    {
        MedicineId = medicineId;
        ActiveIngredientId = ingredientId;
        SetStrength(strength);
    }

    public void UpdateStrength(string? strength)
    {
        SetStrength(strength);
    }

    private void SetStrength(string? strength)
    {
        if (strength != null)
        {
            Check.Length(strength, nameof(Strength), maxLength: 50);
            Strength = strength.Trim();
        }
        else
        {
            Strength = null;
        }
    }
}







