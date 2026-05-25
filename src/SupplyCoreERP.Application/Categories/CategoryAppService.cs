using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Catalog.Categories;
using SupplyCoreERP.Categories.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace SupplyCoreERP.Categories;

public class CategoryAppService : CrudAppService<
    Category,
    CategoryDto,
    Guid,
    GetCategoryListDto,
    CreateUpdateCategoryDto>,
    ICategoryAppService
{
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly CategoryManager _categoryManager;

    public CategoryAppService(
        IRepository<Category, Guid> categoryRepository,
        CategoryManager categoryManager,
        IObjectMapper objectMapper) : base(categoryRepository)
    {
        _categoryRepository = categoryRepository;
        _categoryManager = categoryManager;
    }

    public override async Task<CategoryDto> CreateAsync(CreateUpdateCategoryDto input)
    {
        Category category = await _categoryManager.CreateAsync(input.Name);
        await _categoryRepository.InsertAsync(category, autoSave: true);
        return ObjectMapper.Map<Category, CategoryDto>(category);
    }

    public override async Task<CategoryDto> UpdateAsync(Guid id, CreateUpdateCategoryDto input)
    {
        Category category = await _categoryRepository.GetAsync(id);

        await _categoryManager.UpdateAsync(category, input.Name);
        await _categoryRepository.UpdateAsync(category, autoSave: true);

        return ObjectMapper.Map<Category, CategoryDto>(category);
    }

    public override async Task DeleteAsync(Guid id)
    {
        Category category = await _categoryRepository.GetAsync(id);
        await _categoryManager.DeleteAsync(category);
    }

    protected override async Task<IQueryable<Category>> CreateFilteredQueryAsync(GetCategoryListDto input)
    {
        IQueryable<Category> query = await base.CreateFilteredQueryAsync(input);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            query = query.Where(x => x.Name.ToLower().Contains(input.Filter.ToLower()));
        }

        return query;
    }

    public override async Task<PagedResultDto<CategoryDto>> GetListAsync(GetCategoryListDto input)
    {
        IQueryable<Category> queryable = await CreateFilteredQueryAsync(input);

        IQueryable<CategoryDto> queryDto = queryable.Select(x => new CategoryDto
        {
            Id = x.Id,
            Name = x.Name,
            CreationTime = x.CreationTime,
            CreatorId = x.CreatorId,
            LastModificationTime = x.LastModificationTime,
            LastModifierId = x.LastModifierId,
            ProductCount = x.Products.Count()
        });

        int totalCount = await queryDto.CountAsync();

        List<CategoryDto> items = await queryDto
            .OrderBy(input.Sorting ?? nameof(Category.CreationTime) + " DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<CategoryDto>(totalCount, items);
    }
}

