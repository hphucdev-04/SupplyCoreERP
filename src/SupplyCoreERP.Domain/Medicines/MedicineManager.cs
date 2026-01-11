using SupplyCoreERP.Categories;
using SupplyCoreERP.Enums.Medicines;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Medicines
{
	public class MedicineManager : DomainService
	{
		private readonly IRepository<Medicine, Guid> _medicineRepository;
		private readonly IRepository<Category, Guid> _categoryRepository;

		public MedicineManager(
			IRepository<Medicine, Guid> medicineRepository,
			IRepository<Category, Guid> categoryRepository)
		{
			_medicineRepository = medicineRepository;
			_categoryRepository = categoryRepository;
		}

		public async Task<Medicine> CreateAsync(
			string code,
			string name,
			Guid categoryId,
			string baseUnit,
			ProductType type)
		{
			// Check 1: Nhóm thuốc có tồn tại không
			if (!await _categoryRepository.AnyAsync(x => x.Id == categoryId))
			{
				throw new UserFriendlyException("Nhóm thuốc không tồn tại!");
			}

			// Check 2: Trùng Mã
			var normalizedCode = code?.Trim().ToUpper();
			if (await _medicineRepository.AnyAsync(x => x.Code == normalizedCode))
			{
				throw new UserFriendlyException($"Mã thuốc '{code}' đã tồn tại trong hệ thống!");
			}

			// Check 3: Trùng Tên
			var normalizedName = name?.Trim();
			if (await _medicineRepository.AnyAsync(x => x.Name == normalizedName))
			{
				throw new UserFriendlyException($"Tên thuốc '{name}' đã tồn tại trong hệ thống!");
			}

			return new Medicine(
				GuidGenerator.Create(),
				categoryId,
				code,
				name,
				baseUnit,
				type
			);
		}

		public async Task ChangeCodeAsync(Medicine medicine, string newCode)
		{
			Check.NotNull(medicine, nameof(medicine));
			Check.NotNullOrWhiteSpace(newCode, nameof(newCode));

			var normalizedCode = newCode.Trim().ToUpper();

			// Nếu mã không đổi thì thoát
			if (medicine.Code == normalizedCode) return;

			// Kiểm tra trùng với các thuốc khác
			if (await _medicineRepository.AnyAsync(x => x.Code == normalizedCode && x.Id != medicine.Id))
			{
				throw new UserFriendlyException($"Mã thuốc '{newCode}' đã bị sử dụng bởi thuốc khác!");
			}

			medicine.SetCode(newCode);
		}

		public async Task ChangeNameAsync(Medicine medicine, string newName)
		{
			Check.NotNull(medicine, nameof(medicine));
			Check.NotNullOrWhiteSpace(newName, nameof(newName));

			var normalizedName = newName.Trim();

			// Nếu tên không đổi thì thoát
			if (medicine.Name == normalizedName) return;

			// Kiểm tra trùng với các thuốc khác
			if (await _medicineRepository.AnyAsync(x => x.Name == normalizedName && x.Id != medicine.Id))
			{
				throw new UserFriendlyException($"Tên thuốc '{newName}' đã bị trùng!");
			}

			medicine.SetName(newName);
		}

		public async Task ChangeCategoryAsync(Medicine medicine, Guid newCategoryId)
		{
			Check.NotNull(medicine, nameof(medicine));

			if (medicine.CategoryId == newCategoryId) return;

			if (!await _categoryRepository.AnyAsync(x => x.Id == newCategoryId))
			{
				throw new UserFriendlyException("Nhóm thuốc mới không tồn tại!");
			}

			medicine.SetCategory(newCategoryId);
		}
	}
}