using SupplyCoreERP.ActiveIngredients;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Categories;
using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Manufacturers;
using SupplyCoreERP.Products;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Medicines
{
	public class MedicineManager : DomainService
	{
		private readonly ProductManager _productManager;
		private readonly IRepository<Category, Guid> _categoryRepository;
		private readonly IRepository<Manufacturer, Guid> _manufacturerRepository;
		private readonly IRepository<BaseUnit, Guid> _unitRepository;
		private readonly IRepository<DosageForm, Guid> _dosageFormRepository;
		private readonly IRepository<ActiveIngredient, Guid> _activeIngredientRepository;
		private readonly IRepository<Country, Guid> _countryRepository;

		public MedicineManager(
			ProductManager productManager,
			IRepository<Category, Guid> categoryRepository,
			IRepository<Manufacturer, Guid> manufacturerRepository,
			IRepository<BaseUnit, Guid> unitRepository,
			IRepository<DosageForm, Guid> dosageFormRepository,
			IRepository<ActiveIngredient, Guid> activeIngredientRepository,
			IRepository<Country, Guid> countryRepository 
			)
		{
			_productManager = productManager;
			_categoryRepository = categoryRepository;
			_manufacturerRepository = manufacturerRepository;
			_unitRepository = unitRepository;
			_dosageFormRepository = dosageFormRepository;
			_activeIngredientRepository = activeIngredientRepository;
			_countryRepository = countryRepository; 
		}

		public async Task<Medicine> CreateAsync(
			string code,
			string name,
			Guid categoryId,
			Guid manufacturerId, 
			Guid baseUnitId,
			Guid dosageFormId,
			string regNumber)
		{
			//Validate các khóa ngoại
			await ValidateForeignKeysAsync(categoryId, manufacturerId, baseUnitId, dosageFormId); 

			//Check trùng Code/Name
			await _productManager.CheckCodeAndNameAsync(code, name);

			return new Medicine(
				GuidGenerator.Create(),
				categoryId,
				manufacturerId,
				code,
				name,
				baseUnitId,
				dosageFormId,
				regNumber
			);
		}

		public async Task UpdateAsync(
			Medicine medicine,
			string name,
			Guid categoryId,
			Guid manufacturerId, 
			Guid dosageFormId,
			string regNumber,
			string code)
		{
			Check.NotNull(medicine, nameof(medicine));

			//Validate khóa ngoại mới
			await ValidateForeignKeysAsync(categoryId, manufacturerId, medicine.BaseUnitId, dosageFormId);

			//Check trùng tên
			await _productManager.CheckCodeAndNameAsync(code, name, excludeId: medicine.Id);

			medicine.UpdateCode(code);

			//Update thông tin chung (Product)
			medicine.UpdateInfo(name, categoryId, manufacturerId);

			//Update thông tin riêng (Medicine)
			medicine.UpdatePharmaInfo(
				dosageFormId,
				regNumber,
				medicine.UsageRoute,
				medicine.StorageCondition,
				medicine.IsPrescriptionDrug
			);
		}

		private async Task ValidateForeignKeysAsync(Guid catId, Guid manuId, Guid unitId, Guid dosageId)
		{
			if (!await _categoryRepository.AnyAsync(x => x.Id == catId))
				throw new UserFriendlyException("Nhóm hàng không tồn tại.");

			if (!await _manufacturerRepository.AnyAsync(x => x.Id == manuId))
				throw new UserFriendlyException("Nhà sản xuất không tồn tại.");

			if (!await _unitRepository.AnyAsync(x => x.Id == unitId))
				throw new UserFriendlyException("Đơn vị tính không tồn tại.");

			if (!await _dosageFormRepository.AnyAsync(x => x.Id == dosageId))
				throw new UserFriendlyException("Dạng bào chế không tồn tại.");

		}

		public async Task AddIngredientAsync(Medicine medicine, Guid activeIngredientId)
		{
			Check.NotNull(medicine, nameof(medicine));
			if (!await _activeIngredientRepository.AnyAsync(x => x.Id == activeIngredientId))
			{
				throw new UserFriendlyException("Hoạt chất không tồn tại trong danh mục!");
			}
			medicine.AddIngredient(activeIngredientId);
		}

		public async Task RemoveIngredientAsync(Medicine medicine, Guid activeIngredientId)
		{
			Check.NotNull(medicine, nameof(medicine));
			medicine.RemoveIngredient(activeIngredientId);
			await Task.CompletedTask;
		}
	}
}