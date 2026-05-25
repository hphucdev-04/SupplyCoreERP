using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Categories.Dtos;

public class CreateUpdateCategoryDto
{
    [Required(ErrorMessage = "TÃªn danh má»¥c lÃ  báº¯t buá»™c")]
    [StringLength(100)]
    public string Name { get; set; }
}

