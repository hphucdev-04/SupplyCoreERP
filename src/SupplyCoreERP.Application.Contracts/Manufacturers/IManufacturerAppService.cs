using System;
using System.Collections.Generic;
using System.Text;
using SupplyCoreERP.Manufacturers.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Manufacturers;

public interface IManufacturerAppService :
    ICrudAppService<
        ManufacturerDto,
        Guid,
        GetManufacturerListDto,
        CreateUpdateManufacturerDto>
{

}
