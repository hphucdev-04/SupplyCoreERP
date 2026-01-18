using SupplyCoreERP.Categories.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Categories
{
	public class CategoryAppService :
		CrudAppService<Category, CategoryDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCategoryDto>,
		ICategoryAppService
	{
		private readonly CategoryManager _categoryManager;
		public CategoryAppService(
			IRepository<Category, Guid> repository,
			CategoryManager categoryManager) : base(repository)
		{
			_categoryManager = categoryManager;
		}

		public override async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto input)
		{
			//Manager để lấy Entity hợp lệ 
			var category = await _categoryManager.CreateAsync(input.Name);
			await Repository.InsertAsync(category, autoSave: true);

			//Map ra DTO
			return ObjectMapper.Map<Category, CategoryDto>(category);
		}
	}
}
