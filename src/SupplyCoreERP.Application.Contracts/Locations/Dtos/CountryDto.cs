using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Locations.Dtos;

public class CountryDto : EntityDto<Guid>
{
    public Guid ContinentId { get; set; }
    public string ISO { get; set; }
    public string Name { get; set; }
}
