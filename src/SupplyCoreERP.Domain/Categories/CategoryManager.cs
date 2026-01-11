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

		public CategoryManager(IRepository<Category, Guid> categoryRepository)
		{
			_categoryRepository = categoryRepository;
		}

		public async Task<Category> CreateAsync(string code, string name, string description = null)
		{
			Check.NotNullOrWhiteSpace(code, nameof(code));
			Check.NotNullOrWhiteSpace(name, nameof(name));

			//Mã viết hoa, cắt khoảng trắng
			var normalizedCode = code.Trim().ToUpper();
			var normalizedName = name.Trim();

			// Check 1: Kiểm tra trùng Mã 
			if (await _categoryRepository.AnyAsync(x => x.Code == normalizedCode))
			{
				throw new UserFriendlyException($"Mã nhóm '{code}' đã tồn tại trong hệ thống!");
			}

			// Check 2: Kiểm tra trùng Tên 
			if (await _categoryRepository.AnyAsync(x => x.Name == normalizedName))
			{
				throw new UserFriendlyException($"Tên nhóm '{name}' đã tồn tại!");
			}

			return new Category(
				GuidGenerator.Create(),
				code,
				name,
				description
			);
		}

		public async Task UpdateAsync(Category category, string newName, string newDescription)
		{
			Check.NotNull(category, nameof(category));
			Check.NotNullOrWhiteSpace(newName, nameof(newName));

			var normalizedName = newName.Trim();

			// Nếu tên không đổi thì không cần check, chỉ update mô tả
			if (category.Name == normalizedName)
			{
				category.SetDescription(newDescription);
				return;
			}

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
			category.SetDescription(newDescription);
		}
	}
}