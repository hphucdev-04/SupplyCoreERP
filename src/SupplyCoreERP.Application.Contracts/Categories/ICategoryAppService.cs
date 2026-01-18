using SupplyCoreERP.Categories.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Categories
{
	public interface ICategoryAppService : ICrudAppService<
		CategoryDto, 
		Guid, 
		PagedAndSortedResultRequestDto, 
		CreateUpdateCategoryDto>
	{

	}
}
