using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.DosageForms.Dtos;

public class GetDosageFormListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}

