using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.ActiveIngredients;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Categories;
using SupplyCoreERP.Catalog.DosageForms;
using SupplyCoreERP.Catalog.Manufacturers;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Locations.Countries;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Catalog.Medicines;

public class MedicineManager : DomainService, IMedicineManager
{
    // Dependencies
    private readonly ProductManager _productManager;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Manufacturer, Guid> _manufacturerRepository;
    private readonly IRepository<BaseUnit, Guid> _unitRepository;
    private readonly IRepository<DosageForm, Guid> _dosageFormRepository;
    private readonly IRepository<ActiveIngredient, Guid> _activeIngredientRepository;
    private readonly IDocumentSequenceManager _documentSequenceManager;

    // Constructor injection
    public MedicineManager(
        ProductManager productManager,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Manufacturer, Guid> manufacturerRepository,
        IRepository<BaseUnit, Guid> unitRepository,
        IRepository<DosageForm, Guid> dosageFormRepository,
        IRepository<ActiveIngredient, Guid> activeIngredientRepository,
        IRepository<Country, Guid> countryRepository,
        IDocumentSequenceManager documentSequenceManager
        )
    {
        _productManager = productManager;
        _categoryRepository = categoryRepository;
        _manufacturerRepository = manufacturerRepository;
        _unitRepository = unitRepository;
        _dosageFormRepository = dosageFormRepository;
        _activeIngredientRepository = activeIngredientRepository;
        _documentSequenceManager = documentSequenceManager;
    }

    #region Medicine
    public virtual async Task<Medicine> CreateAsync(
        string name,
        Guid categoryId,
        Guid manufacturerId,
        Guid baseUnitId,
        Guid dosageFormId,
        string regNumber,
        UsageRoute usageRoute,
        StorageCondition storageCondition,
        bool isPrescriptionDrug,
        DateTime? regValidFrom = null,
        DateTime? regValidTo = null,
        string? regNote = null,
        bool raiseEvent = true)
    {
        await ValidateForeignKeysAsync(categoryId, manufacturerId, baseUnitId, dosageFormId);

        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeMedicine);
        await _productManager.CheckCodeAsync(code);

        Medicine medicine = new(
            GuidGenerator.Create(),
            categoryId,
            manufacturerId,
            code,
            name,
            baseUnitId,
            dosageFormId,
            regNumber,
            usageRoute,
            storageCondition,
            isPrescriptionDrug
        );

        MedicineRegistration? firstReg = medicine.GetCurrentRegistration();
        if (firstReg != null)
        {
            firstReg.UpdateValidity(regValidFrom, regValidTo);
            firstReg.SetNote(regNote);
        }

        if (raiseEvent)
        {
            medicine.RaiseCreatedEvent();
        }

        return medicine;
    }

    public virtual async Task UpdateAsync(
        Medicine medicine,
        string name,
        Guid categoryId,
        Guid manufacturerId,
        Guid baseUnitId,
        Guid dosageFormId,
        string regNumber,
        UsageRoute usageRoute,
        StorageCondition storageCondition,
        bool isPrescriptionDrug,
        DateTime? regValidFrom = null,
        DateTime? regValidTo = null,
        string? regNote = null)
    {
        Check.NotNull(medicine, nameof(medicine));

        // Validate foreign keys to ensure data integrity before making any changes
        await ValidateForeignKeysAsync(categoryId, manufacturerId, baseUnitId, dosageFormId);

        // Kích hoạt kiểm tra BaseUnit qua ProductManager
        await _productManager.ValidateBaseUnitChangeAsync(medicine, baseUnitId);

        medicine.UpdateInfo(name, categoryId, manufacturerId, baseUnitId);

        // Kiểm tra nếu số đăng ký thay đổi thì thêm bản ghi mới
        MedicineRegistration? currentReg = medicine.GetCurrentRegistration();
        if (currentReg == null || currentReg.RegistrationNumber != regNumber.Trim().ToUpper())
        {
            medicine.AddRegistration(GuidGenerator.Create(), regNumber, regValidFrom, regValidTo, regNote);
        }
        else
        {
            // Thông tin đăng ký không thay đổi nhưng có thể có cập nhật về thời hạn hoặc ghi chú, nên vẫn cập nhật
            currentReg.UpdateValidity(regValidFrom, regValidTo);
            currentReg.SetNote(regNote);
        }

        // Update toàn bộ thông tin pharma từ input thực sự,
        medicine.UpdatePharmaInfo(dosageFormId, usageRoute, storageCondition, isPrescriptionDrug);

        medicine.SetPending();
    }
    #endregion

    #region Ingredient
    public virtual async Task AddIngredientAsync(Medicine medicine, Guid activeIngredientId)
    {
        Check.NotNull(medicine, nameof(medicine));
        if (!await _activeIngredientRepository.AnyAsync(x => x.Id == activeIngredientId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidActiveIngredient", "Hoạt chất không tồn tại trong danh mục!");
        }
        medicine.AddIngredient(activeIngredientId);
    }

    public virtual async Task RemoveIngredientAsync(Medicine medicine, Guid activeIngredientId)
    {
        Check.NotNull(medicine, nameof(medicine));
        medicine.RemoveIngredient(activeIngredientId);
        await Task.CompletedTask;
    }
    #endregion

    #region Unit
    public virtual async Task AddUnitAsync(Medicine medicine, Guid unitId, int conversionFactor, int level)
    {
        Check.NotNull(medicine, nameof(medicine));
        await _productManager.ValidateUnitChangeAsync(medicine);
        medicine.AddUnit(GuidGenerator.Create(), unitId, conversionFactor, level);
    }

    public virtual async Task UpdateUnitAsync(Medicine medicine, Guid unitId, int conversionFactor, int level)
    {
        Check.NotNull(medicine, nameof(medicine));
        await _productManager.ValidateUnitChangeAsync(medicine);
        medicine.UpdateUnit(unitId, conversionFactor, level);
    }

    public virtual async Task RemoveUnitAsync(Medicine medicine, Guid unitId)
    {
        Check.NotNull(medicine, nameof(medicine));
        await _productManager.ValidateUnitChangeAsync(medicine);
        medicine.RemoveUnit(unitId);
    }
    #endregion

    #region Registration
    public virtual async Task AddRegistrationAsync(
        Medicine medicine,
        string regNumber,
        DateTime? validFrom = null,
        DateTime? validTo = null,
        string? regNote = null)
    {
        Check.NotNull(medicine, nameof(medicine));

        medicine.AddRegistration(
            GuidGenerator.Create(),
            regNumber,
            validFrom,
            validTo,
            regNote
        );

        await Task.CompletedTask;
    }
    #endregion

    #region validation
    private async Task ValidateForeignKeysAsync(Guid catId, Guid manuId, Guid unitId, Guid dosageId)
    {
        if (!await _categoryRepository.AnyAsync(x => x.Id == catId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidCategory", "Nhóm hàng không tồn tại.");
        }

        if (!await _manufacturerRepository.AnyAsync(x => x.Id == manuId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidManufacturer", "Nhà sản xuất không tồn tại.");
        }

        if (!await _unitRepository.AnyAsync(x => x.Id == unitId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidUnit", "Đơn vị tính không tồn tại.");
        }

        if (!await _dosageFormRepository.AnyAsync(x => x.Id == dosageId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidDosageForm", "Dạng bào chế không tồn tại.");
        }
    }
    #endregion
}







