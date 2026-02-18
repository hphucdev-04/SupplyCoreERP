using SupplyCoreERP.Categories.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Categories
{
	public interface ICategoryAppService : ICrudAppService<
		CategoryDto, 
		Guid, 
		GetCategoryListDto, 
		CreateUpdateCategoryDto>
	{

	}
}
