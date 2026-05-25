using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Prices.Dtos;

public class CreateUpdateProductPriceDto
{
    [Required]
    public Guid PriceListId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid UnitId { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "GiÃ¡ bÃ¡n pháº£i >= 0")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Sá»‘ lÆ°á»£ng tá»‘i thiá»ƒu pháº£i >= 1")]
    public int MinQuantity { get; set; } = 1;
}

