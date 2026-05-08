using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Categories.Dtos;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

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
		private readonly ICategoryRepository _categoryRepository;
		private readonly ICategoryManager _categoryManager;
		private readonly IObjectMapper _objectMapper;

		public CategoryAppService(
			ICategoryRepository categoryRepository,
			ICategoryManager categoryManager,
			IObjectMapper objectMapper) : base(categoryRepository)
		{
			_categoryRepository = categoryRepository;
			_categoryManager = categoryManager;
			_objectMapper = objectMapper;
		}

		public override async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto input)
		{
			//Manager để lấy Entity hợp lệ 
			var category = await _categoryManager.CreateAsync(input.Name);
			await _categoryRepository.InsertAsync(category, autoSave: true);

			//Map ra DTO bằng mapper được inject trực tiếp
			return _objectMapper.Map<Category, CategoryDto>(category);
		}

		protected override async Task<IQueryable<Category>> CreateFilteredQueryAsync(GetCategoryListDto input)
		{
			var query = await base.CreateFilteredQueryAsync(input);

			if (!input.Filter.IsNullOrWhiteSpace())
			{
				query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
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
