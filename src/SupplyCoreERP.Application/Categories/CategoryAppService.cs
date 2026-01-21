using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Categories.Dtos;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Categories
{
	public class CategoryAppService :CrudAppService<
		Category, 
		CategoryDto, 
		Guid, 
		GetCategoryListDto, 
		CreateUpdateCategoryDto>,
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

		protected override async Task<IQueryable<Category>> CreateFilteredQueryAsync(GetCategoryListDto input)
		{
			var query = await base.CreateFilteredQueryAsync(input);

			if (!input.Filter.IsNullOrWhiteSpace())
			{
				query = query.Where(x => x.Name.Contains(input.Filter));
			}

			return query;
		}

		public override async Task<PagedResultDto<CategoryDto>> GetListAsync(GetCategoryListDto input)
		{
			var queryable = await CreateFilteredQueryAsync(input);

			var queryDto = queryable.Select(x => new CategoryDto
			{
				Id = x.Id,
				Name = x.Name,
				CreationTime = x.CreationTime,
				CreatorId = x.CreatorId,
				LastModificationTime = x.LastModificationTime,
				LastModifierId = x.LastModifierId,
				ProductCount = x.Products.Count()
			});

			var totalCount = await queryDto.CountAsync();

			var items = await queryDto
				.OrderBy(input.Sorting ?? nameof(Category.CreationTime) + " DESC") 
				.PageBy(input) 
				.ToListAsync();

			return new PagedResultDto<CategoryDto>(totalCount, items);
		}
	}
}
