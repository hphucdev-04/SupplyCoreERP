using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SupplyCoreERP.Catalog.ActiveIngredients;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Categories;
using SupplyCoreERP.Catalog.DosageForms;
using SupplyCoreERP.Catalog.Manufacturers;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Medicines.Events;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Medicines.Dtos;
using SupplyCoreERP.Sales.PriceLists;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Local;

namespace SupplyCoreERP.Medicines;

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
    private readonly ILocalEventBus _localEventBus;

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
        PriceManager priceManager,
        ILocalEventBus localEventBus)
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
        _localEventBus = localEventBus;
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
            RegistrationNumber = x.GetCurrentRegistration()?.RegistrationNumber ?? string.Empty,

            //Enum
            UsageRoute = x.UsageRoute switch
            {
                UsageRoute.Oral => "Uá»‘ng",
                UsageRoute.Injection => "TiÃªm",
                UsageRoute.External => "NgoÃ i da",
                UsageRoute.Other => "KhÃ¡c"
            },
            StorageCondition = x.StorageCondition switch
            {
                StorageCondition.Normal => "BÃ¬nh thÆ°á»ng",
                StorageCondition.Cool => "MÃ¡t",
                StorageCondition.Cold => "Láº¡nh",
                StorageCondition.Frozen => "ÄÃ´ng"
            },
            IsPrescriptionDrug = x.IsPrescriptionDrug ? "CÃ³ (Rx)" : "KhÃ´ng",

            //Status
            Status = x.Status switch
            {
                MedicineStatus.Pending => "Chá» duyá»‡t",
                MedicineStatus.Approved => "ÄÃ£ duyá»‡t",
                MedicineStatus.Rejected => "Tá»« chá»‘i",
                _ => ""
            },
            IsActive = x.IsActive ? "Hoáº¡t Ä‘á»™ng" : "Ngá»«ng",

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
            { "Danh sÃ¡ch thuá»‘c", medicineData },
            { "Báº£ng giÃ¡ chi tiáº¿t", priceData }
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

        // Cache db lÃªn RAM
        Dictionary<string, Guid> categories = (await _categoryRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
        Dictionary<string, Guid> manufacturers = (await _manufacturerRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
        Dictionary<string, Guid> units = (await _baseUnitRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
        Dictionary<string, Guid> dosages = (await _dosageFormRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
        Dictionary<string, Guid> ingredients = (await _ingredientRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);
        Dictionary<string, Guid> priceLists = (await _priceListRepo.GetListAsync()).ToDictionary(x => x.Name.ToLower().Trim(), x => x.Id);

        // Map tempCode ra medicineId
        // DÃ¹ng Ä‘á»ƒ sheet price tham chiáº¿u 
        Dictionary<string, Guid> tempCodeToMedicineId = new(StringComparer.OrdinalIgnoreCase);

        List<string> errors = new();
        List<MedicineImportedItem> importedItems = new();

        int rowIndex = 1;

        // Sheet 1 Danh sÃ¡ch thuá»‘c
        List<MedicineImportDto> medRows = stream.Query<MedicineImportDto>("Danh sÃ¡ch thuá»‘c").ToList();
        if (!medRows.Any())
        {
            medRows = stream.Query<MedicineImportDto>().ToList();
        }

        foreach (MedicineImportDto row in medRows)
        {
            rowIndex++;
            try
            {
                // ÄÃƒ XÃ“A DÃ’NG SKIP ROWINDEX == 2 á»ž ÄÃ‚Y

                // Bá» qua dÃ²ng trá»‘ng
                if (string.IsNullOrWhiteSpace(row.Name))
                {
                    continue;
                }

                // TÃ¬m ID
                Guid catId = GetId(categories, row.Category, $"DÃ²ng {rowIndex}: NhÃ³m hÃ ng '{row.Category}' khÃ´ng tá»“n táº¡i");
                Guid manuId = GetId(manufacturers, row.Manufacturer, $"DÃ²ng {rowIndex}: NSX '{row.Manufacturer}' khÃ´ng tá»“n táº¡i");
                Guid baseUnitId = GetId(units, row.BaseUnit, $"DÃ²ng {rowIndex}: ÄÆ¡n vá»‹ '{row.BaseUnit}' khÃ´ng tá»“n táº¡i");
                Guid dosageId = GetId(dosages, row.DosageForm, $"DÃ²ng {rowIndex}: Dáº¡ng bÃ o cháº¿ '{row.DosageForm}' khÃ´ng tá»“n táº¡i");

                // Manager táº¡o entity vá»›i Ä‘áº§y Ä‘á»§ thÃ´ng tin ngay tá»« Ä‘áº§u
                Medicine medicine = await _medicineManager.CreateAsync(
                    row.Name, catId, manuId, baseUnitId, dosageId,
                    row.RegistrationNumber,
                    ParseUsageRoute(row.UsageRoute),
                    ParseStorage(row.StorageCondition),
                    ParseBool(row.IsPrescriptionDrug),
                    raiseEvent: false
                );

                // Ingredients
                if (!string.IsNullOrWhiteSpace(row.Ingredients))
                {
                    foreach (string name in row.Ingredients.Split(';'))
                    {
                        if (ingredients.TryGetValue(name.Trim().ToLower(), out Guid iId))
                        {
                            medicine.AddIngredient(iId);
                        }
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
                importedItems.Add(new MedicineImportedItem(medicine.Id, medicine.Name, medicine.Code));


                // LÆ°u mapping TempCode -> Id Ä‘á»ƒ Sheet giÃ¡ dÃ¹ng
                // Náº¿u khÃ´ng cÃ³ TempCode thÃ¬ dÃ¹ng Name lÃ m key dá»± phÃ²ng
                string tempKey = !string.IsNullOrWhiteSpace(row.TempCode)
                    ? row.TempCode.Trim()
                    : row.Name.Trim();

                if (!tempCodeToMedicineId.ContainsKey(tempKey))
                {
                    tempCodeToMedicineId[tempKey] = medicine.Id;
                }
            }
            catch (Exception ex)
            {
                errors.Add($"[Thuá»‘c] DÃ²ng {rowIndex}: {ex.Message}");
            }
        }

        // Sheet 2 Báº£ng giÃ¡ chi tiáº¿t
        rowIndex = 1;
        try
        {
            List<MedicinePriceImportDto> priceRows = stream.Query<MedicinePriceImportDto>("Báº£ng giÃ¡ chi tiáº¿t").ToList();
            foreach (MedicinePriceImportDto row in priceRows)
            {
                rowIndex++;
                try
                {
                    if (string.IsNullOrWhiteSpace(row.MedicineCode))
                    {
                        continue;
                    }

                    // TÃ¬m medicine.id Ä‘Ã£ Ä‘Æ°á»£c map theo tempCode
                    if (!tempCodeToMedicineId.TryGetValue(row.MedicineCode.ToUpper().Trim(), out Guid pId))
                    {
                        continue; // Thuá»‘c chÆ°a cÃ³ -> Bá» qua
                    }

                    if (!priceLists.TryGetValue(row.PriceListName.ToLower().Trim(), out Guid plId))
                    {
                        continue;
                    }

                    if (!units.TryGetValue(row.UnitName.ToLower().Trim(), out Guid uId))
                    {
                        continue;
                    }

                    int minQty = row.MinQuantity > 0 ? row.MinQuantity : 1;

                    // Check trÃ¹ng giÃ¡
                    // Náº¿u giÃ¡ nÃ y Ä‘Ã£ cÃ³ rá»“i thÃ¬ bá» qua, khÃ´ng update Ä‘Ã¨
                    bool existsPrice = await _productPriceRepo.AnyAsync(x =>
                        x.PriceListId == plId && x.ProductId == pId &&
                        x.UnitId == uId && x.MinQuantity == minQty);

                    if (existsPrice)
                    {
                        errors.Add($"[GiÃ¡] DÃ²ng {rowIndex}: GiÃ¡ cho '{row.MedicineCode}' Ä‘Ã£ tá»“n táº¡i. Bá» qua.");
                        continue;
                    }

                    // Insert
                    ProductPrice price = await _priceManager.CreatePriceAsync(plId, pId, uId, row.Price, minQty);
                    await _productPriceRepo.InsertAsync(price);
                }
                catch (Exception ex)
                {
                    errors.Add($"[GiÃ¡] DÃ²ng {rowIndex} (MÃ£ {row.MedicineCode}): {ex.Message}");
                }
            }
        }
        catch { /*KhÃ´ng cÃ³ sheet giÃ¡ thÃ¬ thÃ´i */ }

        if (importedItems.Any())
        {
            await _localEventBus.PublishAsync(new MedicineImportDomainEvent(importedItems));
        }

        if (errors.Any())
        {
            string errorMsg = $"Káº¿t quáº£ nháº­p liá»‡u:\n- " + string.Join("\n- ", errors.Take(15));
            if (errors.Count > 15)
            {
                errorMsg += $"\n... vÃ  {errors.Count - 15} lá»—i khÃ¡c.";
            }

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
        ISheet sheetMain = workbook.CreateSheet("Danh sÃ¡ch thuá»‘c");
        ISheet sheetPrice = workbook.CreateSheet("Báº£ng giÃ¡ chi tiáº¿t");
        ISheet sheetData = workbook.CreateSheet("MasterData");
        ICellStyle headerStyle = CreateHeaderStyle(workbook);

        // MasterData
        int maxRows = new[] { categories.Count, manufacturers.Count, units.Count, dosageForms.Count, priceLists.Count }.Max();
        for (int i = 0; i < maxRows; i++)
        {
            IRow row = sheetData.CreateRow(i);
            if (i < categories.Count)
            {
                row.CreateCell(0).SetCellValue(categories[i]);
            }

            if (i < manufacturers.Count)
            {
                row.CreateCell(1).SetCellValue(manufacturers[i]);
            }

            if (i < units.Count)
            {
                row.CreateCell(2).SetCellValue(units[i]);
            }

            if (i < dosageForms.Count)
            {
                row.CreateCell(3).SetCellValue(dosageForms[i]);
            }

            if (i < priceLists.Count)
            {
                row.CreateCell(4).SetCellValue(priceLists[i]);
            }
        }

        CreateNamedRange(workbook, "ListCategories", "MasterData", 0, categories.Count, startRow: 0);
        CreateNamedRange(workbook, "ListManufacturers", "MasterData", 1, manufacturers.Count, startRow: 0);
        CreateNamedRange(workbook, "ListUnits", "MasterData", 2, units.Count, startRow: 0);
        CreateNamedRange(workbook, "ListDosageForms", "MasterData", 3, dosageForms.Count, startRow: 0);
        CreateNamedRange(workbook, "ListPriceLists", "MasterData", 4, priceLists.Count, startRow: 0);

        // Sá»­a startRow thÃ nh 0 Ä‘á»ƒ ListTempCodes map Ä‘Ãºng tá»« dÃ²ng 2 cá»§a Excel
        CreateNamedRange(workbook, "ListTempCodes", "Danh sÃ¡ch thuá»‘c", 0, 1000, startRow: 1);

        workbook.SetSheetHidden(workbook.GetSheetIndex("MasterData"), true);

        // Sheet 1
        string[] headers1 = new[]
        {
            "MÃ£ táº¡m",           // 0
            "TÃªn thuá»‘c",        // 1
            "NhÃ³m hÃ ng",        // 2
            "NhÃ  sáº£n xuáº¥t",     // 3
            "ÄÆ¡n vá»‹ cÆ¡ báº£n",   // 4
            "Dáº¡ng bÃ o cháº¿",     // 5
            "Sá»‘ Ä‘Äƒng kÃ½",       // 6
            "ÄÆ°á»ng dÃ¹ng",       // 7
            "Äiá»u kiá»‡n báº£o quáº£n", // 8
            "Thuá»‘c kÃª Ä‘Æ¡n",     // 9
            "Hoáº¡t cháº¥t",        // 10
            "ÄÆ¡n vá»‹ quy Ä‘á»•i"   // 11
        };

        // Tooltip hover vÃ o header Ä‘á»ƒ biáº¿t cÃ¡ch Ä‘iá»n
        string[] tooltips1 = new[]
        {
            "MÃ£ táº¡m do báº¡n tá»± Ä‘áº·t, dÃ¹ng Ä‘á»ƒ ghÃ©p vá»›i Sheet 'Báº£ng giÃ¡'.\nVD: MED001, PANADOL_1",
            "TÃªn thuá»‘c. Báº¯t buá»™c Ä‘iá»n.",
            "Chá»n tá»« danh sÃ¡ch dropdown.",
            "Chá»n tá»« danh sÃ¡ch dropdown.",
            "Chá»n tá»« danh sÃ¡ch dropdown.",
            "Chá»n tá»« danh sÃ¡ch dropdown.",
            "Sá»‘ Ä‘Äƒng kÃ½ lÆ°u hÃ nh. TÃ¹y chá»n.\nVD: VD-12345-21",
            "Chá»n tá»« danh sÃ¡ch:\nUá»‘ng / TiÃªm / NgoÃ i da / KhÃ¡c",
            "Chá»n tá»« danh sÃ¡ch:\nBÃ¬nh thÆ°á»ng / MÃ¡t / Láº¡nh / ÄÃ´ng",
            "Chá»n tá»« danh sÃ¡ch:\nCÃ³ / KhÃ´ng",
            "Nhiá»u hoáº¡t cháº¥t cÃ¡ch nhau báº±ng dáº¥u ;\nVD: Paracetamol; Caffeine",
            "Nhiá»u Ä‘Æ¡n vá»‹ cÃ¡ch nhau báº±ng dáº¥u ;\nVD: Vá»‰ (x10); Há»™p (x100)\n LÆ°u Ã½: TÃªn Ä‘Æ¡n vá»‹ pháº£i cÃ³ trong Ä‘Æ¡n vá»‹ cÆ¡ báº£n"
        };

        IRow headerRow1 = sheetMain.CreateRow(0);

        for (int i = 0; i < headers1.Length; i++)
        {
            ICell cell = headerRow1.CreateCell(i);
            cell.SetCellValue(headers1[i]);
            cell.CellStyle = headerStyle;
            sheetMain.SetColumnWidth(i, 6500);

            // Tooltip trÃªn header
            AddCellComment(sheetMain, cell, tooltips1[i]);
        }

        // Sá»­a startRow vá» 1 Ä‘á»ƒ Äƒn khá»›p vá»›i dÃ²ng ngay dÆ°á»›i header
        AddValidationFromRow(sheetMain, "ListCategories", 2, 1);
        AddValidationFromRow(sheetMain, "ListManufacturers", 3, 1);
        AddValidationFromRow(sheetMain, "ListUnits", 4, 1);
        AddValidationFromRow(sheetMain, "ListDosageForms", 5, 1);
        AddValidationListFromRow(sheetMain, new[] { "Uá»‘ng", "TiÃªm", "NgoÃ i da", "KhÃ¡c" }, 7, 1);
        AddValidationListFromRow(sheetMain, new[] { "BÃ¬nh thÆ°á»ng", "MÃ¡t", "Láº¡nh", "ÄÃ´ng" }, 8, 1);
        AddValidationListFromRow(sheetMain, new[] { "CÃ³", "KhÃ´ng" }, 9, 1);

        // Sheet 2
        string[] headers2 = new[]
        {
            "MÃ£ táº¡m",       // 0
            "Báº£ng giÃ¡",     // 1
            "ÄÆ¡n vá»‹ tÃ­nh",  // 2
            "GiÃ¡ bÃ¡n",      // 3
            "SL tá»‘i thiá»ƒu"  // 4
        };

        string[] tooltips2 = new[]
        {
            "Äiá»n MÃ£ táº¡m giá»‘ng Sheet 'Danh sÃ¡ch thuá»‘c'.\nChá»n tá»« dropdown Ä‘á»ƒ trÃ¡nh nháº­p sai.",
            "Chá»n tá»« danh sÃ¡ch dropdown.",
            "Chá»n tá»« danh sÃ¡ch dropdown.",
            "GiÃ¡ bÃ¡n, nháº­p sá»‘.\nVD: 5000",
            "Sá»‘ lÆ°á»£ng tá»‘i thiá»ƒu Ã¡p dá»¥ng giÃ¡ nÃ y.\nVD: 1"
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

        // Sá»­a startRow vá» 1 
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
        // Náº¿u lÃ  cá»™t dá»¯ liá»‡u Ä‘á»™ng (nhÆ° ListTempCodes) thÃ¬ cho phÃ©p range dÃ i
        // Náº¿u lÃ  MasterData thÃ¬ pháº£i cÃ³ Ã­t nháº¥t 1 pháº§n tá»­
        if (count <= 0 && startRow == 0 && name != "ListTempCodes")
        {
            return;
        }

        IName namedRange = wb.CreateName();
        namedRange.NameName = name;
        string colLetter = CellReference.ConvertNumToColString(colIndex);

        // Excel dÃ²ng báº¯t Ä‘áº§u tá»« 1. 
        // Header á»Ÿ row 0 -> Data báº¯t Ä‘áº§u tá»« row 1 (Excel gá»i lÃ  dÃ²ng 2)
        int excelStartRow = startRow + 1;
        int excelEndRow = startRow + (count > 0 ? count : 1000); // Náº¿u count=0 thÃ¬ máº·c Ä‘á»‹nh 1000 dÃ²ng cho cá»™t MÃ£ táº¡m

        namedRange.RefersToFormula = $"'{sheetName}'!${colLetter}${excelStartRow}:${colLetter}${excelEndRow}";
    }

    private void AddValidation(ISheet sheet, string namedRange, int colIndex)
    {
        IDataValidationHelper helper = sheet.GetDataValidationHelper();
        // Táº¡o constraint tá»« Named Range
        IDataValidationConstraint constraint = helper.CreateFormulaListConstraint(namedRange);

        // Ãp dá»¥ng tá»« dÃ²ng 1 Ä‘áº¿n dÃ²ng 1000 (Bá» qua header dÃ²ng 0)
        CellRangeAddressList addressList = new(1, 1000, colIndex, colIndex);
        IDataValidation validation = helper.CreateValidation(constraint, addressList);

        validation.ShowErrorBox = true;
        validation.CreateErrorBox("Lá»—i nháº­p liá»‡u", "Vui lÃ²ng chá»n giÃ¡ trá»‹ tá»« danh sÃ¡ch.");
        sheet.AddValidationData(validation);
    }

    private void AddValidationList(ISheet sheet, string[] items, int colIndex)
    {
        IDataValidationHelper helper = sheet.GetDataValidationHelper();
        IDataValidationConstraint constraint = helper.CreateExplicitListConstraint(items);
        CellRangeAddressList addressList = new(1, 1000, colIndex, colIndex);
        IDataValidation validation = helper.CreateValidation(constraint, addressList);

        validation.ShowErrorBox = true;
        validation.CreateErrorBox("Lá»—i nháº­p liá»‡u", "Vui lÃ²ng chá»n giÃ¡ trá»‹ tá»« danh sÃ¡ch.");
        sheet.AddValidationData(validation);
    }
    private Guid GetId(Dictionary<string, Guid> dict, string name, string err)
    {
        // TÃ¬m trong Dict (Key Ä‘Ã£ lower + trim), náº¿u cÃ³ tráº£ vá» ID, khÃ´ng cÃ³ nÃ©m lá»—i
        return dict.TryGetValue(name?.ToLower().Trim() ?? "", out Guid id) ? id : throw new Exception(err);
    }

    private bool ParseBool(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false; // Máº·c Ä‘á»‹nh false
        }

        string s = input.ToLower().Trim();
        return s == "cÃ³" || s == "true" || s == "1" || s == "yes" || s.Contains("hoáº¡t Ä‘á»™ng");
    }

    private UsageRoute ParseUsageRoute(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return UsageRoute.Oral; // Máº·c Ä‘á»‹nh Uá»‘ng
        }

        string s = input.ToLower().Trim();
        if (s.Contains("tiÃªm"))
        {
            return UsageRoute.Injection;
        }

        if (s.Contains("ngoÃ i"))
        {
            return UsageRoute.External;
        }

        if (s.Contains("khÃ¡c"))
        {
            return UsageRoute.Other;
        }

        return UsageRoute.Oral;
    }

    private StorageCondition ParseStorage(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return StorageCondition.Normal; // Máº·c Ä‘á»‹nh BÃ¬nh thÆ°á»ng
        }

        string s = input.ToLower().Trim();
        if (s.Contains("mÃ¡t"))
        {
            return StorageCondition.Cool;
        }

        if (s.Contains("láº¡nh"))
        {
            return StorageCondition.Cold;
        }

        if (s.Contains("Ä‘Ã´ng"))
        {
            return StorageCondition.Frozen;
        }

        return StorageCondition.Normal;
    }


    // ThÃªm tooltip (comment) vÃ o 1 cell
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
        comment.Author = "HÆ°á»›ng dáº«n";
        comment.Visible = false; // chá»‰ hiá»‡n khi hover
        cell.CellComment = comment;
    }

    // Validation tá»« row chá»‰ Ä‘á»‹nh (Ä‘á»ƒ bá» qua dÃ²ng gá»£i Ã½ á»Ÿ row 1)
    private void AddValidationFromRow(ISheet sheet, string namedRange, int colIndex, int startRow = 1)
    {
        IDataValidationHelper helper = sheet.GetDataValidationHelper();
        IDataValidationConstraint constraint = helper.CreateFormulaListConstraint(namedRange);
        CellRangeAddressList addressList = new(startRow, 1000, colIndex, colIndex);
        IDataValidation validation = helper.CreateValidation(constraint, addressList);
        validation.ShowErrorBox = true;
        validation.CreateErrorBox("Lá»—i nháº­p liá»‡u", "Vui lÃ²ng chá»n giÃ¡ trá»‹ tá»« danh sÃ¡ch.");
        sheet.AddValidationData(validation);
    }

    private void AddValidationListFromRow(ISheet sheet, string[] items, int colIndex, int startRow = 1)
    {
        IDataValidationHelper helper = sheet.GetDataValidationHelper();
        IDataValidationConstraint constraint = helper.CreateExplicitListConstraint(items);
        CellRangeAddressList addressList = new(startRow, 1000, colIndex, colIndex);
        IDataValidation validation = helper.CreateValidation(constraint, addressList);
        validation.ShowErrorBox = true;
        validation.CreateErrorBox("Lá»—i nháº­p liá»‡u", "Vui lÃ²ng chá»n giÃ¡ trá»‹ tá»« danh sÃ¡ch.");
        sheet.AddValidationData(validation);
    }
    #endregion
}

