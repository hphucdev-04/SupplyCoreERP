using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.ActiveIngredients.Dtos;

public class GetActiveIngredientListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}

