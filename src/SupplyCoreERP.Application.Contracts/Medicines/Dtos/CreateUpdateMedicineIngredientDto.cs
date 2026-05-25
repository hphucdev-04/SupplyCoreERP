using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Medicines.Dtos;

public class CreateUpdateMedicineIngredientDto
{
    [Required]
    public Guid ActiveIngredientId { get; set; }
}

