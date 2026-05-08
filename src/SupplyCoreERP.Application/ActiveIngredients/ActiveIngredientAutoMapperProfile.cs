using AutoMapper;
using SupplyCoreERP.ActiveIngredients.Dtos;

namespace SupplyCoreERP.ActiveIngredients;

public class ActiveIngredientAutoMapperProfile : Profile
{
    public ActiveIngredientAutoMapperProfile()
    {
        CreateMap<ActiveIngredient, ActiveIngredientDto>();
        CreateMap<CreateUpdateActiveIngredientDto, ActiveIngredient>();
    }
}
