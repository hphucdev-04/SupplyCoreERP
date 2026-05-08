using System;
using SupplyCoreERP.Categories.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Categories;

public interface ICategoryAppService : ICrudAppService<
    CategoryDto,
    Guid,
    GetCategoryListDto,
    CreateUpdateCategoryDto>
{

}
