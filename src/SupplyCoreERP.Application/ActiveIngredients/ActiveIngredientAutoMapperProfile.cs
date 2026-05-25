using AutoMapper;
using SupplyCoreERP.ActiveIngredients.Dtos;
using SupplyCoreERP.Catalog.ActiveIngredients;

namespace SupplyCoreERP.ActiveIngredients;

public class ActiveIngredientAutoMapperProfile : Profile
{
    public ActiveIngredientAutoMapperProfile()
    {
        CreateMap<ActiveIngredient, ActiveIngredientDto>();
        CreateMap<CreateUpdateActiveIngredientDto, ActiveIngredient>();
    }
}

