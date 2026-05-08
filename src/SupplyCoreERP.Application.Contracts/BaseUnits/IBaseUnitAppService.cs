using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.BaseUnits.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.BaseUnits;

public interface IBaseUnitAppService : ICrudAppService<
    BaseUnitDto,
    Guid,
    GetBaseUnitListDto,
    CreateUpdateBaseUnitDto>
{

}
