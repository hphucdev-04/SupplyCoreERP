using SupplyCoreERP.Medicines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.ActiveIngredients
{
	public class ActiveIngredientManager : DomainService
	{
		private readonly IRepository<ActiveIngredient, Guid> _repository;
		private readonly IRepository<MedicineIngredient, Guid> _medIngredientRepo;

		public ActiveIngredientManager(
			IRepository<ActiveIngredient, Guid> repository,
			IRepository<MedicineIngredient, Guid> medIngredientRepo)
		{
			_repository = repository;
			_medIngredientRepo = medIngredientRepo;
		}

		public async Task<ActiveIngredient> CreateAsync(string code, string name)
		{
			Check.NotNullOrWhiteSpace(code, nameof(code));
			var normalizedCode = code.Trim().ToUpper();

			if (await _repository.AnyAsync(x => x.Code == normalizedCode))
				throw new UserFriendlyException($"Mã hoạt chất '{code}' đã tồn tại!");

			return new ActiveIngredient(GuidGenerator.Create(), normalizedCode, name);
		}

		public async Task UpdateAsync(ActiveIngredient entity, string newCode, string newName)
		{
			Check.NotNull(entity, nameof(entity));
			var normalizedCode = newCode.Trim().ToUpper();

			// Check trùng với thằng khác
			if (await _repository.AnyAsync(x => x.Code == normalizedCode && x.Id != entity.Id))
				throw new UserFriendlyException($"Mã hoạt chất '{newCode}' đã bị sử dụng!");

			entity.Update(normalizedCode, newName);
		}

		public async Task DeleteAsync(ActiveIngredient entity)
		{
			//Check xem có thuốc nào đang dùng hoạt chất này không
			var isUsed = await _medIngredientRepo.AnyAsync(x => x.ActiveIngredientId == entity.Id);

			if (isUsed)
			{
				throw new UserFriendlyException($"Không thể xóa hoạt chất '{entity.Name}' vì đang có thuốc sử dụng nó!");
			}

			await _repository.DeleteAsync(entity);
		}
	}
}
