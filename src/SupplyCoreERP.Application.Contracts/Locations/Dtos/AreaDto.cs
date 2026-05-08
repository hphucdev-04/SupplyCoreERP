using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Locations.Dtos;

public class AreaDto : EntityDto<Guid>
{
    public Guid CityId { get; set; }
    public string ZipCode { get; set; }
    public string Name { get; set; }
}
