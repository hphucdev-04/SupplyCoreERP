using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Manufacturers.Dtos;

public class GetManufacturerListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
