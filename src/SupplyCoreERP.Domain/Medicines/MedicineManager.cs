using System;
using System.Threading.Tasks;
using SupplyCoreERP.ActiveIngredients;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Categories;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Manufacturers;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Medicines;

public class MedicineManager : DomainService
{
    private readonly ProductManager _productManager;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Manufacturer, Guid> _manufacturerRepository;
    private readonly IRepository<BaseUnit, Guid> _unitRepository;
    private readonly IRepository<DosageForm, Guid> _dosageFormRepository;
    private readonly IRepository<ActiveIngredient, Guid> _activeIngredientRepository;
    private readonly DocumentSequenceManager _documentSequenceManager;

    public MedicineManager(
        ProductManager productManager,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Manufacturer, Guid> manufacturerRepository,
        IRepository<BaseUnit, Guid> unitRepository,
        IRepository<DosageForm, Guid> dosageFormRepository,
        IRepository<ActiveIngredient, Guid> activeIngredientRepository,
        IRepository<Country, Guid> countryRepository,
        DocumentSequenceManager documentSequenceManager
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

    public async Task<Medicine> CreateAsync(
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

    public async Task UpdateAsync(
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

        // Validate tất cả khóa ngoại bao gồm baseUnitId mới từ input
        await ValidateForeignKeysAsync(categoryId, manufacturerId, baseUnitId, dosageFormId);

        // Kích hoạt kiểm soát BaseUnit qua ProductManager
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
            // Tuỳ chọn: Nếu không đổi SĐK, có thể cho phép cập nhật lại ngày tháng của SĐK hiện tại nếu muốn.
            // currentReg.UpdateValidity(regValidFrom, regValidTo);
            // currentReg.SetNote(regNote);
        }

        // Update toàn bộ thông tin pharma từ input thực sự, 
        medicine.UpdatePharmaInfo(dosageFormId, usageRoute, storageCondition, isPrescriptionDrug);

        medicine.SetPending();
    }

    private async Task ValidateForeignKeysAsync(Guid catId, Guid manuId, Guid unitId, Guid dosageId)
    {
        if (!await _categoryRepository.AnyAsync(x => x.Id == catId))
        {
            throw new UserFriendlyException("Nhóm hàng không tồn tại.");
        }

        if (!await _manufacturerRepository.AnyAsync(x => x.Id == manuId))
        {
            throw new UserFriendlyException("Nhà sản xuất không tồn tại.");
        }

        if (!await _unitRepository.AnyAsync(x => x.Id == unitId))
        {
            throw new UserFriendlyException("Đơn vị tính không tồn tại.");
        }

        if (!await _dosageFormRepository.AnyAsync(x => x.Id == dosageId))
        {
            throw new UserFriendlyException("Dạng bào chế không tồn tại.");
        }
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

    public async Task AddUnitAsync(Medicine medicine, Guid unitId, int conversionFactor, int level)
    {
        Check.NotNull(medicine, nameof(medicine));
        await _productManager.ValidateUnitChangeAsync(medicine);
        medicine.AddUnit(GuidGenerator.Create(), unitId, conversionFactor, level);
    }

    public async Task UpdateUnitAsync(Medicine medicine, Guid unitId, int conversionFactor, int level)
    {
        Check.NotNull(medicine, nameof(medicine));
        await _productManager.ValidateUnitChangeAsync(medicine);
        medicine.UpdateUnit(unitId, conversionFactor, level);
    }

    public async Task RemoveUnitAsync(Medicine medicine, Guid unitId)
    {
        Check.NotNull(medicine, nameof(medicine));
        await _productManager.ValidateUnitChangeAsync(medicine);
        medicine.RemoveUnit(unitId);
    }
}
