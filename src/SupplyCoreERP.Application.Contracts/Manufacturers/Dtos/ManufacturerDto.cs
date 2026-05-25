using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Manufacturers.Dtos;

public class ManufacturerDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }

    public Guid ContinentId { get; set; }
    public string ContinentName { get; set; }

    public Guid CountryId { get; set; }
    public string CountryName { get; set; }
}

