using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Locations.Dtos;

public class CityDto : EntityDto<Guid>
{
    public Guid CountryId { get; set; }
    public string Name { get; set; }
}

