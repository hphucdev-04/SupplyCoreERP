using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Medicines;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;


namespace SupplyCoreERP.ActiveIngredients
{
	public class ActiveIngredientManager : DomainService
	{
		private readonly IRepository<ActiveIngredient, Guid> _repository;
		private readonly IRepository<MedicineIngredient, Guid> _medIngredientRepo;
        private readonly DocumentSequenceManager _documentSequenceManager;


        public ActiveIngredientManager(
			IRepository<ActiveIngredient, Guid> repository,
			IRepository<MedicineIngredient, Guid> medIngredientRepo,
            DocumentSequenceManager documentSequenceManager
            )
		{
			_repository = repository;
			_medIngredientRepo = medIngredientRepo;
			_documentSequenceManager = documentSequenceManager;
		}

		public async Task<ActiveIngredient> CreateAsync(string name)
		{
			var code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeIngredient);

            if (await _repository.AnyAsync(x => x.Code == code))
				throw new UserFriendlyException($"Mã hoạt chất '{code}' đã tồn tại!");

			return new ActiveIngredient(GuidGenerator.Create(), code, name);
		}

		public async Task UpdateAsync(ActiveIngredient entity, string newName)
		{
			entity.Update(newName);
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
