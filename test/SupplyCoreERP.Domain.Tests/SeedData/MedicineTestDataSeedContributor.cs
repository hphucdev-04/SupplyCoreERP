using System;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.ActiveIngredients;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Categories;
using SupplyCoreERP.Catalog.DosageForms;
using SupplyCoreERP.Catalog.Manufacturers;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.SeedData;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.SeedData;

public class MedicineTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Continent, Guid> _continentRepository;
    private readonly IRepository<Country, Guid> _countryRepository;
    private readonly IRepository<Manufacturer, Guid> _manufacturerRepository;
    private readonly IRepository<BaseUnit, Guid> _unitRepository;
    private readonly IRepository<DosageForm, Guid> _dosageFormRepository;
    private readonly IRepository<ActiveIngredient, Guid> _activeIngredientRepository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;

    public MedicineTestDataSeedContributor(
        IRepository<Category, Guid> categoryRepository,
        IRepository<Continent, Guid> continentRepository,
        IRepository<Country, Guid> countryRepository,
        IRepository<Manufacturer, Guid> manufacturerRepository,
        IRepository<BaseUnit, Guid> unitRepository,
        IRepository<DosageForm, Guid> dosageFormRepository,
        IRepository<ActiveIngredient, Guid> activeIngredientRepository,
        IRepository<Medicine, Guid> medicineRepository)
    {
        _categoryRepository = categoryRepository;
        _continentRepository = continentRepository;
        _countryRepository = countryRepository;
        _manufacturerRepository = manufacturerRepository;
        _unitRepository = unitRepository;
        _dosageFormRepository = dosageFormRepository;
        _activeIngredientRepository = activeIngredientRepository;
        _medicineRepository = medicineRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 1. Seed Category
        if (await _categoryRepository.FindAsync(TestDataConsts.CategoryMedicineId) == null)
        {
            await _categoryRepository.InsertAsync(
                new Category(TestDataConsts.CategoryMedicineId, "Thuốc Kháng Sinh"),
                autoSave: true
            );
        }

        // 2. Seed Geography & Manufacturer
        Continent? asia = await _continentRepository.FirstOrDefaultAsync(x => x.Name == "Asia");
        if (asia == null)
        {
            asia = await _continentRepository.InsertAsync(new Continent(Guid.NewGuid(), "Asia"), autoSave: true);
        }

        Country? vn = await _countryRepository.FirstOrDefaultAsync(x => x.ISO == "VNM");
        if (vn == null)
        {
            vn = await _countryRepository.InsertAsync(new Country(Guid.NewGuid(), asia.Id, "VNM", "Viet Nam"), autoSave: true);
        }

        if (await _manufacturerRepository.FindAsync(TestDataConsts.ManufacturerAId) == null)
        {
            await _manufacturerRepository.InsertAsync(
                new Manufacturer(TestDataConsts.ManufacturerAId, "MAN-001", "Dược Hậu Giang", asia.Id, vn.Id),
                autoSave: true
            );
        }

        // 3. Seed Base Units
        if (await _unitRepository.FindAsync(TestDataConsts.UnitBoxId) == null)
        {
            await _unitRepository.InsertAsync(
                new BaseUnit(TestDataConsts.UnitBoxId, "BOX", "Hộp"),
                autoSave: true
            );
        }

        if (await _unitRepository.FindAsync(TestDataConsts.UnitPillId) == null)
        {
            await _unitRepository.InsertAsync(
                new BaseUnit(TestDataConsts.UnitPillId, "PILL", "Viên"),
                autoSave: true
            );
        }

        // 4. Seed Dosage Form
        if (await _dosageFormRepository.FindAsync(TestDataConsts.DosageTabletId) == null)
        {
            await _dosageFormRepository.InsertAsync(
                new DosageForm(TestDataConsts.DosageTabletId, "TAB", "Viên Nén"),
                autoSave: true
            );
        }

        // 5. Seed Active Ingredient
        if (await _activeIngredientRepository.FindAsync(TestDataConsts.ActiveIngredientParacetamolId) == null)
        {
            await _activeIngredientRepository.InsertAsync(
                new ActiveIngredient(TestDataConsts.ActiveIngredientParacetamolId, "PARA", "Paracetamol"),
                autoSave: true
            );
        }

        // 6. Seed Medicine
        if (await _medicineRepository.FindAsync(TestDataConsts.MedicineParacetamolId) == null)
        {
            Medicine medicine = new(
                TestDataConsts.MedicineParacetamolId,
                TestDataConsts.CategoryMedicineId,
                TestDataConsts.ManufacturerAId,
                "MED-001",
                "Paracetamol 500mg",
                TestDataConsts.UnitBoxId,
                TestDataConsts.DosageTabletId,
                "SDK-12345",
                Enums.Medicines.UsageRoute.Oral,
                Enums.Medicines.StorageCondition.Normal,
                false
            );

            medicine.Approve();
            await _medicineRepository.InsertAsync(medicine, autoSave: true);
        }
    }
}
