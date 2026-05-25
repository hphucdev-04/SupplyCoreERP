using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Catalog.Categories;

public class CategoryManager : DomainService
{
    // Dependencies
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IGuidGenerator _guidGenerator;

    // Constructor injection
    public CategoryManager(
        IRepository<Category, Guid> categoryRepository,
        IRepository<Product, Guid> productRepository,
        IGuidGenerator guidGenerator)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _guidGenerator = guidGenerator;
    }

    public virtual async Task<Category> CreateAsync(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));

        // Check duplicate tên nhóm 
        if (await _categoryRepository.AnyAsync(x => x.Name.ToLower() == name.ToLower()))
        {
            throw new BusinessException("SupplyCoreERP:InvalidCategoryName", $"Tên nhóm '{name}' đã tồn tại!");
        }

        return new Category(
            _guidGenerator.Create(),
            name
        );
    }

    public virtual async Task UpdateAsync(Category category, string newName)
    {
        Check.NotNull(category, nameof(category));
        Check.NotNullOrWhiteSpace(newName, nameof(newName));

        string normalizedName = newName.Trim();

        // Check duplicate tên nhóm 
        if (await _categoryRepository.AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower() && x.Id != category.Id))
        {
            throw new BusinessException("SupplyCoreERP:InvalidCategoryName", $"Tên nhóm '{newName}' đã bị sử dụng bởi nhóm khác!");
        }

        category.SetName(newName);
    }

    public virtual async Task DeleteAsync(Category category)
    {
        Check.NotNull(category, nameof(category));

        //Check sản phẩm thuộc nhóm này
        bool isInUse = await _productRepository.AnyAsync(x => x.CategoryId == category.Id);

        if (isInUse)
        {
            throw new BusinessException("SupplyCoreERP:CategoryInUse", $"Không thể xóa nhóm '{category.Name}' vì đang có sản phẩm thuộc nhóm này!");
        }

        await _categoryRepository.DeleteAsync(category);
    }

}







