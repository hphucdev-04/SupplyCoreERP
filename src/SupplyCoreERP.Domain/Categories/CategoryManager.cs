using SupplyCoreERP.Products;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Categories
{
	public class CategoryManager : DomainService
	{
		private readonly IRepository<Category, Guid> _categoryRepository;
		private readonly IRepository<Product, Guid> _productRepository;
		public CategoryManager(
			IRepository<Category, Guid> categoryRepository,
			IRepository<Product, Guid> productRepository)
		{
			_categoryRepository = categoryRepository;
			_productRepository = productRepository;
		}

		public async Task<Category> CreateAsync(string name)
		{
			Check.NotNullOrWhiteSpace(name, nameof(name));
			var normalizedName = name.Trim();

			// Check: Kiểm tra trùng Tên 
			if (await _categoryRepository.AnyAsync(x => x.Name == normalizedName))
			{
				throw new UserFriendlyException($"Tên nhóm '{name}' đã tồn tại!");
			}

			return new Category(
				GuidGenerator.Create(),
				name
			);
		}

		public async Task UpdateAsync(Category category, string newName)
		{
			Check.NotNull(category, nameof(category));
			Check.NotNullOrWhiteSpace(newName, nameof(newName));

			var normalizedName = newName.Trim();

			//  Kiểm tra trùng tên với nhóm khác
			var isDuplicateName = await _categoryRepository.AnyAsync(x =>
				x.Name == normalizedName &&
				x.Id != category.Id
			);

			if (isDuplicateName)
			{
				throw new UserFriendlyException($"Tên nhóm '{newName}' đã bị sử dụng bởi nhóm khác!");
			}

			category.SetName(newName);
		}

		public async Task DeleteAsync(Category category)
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
}