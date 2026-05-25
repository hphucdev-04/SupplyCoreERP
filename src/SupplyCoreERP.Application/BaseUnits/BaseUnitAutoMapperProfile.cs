using AutoMapper;
using SupplyCoreERP.BaseUnits.Dtos;
using SupplyCoreERP.Catalog.BaseUnits;

namespace SupplyCoreERP.BaseUnits;

public class BaseUnitAutoMapperProfile : Profile
{
    public BaseUnitAutoMapperProfile()
    {
        CreateMap<BaseUnit, BaseUnitDto>();
        CreateMap<CreateUpdateBaseUnitDto, BaseUnit>();
    }
}

