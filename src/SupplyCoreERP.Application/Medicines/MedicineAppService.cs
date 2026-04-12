using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SupplyCoreERP.ActiveIngredients;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Categories;
using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Manufacturers;
using SupplyCoreERP.Medicines.Dtos;
using SupplyCoreERP.Permissions;
using SupplyCoreERP.Prices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
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
		private readonly IRepository<Category, Guid> _categoryRepo;
		private readonly IRepository<Manufacturer, Guid> _manufacturerRepo;
		private readonly IRepository<BaseUnit, Guid> _baseUnitRepo;
		private readonly IRepository<DosageForm, Guid> _dosageFormRepo;
		private readonly IRepository<ActiveIngredient, Guid> _ingredientRepo;
		private readonly IRepository<PriceList, Guid> _priceListRepo;
		private readonly IRepository<ProductPrice, Guid> _productPriceRepo;
		private readonly PriceManager _priceManager;

		public MedicineAppService(
			IRepository<Medicine, Guid> medicineRepo,
			MedicineManager medicineManager,
			IRepository<Category, Guid> categoryRepo,
			IRepository<Manufacturer, Guid> manufacturerRepo,
			IRepository<BaseUnit, Guid> baseUnitRepo,
			IRepository<DosageForm, Guid> dosageFormRepo,
			IRepository<ActiveIngredient, Guid> ingredientRepo,
			IRepository<PriceList, Guid> priceListRepo,
			IRepository<ProductPrice, Guid> productPriceRepo,
			PriceManager priceManager)
		{
			_medicineRepo = medicineRepo;
			_medicineManager = medicineManager;
			_categoryRepo = categoryRepo;
			_manufacturerRepo = manufacturerRepo;
			_baseUnitRepo = baseUnitRepo;
			_dosageFormRepo = dosageFormRepo;
			_ingredientRepo = ingredientRepo;
			_priceListRepo = priceListRepo;
			_productPriceRepo = productPriceRepo;
			_priceManager = priceManager;
		}
		#region Medicine
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
			var entity = await _medicineManager.CreateAsync(
				input.Code,
				input.Name,
				input.CategoryId,
				input.ManufacturerId,
				input.BaseUnitId,
				input.DosageFormId,
				input.RegistrationNumber,
				input.UsageRoute,
				input.StorageCondition,
				input.IsPrescriptionDrug
			);

			// IsActive là tùy chọn của người dùng, không thuộc business logic tạo mới
			entity.SetActive(input.IsActive);

			await _medicineRepo.InsertAsync(entity);

			return ObjectMapper.Map<Medicine, MedicineDetailDto>(entity);
		}

		public async Task<MedicineDetailDto> UpdateAsync(Guid id, CreateUpdateMedicineDto input)
		{
			var entity = await _medicineRepo.GetAsync(id);

			await _medicineManager.UpdateAsync(
				entity,
				input.Code,
				input.Name,
				input.CategoryId,
				input.ManufacturerId,
				input.BaseUnitId,
				input.DosageFormId,
				input.RegistrationNumber,
				input.UsageRoute,
				input.StorageCondition,
				input.IsPrescriptionDrug
			);

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

		public async Task<MedicineSummaryDto> GetSummaryAsync()
		{
			var query = await _medicineRepo.GetQueryableAsync();

			var summary = await query
				.GroupBy(x => 1)
				.Select(g => new MedicineSummaryDto
				{
					TotalCount = g.Count(),
					TotalActive = g.Count(x => x.IsActive),
					TotalInactive = g.Count(x => !x.IsActive),
					TotalApproved = g.Count(x => x.Status == MedicineStatus.Approved),
					TotalPending = g.Count(x => x.Status == MedicineStatus.Pending),
					TotalRejected = g.Count(x => x.Status == MedicineStatus.Rejected)
				})
				.FirstOrDefaultAsync();

			return summary ?? new MedicineSummaryDto();
		}
		#endregion

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
			var query = await _medicineRepo.GetQueryableAsync();
			var medicine = await query
				.Include(x => x.Ingredients)
				.FirstOrDefaultAsync(x => x.Id == id);

			if (medicine == null)
				throw new EntityNotFoundException(typeof(Medicine), id);

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
			var query = await _medicineRepo.GetQueryableAsync();

			var medicine = await query
				.Include(x => x.Units)          
				.FirstOrDefaultAsync(x => x.Id == id);

			if (medicine == null)
				throw new EntityNotFoundException(typeof(Medicine), id);

			medicine.RemoveUnit(unitId);
			await _medicineRepo.UpdateAsync(medicine);
		}
		#endregion

		#region Export Excel
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

			// Filter
			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter) || x.Code.Contains(input.Filter))
				.WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId)
				.WhereIf(input.ManufacturerId.HasValue, x => x.ManufacturerId == input.ManufacturerId)
				.WhereIf(input.Status.HasValue, x => x.Status == (MedicineStatus)input.Status);

			var items = await AsyncExecuter.ToListAsync(query);

			//Map Sheet 1
			var medicineData = items.Select(x => new MedicineExportDto
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

			var medicineIds = items.Select(x => x.Id).ToList();

			var priceQuery = await _productPriceRepo.GetQueryableAsync();

			var prices = await priceQuery
				.Include(x => x.PriceList)
				.Include(x => x.Unit)
				.Include(x => x.Product)
				.Where(x => medicineIds.Contains(x.ProductId))
				.OrderBy(x => x.Product.Name)
				.ThenBy(x => x.PriceList.Code)
				.ToListAsync();

			//Map Sheet 2
			var priceData = prices.Select(x => new MedicinePriceExportDto
			{
				MedicineCode = x.Product?.Code,
				MedicineName = x.Product?.Name,
				PriceListName = x.PriceList?.Name,
				UnitName = x.Unit?.Name,
				Price = x.Price,
				MinQuantity = x.MinQuantity,
				Currency = x.PriceList?.Currency.ToString()
			});

			var memoryStream = new MemoryStream();
			var sheets = new Dictionary<string, object>
			{
				{ "Danh sách thuốc", medicineData },
				{ "Bảng giá chi tiết", priceData }
			};

			await memoryStream.SaveAsAsync(sheets);

			memoryStream.Seek(0, SeekOrigin.Begin);

			return new RemoteStreamContent(
				memoryStream,
				$"DS_Thuoc_Va_Gia_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		}
		#endregion

		#region Import Excel
		public async Task ImportExcelAsync(IRemoteStreamContent file)
		{
			using var stream = file.GetStream();

			// Cache db  lên RAM
			var categories = (await _categoryRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
			var manufacturers = (await _manufacturerRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
			var units = (await _baseUnitRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
			var dosages = (await _dosageFormRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
			var ingredients = (await _ingredientRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
			var priceLists = (await _priceListRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);

			// Cache Mã thuốc đang có để check trùng
			var existingMedicines = (await _medicineRepo.GetListAsync()).ToDictionary(x => x.Code.ToUpper().Trim(), x => x.Id);

			var errors = new List<string>();
			int rowIndex = 1;

			// Sheet 1 Danh sách thuốc
			var medRows = stream.Query<MedicineImportDto>("Danh sách thuốc").ToList();
			if (!medRows.Any()) medRows = stream.Query<MedicineImportDto>().ToList();

			foreach (var row in medRows)
			{
				rowIndex++;
				try
				{
					if (string.IsNullOrWhiteSpace(row.Code) || string.IsNullOrWhiteSpace(row.Name)) continue;

					var codeKey = row.Code.ToUpper().Trim();

					// Check trùng 
					if (existingMedicines.ContainsKey(codeKey))
					{
						errors.Add($"[Thuốc] Dòng {rowIndex}: Mã thuốc '{row.Code}' đã tồn tại trong hệ thống. Bỏ qua.");
						continue; // Bỏ qua, không làm gì cả
					}

					// Tìm ID
					var catId = GetId(categories, row.Category, $"Dòng {rowIndex}: Nhóm hàng '{row.Category}' không tồn tại");
					var manuId = GetId(manufacturers, row.Manufacturer, $"Dòng {rowIndex}: NSX '{row.Manufacturer}' không tồn tại");
					var baseUnitId = GetId(units, row.BaseUnit, $"Dòng {rowIndex}: Đơn vị '{row.BaseUnit}' không tồn tại");
					var dosageId = GetId(dosages, row.DosageForm, $"Dòng {rowIndex}: Dạng bào chế '{row.DosageForm}' không tồn tại");

					// Manager tạo entity với đầy đủ thông tin ngay từ đầu
					var medicine = await _medicineManager.CreateAsync(
						row.Code, row.Name, catId, manuId, baseUnitId, dosageId,
						row.RegistrationNumber,
						ParseUsageRoute(row.UsageRoute),
						ParseStorage(row.StorageCondition),
						ParseBool(row.IsPrescriptionDrug)
					);

					medicine.SetStatus(MedicineStatus.Pending);
					medicine.SetActive(true);

					// Ingredients
					if (!string.IsNullOrWhiteSpace(row.Ingredients))
					{
						foreach (var name in row.Ingredients.Split(';'))
						{
							if (ingredients.TryGetValue(name.Trim().ToLower(), out var iId)) medicine.AddIngredient(iId);
						}
					}

					// Units
					if (!string.IsNullOrWhiteSpace(row.Units))
					{
						foreach (var item in row.Units.Split(';'))
						{
							var match = Regex.Match(item.Trim(), @"^(.*?)\s*\(x(\d+)\)$");
							if (match.Success && units.TryGetValue(match.Groups[1].Value.Trim().ToLower(), out var uId))
							{
								medicine.AddUnit(GuidGenerator.Create(), uId, int.Parse(match.Groups[2].Value), 1);
							}
						}
					}

					// Insert
					await _medicineRepo.InsertAsync(medicine, autoSave: true);

					// Cập nhật Cache ngay lập tức để Sheet Giá tìm thấy thuốc này
					existingMedicines[codeKey] = medicine.Id;
				}
				catch (Exception ex)
				{
					errors.Add($"[Thuốc] Dòng {rowIndex}: {ex.Message}");
				}
			}

			// Sheet 2 Bảng giá chi tiết
			rowIndex = 1;
			try
			{
				var priceRows = stream.Query<MedicinePriceImportDto>("Bảng giá chi tiết").ToList();
				foreach (var row in priceRows)
				{
					rowIndex++;
					try
					{
						if (string.IsNullOrWhiteSpace(row.MedicineCode)) continue;

						if (!existingMedicines.TryGetValue(row.MedicineCode.ToUpper().Trim(), out var pId)) continue; // Thuốc chưa có -> Bỏ qua
						if (!priceLists.TryGetValue(row.PriceListName.ToLower().Trim(), out var plId)) continue;
						if (!units.TryGetValue(row.UnitName.ToLower().Trim(), out var uId)) continue;

						int minQty = row.MinQuantity > 0 ? row.MinQuantity : 1;

						// Check trùng giá
						// Nếu giá này đã có rồi thì bỏ qua, không update đè
						var existsPrice = await _productPriceRepo.AnyAsync(x =>
							x.PriceListId == plId && x.ProductId == pId &&
							x.UnitId == uId && x.MinQuantity == minQty);

						if (existsPrice)
						{
							errors.Add($"[Giá] Dòng {rowIndex}: Giá cho '{row.MedicineCode}' đã tồn tại. Bỏ qua.");
							continue;
						}

						// Insert
						var price = await _priceManager.CreatePriceAsync(plId, pId, uId, row.Price, minQty);
						await _productPriceRepo.InsertAsync(price);
					}
					catch (Exception ex)
					{
						errors.Add($"[Giá] Dòng {rowIndex} (Mã {row.MedicineCode}): {ex.Message}");
					}
				}
			}
			catch { /*Không có sheet giá thì thôi */ }

			if (errors.Any())
			{
				var errorMsg = $"Kết quả nhập liệu:\n- " + string.Join("\n- ", errors.Take(15));
				if (errors.Count > 15) errorMsg += $"\n... và {errors.Count - 15} lỗi khác.";
				throw new UserFriendlyException(errorMsg);
			}
		}

		public async Task<IRemoteStreamContent> GetImportTemplateAsync()
		{

			// Lấy danh mục từ DB
			var categories = (await _categoryRepo.GetListAsync()).Select(x => x.Name).ToList();
			var manufacturers = (await _manufacturerRepo.GetListAsync()).Select(x => x.Name).ToList();
			var units = (await _baseUnitRepo.GetListAsync()).Select(x => x.Name).ToList();
			var dosageForms = (await _dosageFormRepo.GetListAsync()).Select(x => x.Name).ToList();
			var priceLists = (await _priceListRepo.GetListAsync()).Select(x => x.Name).ToList();

			// Dữ liệu mẫu Sheet 1
			var medicineSamples = new List<MedicineImportDto>
			{
				new MedicineImportDto {
					Code = "MED_PANADOL_RWEV",
					Name = "Panadol",
					Category = "Giảm đau - Hạ sốt",
					Manufacturer = "Sterling Drug",
					BaseUnit = "Viên",
					DosageForm = "Viên nén",
					RegistrationNumber = "VD-29584-18",
					UsageRoute = "Uống",
					StorageCondition = "Bình thường",
					IsPrescriptionDrug = "Không",
					Ingredients = "Paracetamol",
					Units = "Vỉ (x12)"
				},
				new MedicineImportDto {
					Code = "MED_PANADOL_EXTRA_07HR",
					Name = "Panadol Extra",
					Category = "Giảm đau - Hạ sốt",
					Manufacturer = "Sterling Drug",
					BaseUnit = "Viên",
					DosageForm = "Viên nén",
					RegistrationNumber = "VD-21189-14",
					UsageRoute = "Uống",
					StorageCondition = "Mát",
					IsPrescriptionDrug = "Không",
					Ingredients = "Paracetamol; Caffeine",
					Units = "Vỉ (x12); Hộp (x15)"
				}
			};

			// Dữ liệu mẫu Sheet 2
			var priceSamples = new List<MedicinePriceImportDto>
			{
				new MedicinePriceImportDto { MedicineCode = "MED_PANADOL_RWEV", PriceListName = "Standard", UnitName = "Viên", Price = 1000, MinQuantity = 1 },
				new MedicinePriceImportDto { MedicineCode = "MED_PANADOL_RWEV", PriceListName = "Wholesale", UnitName = "Hộp", Price = 95000, MinQuantity = 10 },
				new MedicinePriceImportDto { MedicineCode = "MED_PANADOL_EXTRA_07HR", PriceListName = "Standard", UnitName = "Viên", Price = 2000, MinQuantity = 1 }
			};

			// Khởi tạo Excel
			var workbook = new XSSFWorkbook();
			var sheetMain = workbook.CreateSheet("Danh sách thuốc");
			var sheetPrice = workbook.CreateSheet("Bảng giá chi tiết");
			var sheetData = workbook.CreateSheet("MasterData"); // Sheet ẩn
			var headerStyle = CreateHeaderStyle(workbook);

			int maxRows = Math.Max(categories.Count, Math.Max(manufacturers.Count, Math.Max(units.Count, Math.Max(dosageForms.Count, priceLists.Count))));
			for (int i = 0; i < maxRows; i++)
			{
				var row = sheetData.CreateRow(i);
				if (i < categories.Count) row.CreateCell(0).SetCellValue(categories[i]);
				if (i < manufacturers.Count) row.CreateCell(1).SetCellValue(manufacturers[i]);
				if (i < units.Count) row.CreateCell(2).SetCellValue(units[i]);
				if (i < dosageForms.Count) row.CreateCell(3).SetCellValue(dosageForms[i]);
				if (i < priceLists.Count) row.CreateCell(4).SetCellValue(priceLists[i]);
			}

			// Tạo Named Ranges (Vùng dữ liệu có tên)
			CreateNamedRange(workbook, "ListCategories", "MasterData", 0, categories.Count);
			CreateNamedRange(workbook, "ListManufacturers", "MasterData", 1, manufacturers.Count);
			CreateNamedRange(workbook, "ListUnits", "MasterData", 2, units.Count);
			CreateNamedRange(workbook, "ListDosageForms", "MasterData", 3, dosageForms.Count);
			CreateNamedRange(workbook, "ListPriceLists", "MasterData", 4, priceLists.Count);
			CreateNamedRange(workbook, "ListMedicineCodes", "Danh sách thuốc", 0, 1000, startRow: 1);

			// Ẩn sheet dữ liệu
			workbook.SetSheetHidden(workbook.GetSheetIndex("MasterData"), true);

			// Sheet 1 Danh sách thuốc
			var headers1 = new[] { "Mã thuốc", "Tên thuốc", "Nhóm hàng", "Nhà sản xuất", "Đơn vị cơ bản", "Dạng bào chế", "Số đăng ký", "Đường dùng", "Điều kiện bảo quản", "Thuốc kê đơn", "Hoạt chất", "Đơn vị quy đổi" };
			var headerRow1 = sheetMain.CreateRow(0);

			for (int i = 0; i < headers1.Length; i++)
			{
				var cell = headerRow1.CreateCell(i);
				cell.SetCellValue(headers1[i]);
				cell.CellStyle = headerStyle;
				sheetMain.SetColumnWidth(i, 5000);
			}

			// Fill dữ liệu mẫu
			for (int i = 0; i < medicineSamples.Count; i++)
			{
				var item = medicineSamples[i];
				var row = sheetMain.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(item.Code);
				row.CreateCell(1).SetCellValue(item.Name);
				row.CreateCell(2).SetCellValue(item.Category);
				row.CreateCell(3).SetCellValue(item.Manufacturer);
				row.CreateCell(4).SetCellValue(item.BaseUnit);
				row.CreateCell(5).SetCellValue(item.DosageForm);
				row.CreateCell(6).SetCellValue(item.RegistrationNumber);
				row.CreateCell(7).SetCellValue(item.UsageRoute);
				row.CreateCell(8).SetCellValue(item.StorageCondition);
				row.CreateCell(9).SetCellValue(item.IsPrescriptionDrug);
				row.CreateCell(10).SetCellValue(item.Ingredients);
				row.CreateCell(11).SetCellValue(item.Units);
			}

			// Gán Dropdown 
			AddValidation(sheetMain, "ListCategories", 2);
			AddValidation(sheetMain, "ListManufacturers", 3);
			AddValidation(sheetMain, "ListUnits", 4);
			AddValidation(sheetMain, "ListDosageForms", 5);
			AddValidationList(sheetMain, new[] { "Uống", "Tiêm", "Ngoài da", "Khác" }, 7);
			AddValidationList(sheetMain, new[] { "Bình thường", "Mát", "Lạnh", "Đông" }, 8);
			AddValidationList(sheetMain, new[] { "Có", "Không" }, 9);

			// Sheet 2 Bảng giá chi tiết
			var headers2 = new[] { "Mã thuốc", "Bảng giá", "Đơn vị tính", "Giá bán", "SL tối thiểu" };
			var headerRow2 = sheetPrice.CreateRow(0);

			for (int i = 0; i < headers2.Length; i++)
			{
				var cell = headerRow2.CreateCell(i);
				cell.SetCellValue(headers2[i]);
				cell.CellStyle = headerStyle;
				sheetPrice.SetColumnWidth(i, 5000);
			}

			// Fill dữ liệu mẫu
			for (int i = 0; i < priceSamples.Count; i++)
			{
				var item = priceSamples[i];
				var row = sheetPrice.CreateRow(i + 1);
				row.CreateCell(0).SetCellValue(item.MedicineCode);
				row.CreateCell(1).SetCellValue(item.PriceListName);
				row.CreateCell(2).SetCellValue(item.UnitName);
				row.CreateCell(3).SetCellValue((double)item.Price);
				row.CreateCell(4).SetCellValue(item.MinQuantity);
			}

			// Gán Dropdown
			AddValidation(sheetPrice, "ListMedicineCodes", 0);
			AddValidation(sheetPrice, "ListPriceLists", 1);
			AddValidation(sheetPrice, "ListUnits", 2);

			var memoryStream = new MemoryStream();
			workbook.Write(memoryStream, leaveOpen: true);
			memoryStream.Seek(0, SeekOrigin.Begin);

			return new RemoteStreamContent(memoryStream, "Template_Import_Medicine.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
		}
		#endregion

		#region Hepler
		private ICellStyle CreateHeaderStyle(IWorkbook wb)
		{
			var style = wb.CreateCellStyle();
			var font = wb.CreateFont();
			font.Color = IndexedColors.White.Index;
			font.IsBold = true;
			style.SetFont(font);
			style.Alignment = HorizontalAlignment.Center;
			style.VerticalAlignment = VerticalAlignment.Center;
			style.FillForegroundColor = IndexedColors.DarkBlue.Index;
			style.FillPattern = FillPattern.SolidForeground;
			return style;
		}
		private void CreateNamedRange(IWorkbook wb, string name, string sheetName, int colIndex, int count, int startRow = 0)
		{
			if (count == 0 && startRow == 0) return; // Nếu danh mục rỗng
													 // Nếu count = 1000 (cho Mã thuốc) thì cứ tạo range dù chưa có data

			IName namedRange = wb.CreateName();
			namedRange.NameName = name;
			string colLetter = CellReference.ConvertNumToColString(colIndex);

			// startRow + 1 vì Excel tính row bắt đầu từ 1, code tính từ 0 (Header là row 0 -> Excel là 1)
			// Range dữ liệu thực tế bắt đầu sau Header
			namedRange.RefersToFormula = $"'{sheetName}'!${colLetter}${startRow + 2}:${colLetter}${count + 1}";
		}

		private void AddValidation(ISheet sheet, string namedRange, int colIndex)
		{
			var helper = sheet.GetDataValidationHelper();
			// Tạo constraint từ Named Range
			var constraint = helper.CreateFormulaListConstraint(namedRange);

			// Áp dụng từ dòng 1 đến dòng 1000 (Bỏ qua header dòng 0)
			var addressList = new CellRangeAddressList(1, 1000, colIndex, colIndex);
			var validation = helper.CreateValidation(constraint, addressList);

			validation.ShowErrorBox = true;
			validation.CreateErrorBox("Lỗi nhập liệu", "Vui lòng chọn giá trị từ danh sách.");
			sheet.AddValidationData(validation);
		}

		private void AddValidationList(ISheet sheet, string[] items, int colIndex)
		{
			var helper = sheet.GetDataValidationHelper();
			var constraint = helper.CreateExplicitListConstraint(items);
			var addressList = new CellRangeAddressList(1, 1000, colIndex, colIndex);
			var validation = helper.CreateValidation(constraint, addressList);

			validation.ShowErrorBox = true;
			validation.CreateErrorBox("Lỗi nhập liệu", "Vui lòng chọn giá trị từ danh sách.");
			sheet.AddValidationData(validation);
		}
		private Guid GetId(Dictionary<string, Guid> dict, string name, string err)
		{
			// Tìm trong Dict (Key đã lower + trim), nếu có trả về ID, không có ném lỗi
			return dict.TryGetValue(name?.ToLower().Trim() ?? "", out var id) ? id : throw new Exception(err);
		}

		private bool ParseBool(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return false; // Mặc định false
			var s = input.ToLower().Trim();
			return s == "có" || s == "true" || s == "1" || s == "yes" || s.Contains("hoạt động");
		}

		private UsageRoute ParseUsageRoute(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return UsageRoute.Oral; // Mặc định Uống
			var s = input.ToLower().Trim();
			if (s.Contains("tiêm")) return UsageRoute.Injection;
			if (s.Contains("ngoài")) return UsageRoute.External;
			if (s.Contains("khác")) return UsageRoute.Other;
			return UsageRoute.Oral;
		}

		private StorageCondition ParseStorage(string input)
		{
			if (string.IsNullOrWhiteSpace(input)) return StorageCondition.Normal; // Mặc định Bình thường
			var s = input.ToLower().Trim();
			if (s.Contains("mát")) return StorageCondition.Cool;
			if (s.Contains("lạnh")) return StorageCondition.Cold;
			if (s.Contains("đông")) return StorageCondition.Frozen;
			return StorageCondition.Normal;
		}
		#endregion
	}
}