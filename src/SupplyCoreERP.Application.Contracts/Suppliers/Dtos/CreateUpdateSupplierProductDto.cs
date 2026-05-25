using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Suppliers.Dtos;

public class CreateUpdateSupplierProductDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid DefaultUnitId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Thá»i gian giao hÃ ng khÃ´ng Ä‘Æ°á»£c Ã¢m.")]
    public int LeadTimeDays { get; set; }

    public bool IsPreferred { get; set; }

    public string? Note { get; set; }

    public List<CreateUpdateSupplierProductConditionDto> Conditions { get; set; } = new();
}

public class SourcingSuggestionDto
{
    public Guid ProductId { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; }
    public double Score { get; set; }
}

