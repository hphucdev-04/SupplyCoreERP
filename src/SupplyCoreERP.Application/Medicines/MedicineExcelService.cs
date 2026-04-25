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
using SupplyCoreERP.Prices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Medicines
{
    public class MedicineExcelService : SupplyCore
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

        public MedicineExcelService(
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

        #region Export Excel
        public async Task<IRemoteStreamContent> GetListAsExcelFileAsync(GetMedicineListDto input)
        {
            IQueryable<Medicine> query = await _medicineRepo.GetQueryableAsync();

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

            List<Medicine> items = await AsyncExecuter.ToListAsync(query);

            //Map Sheet 1
            IEnumerable<MedicineExportDto> medicineData = items.Select(x => new MedicineExportDto
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

            List<Guid> medicineIds = items.Select(x => x.Id).ToList();

            IQueryable<ProductPrice> priceQuery = await _productPriceRepo.GetQueryableAsync();

            List<ProductPrice> prices = await priceQuery
                .Include(x => x.PriceList)
                .Include(x => x.Unit)
                .Include(x => x.Product)
                .Where(x => medicineIds.Contains(x.ProductId))
                .OrderBy(x => x.Product.Name)
                .ThenBy(x => x.PriceList.Code)
                .ToListAsync();

            //Map Sheet 2
            IEnumerable<MedicinePriceExportDto> priceData = prices.Select(x => new MedicinePriceExportDto
            {
                MedicineCode = x.Product?.Code,
                MedicineName = x.Product?.Name,
                PriceListName = x.PriceList?.Name,
                UnitName = x.Unit?.Name,
                Price = x.Price,
                MinQuantity = x.MinQuantity,
                Currency = x.PriceList?.Currency.ToString()
            });

            MemoryStream memoryStream = new();
            Dictionary<string, object> sheets = new()
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

        #region Import
        public async Task ImportExcelAsync(IRemoteStreamContent file)
        {
            using Stream stream = file.GetStream();

            // Cache db lên RAM
            Dictionary<string, Guid> categories = (await _categoryRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
            Dictionary<string, Guid> manufacturers = (await _manufacturerRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
            Dictionary<string, Guid> units = (await _baseUnitRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
            Dictionary<string, Guid> dosages = (await _dosageFormRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
            Dictionary<string, Guid> ingredients = (await _ingredientRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
            Dictionary<string, Guid> priceLists = (await _priceListRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);

            // Map tempCode ra medicineId
            // Dùng để sheet price tham chiếu 
            Dictionary<string, Guid> tempCodeToMedicineId = new(StringComparer.OrdinalIgnoreCase);

            List<string> errors = new();
            int rowIndex = 1;

            // Sheet 1 Danh sách thuốc
            List<MedicineImportDto> medRows = stream.Query<MedicineImportDto>("Danh sách thuốc").ToList();
            if (!medRows.Any()) medRows = stream.Query<MedicineImportDto>().ToList();

            foreach (MedicineImportDto? row in medRows)
            {
                rowIndex++;
                try
                {
                    // ĐÃ XÓA DÒNG SKIP ROWINDEX == 2 Ở ĐÂY

                    // Bỏ qua dòng trống
                    if (string.IsNullOrWhiteSpace(row.Name)) continue;

                    // Tìm ID
                    Guid catId = GetId(categories, row.Category, $"Dòng {rowIndex}: Nhóm hàng '{row.Category}' không tồn tại");
                    Guid manuId = GetId(manufacturers, row.Manufacturer, $"Dòng {rowIndex}: NSX '{row.Manufacturer}' không tồn tại");
                    Guid baseUnitId = GetId(units, row.BaseUnit, $"Dòng {rowIndex}: Đơn vị '{row.BaseUnit}' không tồn tại");
                    Guid dosageId = GetId(dosages, row.DosageForm, $"Dòng {rowIndex}: Dạng bào chế '{row.DosageForm}' không tồn tại");

                    // Manager tạo entity với đầy đủ thông tin ngay từ đầu
                    Medicine medicine = await _medicineManager.CreateAsync(
                        row.Name, catId, manuId, baseUnitId, dosageId,
                        row.RegistrationNumber,
                        ParseUsageRoute(row.UsageRoute),
                        ParseStorage(row.StorageCondition),
                        ParseBool(row.IsPrescriptionDrug)
                    );

                    medicine.SetStatus(MedicineStatus.Pending);

                    // Ingredients
                    if (!string.IsNullOrWhiteSpace(row.Ingredients))
                    {
                        foreach (string name in row.Ingredients.Split(';'))
                        {
                            if (ingredients.TryGetValue(name.Trim().ToLower(), out Guid iId)) medicine.AddIngredient(iId);
                        }
                    }

                    // Units
                    if (!string.IsNullOrWhiteSpace(row.Units))
                    {
                        foreach (string item in row.Units.Split(';'))
                        {
                            Match match = Regex.Match(item.Trim(), @"^(.*?)\s*\(x(\d+)\)$");
                            if (match.Success && units.TryGetValue(match.Groups[1].Value.Trim().ToLower(), out Guid uId))
                            {
                                medicine.AddUnit(GuidGenerator.Create(), uId, int.Parse(match.Groups[2].Value), 1);
                            }
                        }
                    }

                    // Insert
                    await _medicineRepo.InsertAsync(medicine, autoSave: true);

                    // Lưu mapping TempCode -> Id để Sheet giá dùng
                    // Nếu không có TempCode thì dùng Name làm key dự phòng
                    string tempKey = !string.IsNullOrWhiteSpace(row.TempCode)
                        ? row.TempCode.Trim()
                        : row.Name.Trim();

                    if (!tempCodeToMedicineId.ContainsKey(tempKey))
                        tempCodeToMedicineId[tempKey] = medicine.Id;
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
                List<MedicinePriceImportDto> priceRows = stream.Query<MedicinePriceImportDto>("Bảng giá chi tiết").ToList();
                foreach (MedicinePriceImportDto? row in priceRows)
                {
                    rowIndex++;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(row.MedicineCode)) continue;

                        // Tìm medicine.id đã được map theo tempCode
                        if (!tempCodeToMedicineId.TryGetValue(row.MedicineCode.ToUpper().Trim(), out Guid pId)) continue; // Thuốc chưa có -> Bỏ qua
                        if (!priceLists.TryGetValue(row.PriceListName.ToLower().Trim(), out Guid plId)) continue;
                        if (!units.TryGetValue(row.UnitName.ToLower().Trim(), out Guid uId)) continue;

                        int minQty = row.MinQuantity > 0 ? row.MinQuantity : 1;

                        // Check trùng giá
                        // Nếu giá này đã có rồi thì bỏ qua, không update đè
                        bool existsPrice = await _productPriceRepo.AnyAsync(x =>
                            x.PriceListId == plId && x.ProductId == pId &&
                            x.UnitId == uId && x.MinQuantity == minQty);

                        if (existsPrice)
                        {
                            errors.Add($"[Giá] Dòng {rowIndex}: Giá cho '{row.MedicineCode}' đã tồn tại. Bỏ qua.");
                            continue;
                        }

                        // Insert
                        ProductPrice price = await _priceManager.CreatePriceAsync(plId, pId, uId, row.Price, minQty);
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
                string errorMsg = $"Kết quả nhập liệu:\n- " + string.Join("\n- ", errors.Take(15));
                if (errors.Count > 15) errorMsg += $"\n... và {errors.Count - 15} lỗi khác.";
                throw new UserFriendlyException(errorMsg);
            }
        }
        #endregion

        #region Template
        public async Task<IRemoteStreamContent> GetImportTemplateAsync()
        {
            List<string> categories = (await _categoryRepo.GetListAsync()).Select(x => x.Name).ToList();
            List<string> manufacturers = (await _manufacturerRepo.GetListAsync()).Select(x => x.Name).ToList();
            List<string> units = (await _baseUnitRepo.GetListAsync()).Select(x => x.Name).ToList();
            List<string> dosageForms = (await _dosageFormRepo.GetListAsync()).Select(x => x.Name).ToList();
            List<string> priceLists = (await _priceListRepo.GetListAsync()).Select(x => x.Name).ToList();

            XSSFWorkbook workbook = new();
            ISheet sheetMain = workbook.CreateSheet("Danh sách thuốc");
            ISheet sheetPrice = workbook.CreateSheet("Bảng giá chi tiết");
            ISheet sheetData = workbook.CreateSheet("MasterData");
            ICellStyle headerStyle = CreateHeaderStyle(workbook);

            // MasterData
            int maxRows = new[] { categories.Count, manufacturers.Count, units.Count, dosageForms.Count, priceLists.Count }.Max();
            for (int i = 0; i < maxRows; i++)
            {
                IRow row = sheetData.CreateRow(i);
                if (i < categories.Count) row.CreateCell(0).SetCellValue(categories[i]);
                if (i < manufacturers.Count) row.CreateCell(1).SetCellValue(manufacturers[i]);
                if (i < units.Count) row.CreateCell(2).SetCellValue(units[i]);
                if (i < dosageForms.Count) row.CreateCell(3).SetCellValue(dosageForms[i]);
                if (i < priceLists.Count) row.CreateCell(4).SetCellValue(priceLists[i]);
            }

            CreateNamedRange(workbook, "ListCategories", "MasterData", 0, categories.Count, startRow: 0);
            CreateNamedRange(workbook, "ListManufacturers", "MasterData", 1, manufacturers.Count, startRow: 0);
            CreateNamedRange(workbook, "ListUnits", "MasterData", 2, units.Count, startRow: 0);
            CreateNamedRange(workbook, "ListDosageForms", "MasterData", 3, dosageForms.Count, startRow: 0);
            CreateNamedRange(workbook, "ListPriceLists", "MasterData", 4, priceLists.Count, startRow: 0);

            // Sửa startRow thành 0 để ListTempCodes map đúng từ dòng 2 của Excel
            CreateNamedRange(workbook, "ListTempCodes", "Danh sách thuốc", 0, 1000, startRow: 1);

            workbook.SetSheetHidden(workbook.GetSheetIndex("MasterData"), true);

            // Sheet 1
            string[] headers1 = new[]
            {
                "Mã tạm",           // 0
                "Tên thuốc",        // 1
                "Nhóm hàng",        // 2
                "Nhà sản xuất",     // 3
                "Đơn vị cơ bản",   // 4
                "Dạng bào chế",     // 5
                "Số đăng ký",       // 6
                "Đường dùng",       // 7
                "Điều kiện bảo quản", // 8
                "Thuốc kê đơn",     // 9
                "Hoạt chất",        // 10
                "Đơn vị quy đổi"   // 11
            };

            // Tooltip hover vào header để biết cách điền
            string[] tooltips1 = new[]
            {
                "Mã tạm do bạn tự đặt, dùng để ghép với Sheet 'Bảng giá'.\nVD: MED001, PANADOL_1",
                "Tên thuốc. Bắt buộc điền.",
                "Chọn từ danh sách dropdown.",
                "Chọn từ danh sách dropdown.",
                "Chọn từ danh sách dropdown.",
                "Chọn từ danh sách dropdown.",
                "Số đăng ký lưu hành. Tùy chọn.\nVD: VD-12345-21",
                "Chọn từ danh sách:\nUống / Tiêm / Ngoài da / Khác",
                "Chọn từ danh sách:\nBình thường / Mát / Lạnh / Đông",
                "Chọn từ danh sách:\nCó / Không",
                "Nhiều hoạt chất cách nhau bằng dấu ;\nVD: Paracetamol; Caffeine",
                "Nhiều đơn vị cách nhau bằng dấu ;\nVD: Vỉ (x10); Hộp (x100)\n Lưu ý: Tên đơn vị phải có trong đơn vị cơ bản"
            };

            IRow headerRow1 = sheetMain.CreateRow(0);

            for (int i = 0; i < headers1.Length; i++)
            {
                ICell cell = headerRow1.CreateCell(i);
                cell.SetCellValue(headers1[i]);
                cell.CellStyle = headerStyle;
                sheetMain.SetColumnWidth(i, 6500);

                // Tooltip trên header
                AddCellComment(sheetMain, cell, tooltips1[i]);
            }

            // Sửa startRow về 1 để ăn khớp với dòng ngay dưới header
            AddValidationFromRow(sheetMain, "ListCategories", 2, 1);
            AddValidationFromRow(sheetMain, "ListManufacturers", 3, 1);
            AddValidationFromRow(sheetMain, "ListUnits", 4, 1);
            AddValidationFromRow(sheetMain, "ListDosageForms", 5, 1);
            AddValidationListFromRow(sheetMain, new[] { "Uống", "Tiêm", "Ngoài da", "Khác" }, 7, 1);
            AddValidationListFromRow(sheetMain, new[] { "Bình thường", "Mát", "Lạnh", "Đông" }, 8, 1);
            AddValidationListFromRow(sheetMain, new[] { "Có", "Không" }, 9, 1);

            // Sheet 2
            string[] headers2 = new[]
            {
                "Mã tạm",       // 0
                "Bảng giá",     // 1
                "Đơn vị tính",  // 2
                "Giá bán",      // 3
                "SL tối thiểu"  // 4
            };

            string[] tooltips2 = new[]
            {
                "Điền Mã tạm giống Sheet 'Danh sách thuốc'.\nChọn từ dropdown để tránh nhập sai.",
                "Chọn từ danh sách dropdown.",
                "Chọn từ danh sách dropdown.",
                "Giá bán, nhập số.\nVD: 5000",
                "Số lượng tối thiểu áp dụng giá này.\nVD: 1"
            };

            IRow headerRow2 = sheetPrice.CreateRow(0);

            for (int i = 0; i < headers2.Length; i++)
            {
                ICell cell = headerRow2.CreateCell(i);
                cell.SetCellValue(headers2[i]);
                cell.CellStyle = headerStyle;
                sheetPrice.SetColumnWidth(i, 6500);

                AddCellComment(sheetPrice, cell, tooltips2[i]);
            }

            // Sửa startRow về 1 
            AddValidationFromRow(sheetPrice, "ListTempCodes", 0, 1);
            AddValidationFromRow(sheetPrice, "ListPriceLists", 1, 1);
            AddValidationFromRow(sheetPrice, "ListUnits", 2, 1);

            MemoryStream memoryStream = new();
            workbook.Write(memoryStream, leaveOpen: true);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return new RemoteStreamContent(
                memoryStream,
                "Template_Import_Medicine.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        #endregion

        #region Hepler
        private ICellStyle CreateHeaderStyle(IWorkbook wb)
        {
            ICellStyle style = wb.CreateCellStyle();
            IFont font = wb.CreateFont();
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
            // Nếu là cột dữ liệu động (như ListTempCodes) thì cho phép range dài
            // Nếu là MasterData thì phải có ít nhất 1 phần tử
            if (count <= 0 && startRow == 0 && name != "ListTempCodes") return;

            IName namedRange = wb.CreateName();
            namedRange.NameName = name;
            string colLetter = CellReference.ConvertNumToColString(colIndex);

            // Excel dòng bắt đầu từ 1. 
            // Header ở row 0 -> Data bắt đầu từ row 1 (Excel gọi là dòng 2)
            int excelStartRow = startRow + 1;
            int excelEndRow = startRow + (count > 0 ? count : 1000); // Nếu count=0 thì mặc định 1000 dòng cho cột Mã tạm

            namedRange.RefersToFormula = $"'{sheetName}'!${colLetter}${excelStartRow}:${colLetter}${excelEndRow}";
        }

        private void AddValidation(ISheet sheet, string namedRange, int colIndex)
        {
            IDataValidationHelper helper = sheet.GetDataValidationHelper();
            // Tạo constraint từ Named Range
            IDataValidationConstraint constraint = helper.CreateFormulaListConstraint(namedRange);

            // Áp dụng từ dòng 1 đến dòng 1000 (Bỏ qua header dòng 0)
            CellRangeAddressList addressList = new(1, 1000, colIndex, colIndex);
            IDataValidation validation = helper.CreateValidation(constraint, addressList);

            validation.ShowErrorBox = true;
            validation.CreateErrorBox("Lỗi nhập liệu", "Vui lòng chọn giá trị từ danh sách.");
            sheet.AddValidationData(validation);
        }

        private void AddValidationList(ISheet sheet, string[] items, int colIndex)
        {
            IDataValidationHelper helper = sheet.GetDataValidationHelper();
            IDataValidationConstraint constraint = helper.CreateExplicitListConstraint(items);
            CellRangeAddressList addressList = new(1, 1000, colIndex, colIndex);
            IDataValidation validation = helper.CreateValidation(constraint, addressList);

            validation.ShowErrorBox = true;
            validation.CreateErrorBox("Lỗi nhập liệu", "Vui lòng chọn giá trị từ danh sách.");
            sheet.AddValidationData(validation);
        }
        private Guid GetId(Dictionary<string, Guid> dict, string name, string err)
        {
            // Tìm trong Dict (Key đã lower + trim), nếu có trả về ID, không có ném lỗi
            return dict.TryGetValue(name?.ToLower().Trim() ?? "", out Guid id) ? id : throw new Exception(err);
        }

        private bool ParseBool(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false; // Mặc định false
            string s = input.ToLower().Trim();
            return s == "có" || s == "true" || s == "1" || s == "yes" || s.Contains("hoạt động");
        }

        private UsageRoute ParseUsageRoute(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return UsageRoute.Oral; // Mặc định Uống
            string s = input.ToLower().Trim();
            if (s.Contains("tiêm")) return UsageRoute.Injection;
            if (s.Contains("ngoài")) return UsageRoute.External;
            if (s.Contains("khác")) return UsageRoute.Other;
            return UsageRoute.Oral;
        }

        private StorageCondition ParseStorage(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return StorageCondition.Normal; // Mặc định Bình thường
            string s = input.ToLower().Trim();
            if (s.Contains("mát")) return StorageCondition.Cool;
            if (s.Contains("lạnh")) return StorageCondition.Cold;
            if (s.Contains("đông")) return StorageCondition.Frozen;
            return StorageCondition.Normal;
        }


        // Thêm tooltip (comment) vào 1 cell
        private void AddCellComment(ISheet sheet, ICell cell, string commentText)
        {
            IClientAnchor anchor = sheet.Workbook.GetCreationHelper().CreateClientAnchor();
            anchor.Col1 = cell.ColumnIndex;
            anchor.Col2 = cell.ColumnIndex + 3;
            anchor.Row1 = 0;
            anchor.Row2 = 5;

            IDrawing<IShape> drawing = sheet.CreateDrawingPatriarch();
            IComment comment = drawing.CreateCellComment(anchor);
            comment.String = new XSSFRichTextString(commentText);
            comment.Author = "Hướng dẫn";
            comment.Visible = false; // chỉ hiện khi hover
            cell.CellComment = comment;
        }

        // Validation từ row chỉ định (để bỏ qua dòng gợi ý ở row 1)
        private void AddValidationFromRow(ISheet sheet, string namedRange, int colIndex, int startRow = 1)
        {
            IDataValidationHelper helper = sheet.GetDataValidationHelper();
            IDataValidationConstraint constraint = helper.CreateFormulaListConstraint(namedRange);
            CellRangeAddressList addressList = new(startRow, 1000, colIndex, colIndex);
            IDataValidation validation = helper.CreateValidation(constraint, addressList);
            validation.ShowErrorBox = true;
            validation.CreateErrorBox("Lỗi nhập liệu", "Vui lòng chọn giá trị từ danh sách.");
            sheet.AddValidationData(validation);
        }

        private void AddValidationListFromRow(ISheet sheet, string[] items, int colIndex, int startRow = 1)
        {
            IDataValidationHelper helper = sheet.GetDataValidationHelper();
            IDataValidationConstraint constraint = helper.CreateExplicitListConstraint(items);
            CellRangeAddressList addressList = new(startRow, 1000, colIndex, colIndex);
            IDataValidation validation = helper.CreateValidation(constraint, addressList);
            validation.ShowErrorBox = true;
            validation.CreateErrorBox("Lỗi nhập liệu", "Vui lòng chọn giá trị từ danh sách.");
            sheet.AddValidationData(validation);
        }
        #endregion
    }
}
