using System;
using System.Threading.Tasks;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.Categories;

public class CategoryManager : DomainService, ICategoryManager
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CategoryManager(
        ICategoryRepository categoryRepository,
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

        // Dùng phương thức minh bạch từ Interface, dễ dàng Unit Test
        if (await _categoryRepository.IsNameExistsAsync(name))
        {
            throw new UserFriendlyException($"Tên nhóm '{name}' đã tồn tại!");
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

        var normalizedName = newName.Trim();

        //  Kiểm tra trùng tên với nhóm khác
        if (await _categoryRepository.IsNameExistsAsync(normalizedName, category.Id))
        {
            throw new UserFriendlyException($"Tên nhóm '{newName}' đã bị sử dụng bởi nhóm khác!");
        }

        category.SetName(newName);
    }

    public virtual async Task DeleteAsync(Category category)
    {
        Check.NotNull(category, nameof(category));

        //Check sản phẩm thuộc nhóm
        var isInUse = await _productRepository.AnyAsync(x => x.CategoryId == category.Id);

        if (isInUse)
        {
            //Có - chặn
            throw new UserFriendlyException($"Không thể xóa nhóm '{category.Name}' vì đang có sản phẩm thuộc nhóm này!");
        }

        //Không - xóa
        await _categoryRepository.DeleteAsync(category);
    }

}
