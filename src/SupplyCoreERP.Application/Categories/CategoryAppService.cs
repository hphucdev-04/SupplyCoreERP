using SupplyCoreERP.Categories.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Categories
{
	public class CategoryAppService : CrudAppService<
		Category,
		CategoryDto,
		Guid,
		PagedAndSortedResultRequestDto,
		CreateUpdateCategoryDto,
		CreateUpdateCategoryDto>,
		ICategoryAppService
	{
		private readonly CategoryManager _categoryManager;

		public CategoryAppService(
			IRepository<Category, Guid> repository,
			CategoryManager categoryManager)
			: base(repository)
		{
			_categoryManager = categoryManager;
		}

		public override async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto input)
		{
			var category = await _categoryManager.CreateAsync(input.Code, input.Name, input.Description);
			await Repository.InsertAsync(category);
			return ObjectMapper.Map<Category, CategoryDto>(category);
		}
	}
}
