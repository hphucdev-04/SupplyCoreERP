using AutoMapper;
using SupplyCoreERP.Categories.Dtos;

namespace SupplyCoreERP.Categories;

public class CategoryAutoMapperProfile : Profile
{
    public CategoryAutoMapperProfile()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CreateUpdateCategoryDto, Category>();
    }
}
