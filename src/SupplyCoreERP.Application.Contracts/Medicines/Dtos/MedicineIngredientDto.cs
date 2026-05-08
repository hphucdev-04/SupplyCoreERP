using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineIngredientDto
{
    public Guid ActiveIngredientId { get; set; }
    public string ActiveIngredientName { get; set; }
    public string ActiveIngredientCode { get; set; }
}
