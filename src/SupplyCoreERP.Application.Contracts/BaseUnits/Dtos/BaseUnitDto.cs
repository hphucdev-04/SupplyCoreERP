using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.BaseUnits.Dtos;

public class BaseUnitDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; }
    public string Name { get; set; }
}
