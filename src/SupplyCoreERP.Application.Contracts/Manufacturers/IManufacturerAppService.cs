using System;
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

