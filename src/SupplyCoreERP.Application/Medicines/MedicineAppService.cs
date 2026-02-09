using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using SupplyCoreERP.ActiveIngredients;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Medicines.Dtos;
using SupplyCoreERP.Permissions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core; 
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Medicines
{
	public class MedicineAppService : ApplicationService, IMedicineAppService
	{
		private readonly IRepository<Medicine, Guid> _medicineRepo;
		private readonly MedicineManager _medicineManager;
		private readonly IRepository<ActiveIngredient, Guid> _ingredientRepo;

		public MedicineAppService(
			IRepository<Medicine, Guid> medicineRepo,
			MedicineManager medicineManager,
			IRepository<ActiveIngredient, Guid> ingredientRepo)
		{
			_medicineRepo = medicineRepo;
			_medicineManager = medicineManager;
			_ingredientRepo = ingredientRepo;
		}

		public async Task<PagedResultDto<MedicineDto>> GetListAsync(GetMedicineListDto input)
		{
			var isManager = await AuthorizationService.IsGrantedAsync(SupplyCoreERPPermissions.Catalog.Medicine.Approve);

			//Queryable
			var query = await _medicineRepo.GetQueryableAsync();

			//JOIN BẢNG (Eager Loading) -> Để AutoMapper có dữ liệu map Name
			query = query
				.Include(x => x.Category)
				.Include(x => x.Manufacturer).ThenInclude(m => m.Country)
				.Include(x => x.BaseUnit)
				.Include(x => x.DosageForm);

			//Filter Logic
			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter) || x.Code.Contains(input.Filter))
				.WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
				.WhereIf(input.ManufacturerId.HasValue, x => x.ManufacturerId == input.ManufacturerId);

			if (!isManager)
			{
				query = query.Where(x => x.Status == MedicineStatus.Approved && x.IsActive);
			}
			else
			{
				query = query
					.WhereIf(input.Status.HasValue, x => x.Status == (MedicineStatus)input.Status)
					.WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
			}

			//Sort & Paging
			var totalCount = await AsyncExecuter.CountAsync(query);

			query = query
				.OrderBy(input.Sorting ?? nameof(Medicine.CreationTime) + " DESC")
				.PageBy(input);

			var items = await AsyncExecuter.ToListAsync(query);

			//Map to DTO (AutoMapper tự điền Name nhờ Include bên trên)
			var dtos = ObjectMapper.Map<List<Medicine>, List<MedicineDto>>(items);

			return new PagedResultDto<MedicineDto>(totalCount, dtos);
		}

		public async Task<MedicineDetailDto> GetAsync(Guid id)
		{
			var query = await _medicineRepo.GetQueryableAsync();

			var entity = await query
				.Include(x => x.Category)      
				.Include(x => x.Manufacturer).ThenInclude(m => m.Country)
				.Include(x => x.BaseUnit)
				.Include(x => x.DosageForm)
				.Include(x => x.Ingredients).ThenInclude(i => i.ActiveIngredient)
				.Include(x => x.Units).ThenInclude(u => u.Unit)

				.FirstOrDefaultAsync(x => x.Id == id);

			if (entity == null) throw new EntityNotFoundException(typeof(Medicine), id);

			return ObjectMapper.Map<Medicine, MedicineDetailDto>(entity);
		}


		public async Task<MedicineDetailDto> CreateAsync(CreateUpdateMedicineDto input)
		{
			// Gọi Manager 
			var entity = await _medicineManager.CreateAsync(
				input.Code, input.Name, input.CategoryId, input.ManufacturerId,
				input.BaseUnitId, input.DosageFormId, input.RegistrationNumber
			);

			entity.SetPharmaInfo(input.UsageRoute, input.StorageCondition, input.IsPrescriptionDrug);
			entity.SetActive(input.IsActive);

			await _medicineRepo.InsertAsync(entity);

			// Map ngược lại DetailDto 
			return ObjectMapper.Map<Medicine, MedicineDetailDto>(entity);
		}

		public async Task<MedicineDetailDto> UpdateAsync(Guid id, CreateUpdateMedicineDto input)
		{
			var entity = await _medicineRepo.GetAsync(id);

			await _medicineManager.UpdateAsync(
				entity, input.Name, input.CategoryId, input.ManufacturerId,
				input.DosageFormId, input.RegistrationNumber
			);

			entity.UpdateCode(input.Code);
			entity.SetPharmaInfo(input.UsageRoute, input.StorageCondition, input.IsPrescriptionDrug);
			entity.SetActive(input.IsActive);

			await _medicineRepo.UpdateAsync(entity);
			return ObjectMapper.Map<Medicine, MedicineDetailDto>(entity);
		}

		public async Task DeleteAsync(Guid id)
		{
			await _medicineRepo.DeleteAsync(id);
		}

		public async Task ApproveAsync(Guid id)
		{
			var entity = await _medicineRepo.GetAsync(id);
			entity.Approve();
			await _medicineRepo.UpdateAsync(entity);
		}

		public async Task RejectAsync(Guid id)
		{
			var entity = await _medicineRepo.GetAsync(id);
			entity.Reject();
			await _medicineRepo.UpdateAsync(entity);
		}

		public async Task ToggleActiveAsync(Guid id)
		{
			var entity = await _medicineRepo.GetAsync(id);
			entity.SetActive(!entity.IsActive);
			await _medicineRepo.UpdateAsync(entity);
		}

		#region Ingredients
		public async Task AddIngredientAsync(Guid id, CreateUpdateMedicineIngredientDto input)
		{
			var query = await _medicineRepo.GetQueryableAsync();
			var medicine = await query
				.Include(x => x.Ingredients) 
				.FirstOrDefaultAsync(x => x.Id == id);

			if (medicine == null)
				throw new EntityNotFoundException(typeof(Medicine), id);

			await _medicineManager.AddIngredientAsync(medicine, input.ActiveIngredientId);

			await _medicineRepo.UpdateAsync(medicine);
		}

		public async Task RemoveIngredientAsync(Guid id, Guid activeIngredientId)
		{
			var medicine = await _medicineRepo.GetAsync(id, includeDetails: true);
			await _medicineManager.RemoveIngredientAsync(medicine, activeIngredientId);
			await _medicineRepo.UpdateAsync(medicine);
		}
		#endregion

		#region Units
		public async Task AddUnitAsync(Guid id, CreateUpdateMedicineUnitDto input)
		{
			var query = await _medicineRepo.GetQueryableAsync();

			var medicine = await query
				.Include(x => x.Units)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (medicine == null)
				throw new EntityNotFoundException(typeof(Medicine), id);
			medicine.AddUnit(GuidGenerator.Create(), input.UnitId, input.ConversionFactor, input.Level);

			await _medicineRepo.UpdateAsync(medicine);
		}

		public async Task UpdateUnitAsync(Guid id, Guid unitId, CreateUpdateMedicineUnitDto input)
		{
			var query = await _medicineRepo.GetQueryableAsync();

			var medicine = await query
				.Include(x => x.Units) 
				.FirstOrDefaultAsync(x => x.Id == id);

			if (medicine == null)
				throw new EntityNotFoundException(typeof(Medicine), id);
			medicine.UpdateUnit(unitId, input.ConversionFactor, input.Level);

			await _medicineRepo.UpdateAsync(medicine);
		}

		public async Task RemoveUnitAsync(Guid id, Guid unitId)
		{
			var medicine = await _medicineRepo.GetAsync(id, includeDetails: true);
			medicine.RemoveUnit(unitId);
			await _medicineRepo.UpdateAsync(medicine);
		}
		#endregion

		public async Task<IRemoteStreamContent> GetListAsExcelFileAsync(GetMedicineListDto input)
		{
			var query = await _medicineRepo.GetQueryableAsync();

			query = query
				.Include(x => x.Category)
				.Include(x => x.Manufacturer).ThenInclude(m => m.Country)
				.Include(x => x.BaseUnit)
				.Include(x => x.DosageForm)
				.Include(x => x.Ingredients).ThenInclude(i => i.ActiveIngredient)
				.Include(x => x.Units).ThenInclude(u => u.Unit);

			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter) || x.Code.Contains(input.Filter))
				.WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
				.WhereIf(input.ManufacturerId.HasValue, x => x.ManufacturerId == input.ManufacturerId)
				.WhereIf(input.Status.HasValue, x => x.Status == (MedicineStatus)input.Status);

			var items = await AsyncExecuter.ToListAsync(query);

			var exportData = items.Select(x => new MedicineExportDto
			{
				Code = x.Code,
				Name = x.Name,
				Category = x.Category?.Name,
				Manufacturer = x.Manufacturer?.Name,
				OriginCountry = x.Manufacturer?.Country?.Name,
				BaseUnit = x.BaseUnit?.Name,
				DosageForm = x.DosageForm?.Name,
				RegistrationNumber = x.RegistrationNumber,

				//Enum
				UsageRoute = x.UsageRoute switch 
				{ 
					UsageRoute.Oral => "Uống",
					UsageRoute.Injection => "Tiêm",
					UsageRoute.External => "Ngoài da",
					UsageRoute.Other => "Khác"	
				},
				StorageCondition = x.StorageCondition switch
				{ 
					StorageCondition.Normal => "Bình thường",
					StorageCondition.Cool => "Mát",
					StorageCondition.Cold => "Lạnh",
					StorageCondition.Frozen => "Đông"
				},
				IsPrescriptionDrug = x.IsPrescriptionDrug ? "Có (Rx)" : "Không",

				//Status
				Status = x.Status switch
				{
					MedicineStatus.Pending => "Chờ duyệt",
					MedicineStatus.Approved => "Đã duyệt",
					MedicineStatus.Rejected => "Từ chối",
					_ => ""
				},
				IsActive = x.IsActive ? "Hoạt động" : "Ngừng",

				//Ingredients
				Ingredients = x.Ingredients != null && x.Ingredients.Any()
					? string.Join("; ", x.Ingredients.Select(i => i.ActiveIngredient?.Name))
					: "",

				//Units
				Units = x.Units != null && x.Units.Any()
					? string.Join("; ", x.Units.Select(u => $"{u.Unit?.Name} (x{u.ConversionFactor})"))
					: "",

				CreationTime = x.CreationTime
			});

			var memoryStream = new MemoryStream();
			await memoryStream.SaveAsAsync(exportData);
			memoryStream.Seek(0, SeekOrigin.Begin);

			return new RemoteStreamContent(
				memoryStream,
				$"DS_Thuoc_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		}
	}
}
