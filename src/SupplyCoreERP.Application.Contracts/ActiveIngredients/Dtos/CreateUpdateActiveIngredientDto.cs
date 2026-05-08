using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.ActiveIngredients.Dtos;

public class CreateUpdateActiveIngredientDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; }
}
