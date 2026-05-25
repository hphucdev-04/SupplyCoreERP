using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Categories.Dtos;

public class CategoryDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; }
    public int ProductCount { get; set; }
}

