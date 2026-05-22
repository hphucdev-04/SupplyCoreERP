using System;
using System.Threading.Tasks;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Categories;
using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Locations.Continents;
using SupplyCoreERP.Locations.Countries;
using SupplyCoreERP.Manufacturers;
using SupplyCoreERP.Medicines;
using SupplyCoreERP.Orders.PR;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Products;

public class ProductTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<BaseUnit, Guid> _unitRepository;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Manufacturer, Guid> _manufacturerRepository;
    private readonly IRepository<DosageForm, Guid> _dosageFormRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<PurchaseRequisition, Guid> _prRepository;
    private readonly IRepository<Continent, Guid> _continentRepository;
    private readonly IRepository<Country, Guid> _countryRepository;

    public ProductTestDataSeedContributor(
        IRepository<BaseUnit, Guid> unitRepository,
        IRepository<Category, Guid> categoryRepository,
        IRepository<Manufacturer, Guid> manufacturerRepository,
        IRepository<DosageForm, Guid> dosageFormRepository,
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<Medicine, Guid> medicineRepository,
        IRepository<PurchaseRequisition, Guid> prRepository,
        IRepository<Continent, Guid> continentRepository,
        IRepository<Country, Guid> countryRepository)
    {
        _unitRepository = unitRepository;
        _categoryRepository = categoryRepository;
        _manufacturerRepository = manufacturerRepository;
        _dosageFormRepository = dosageFormRepository;
        _warehouseRepository = warehouseRepository;
        _medicineRepository = medicineRepository;
        _prRepository = prRepository;
        _continentRepository = continentRepository;
        _countryRepository = countryRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // 1. Seed BaseUnits
        var unitViId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111");
        var unitHopId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");
        var unitThungId = Guid.Parse("bbbbbbbb-3333-3333-3333-333333333333");
        var unitVienId = Guid.Parse("bbbbbbbb-4444-4444-4444-444444444444");
        var unitGoiId = Guid.Parse("bbbbbbbb-5555-5555-5555-555555555555");

        if (await _unitRepository.FindAsync(unitVienId) == null)
        {
            await _unitRepository.InsertAsync(new BaseUnit(unitVienId, "Vien", "Viên"), autoSave: true);
        }
        if (await _unitRepository.FindAsync(unitViId) == null)
        {
            await _unitRepository.InsertAsync(new BaseUnit(unitViId, "Vi", "Vỉ"), autoSave: true);
        }
        if (await _unitRepository.FindAsync(unitHopId) == null)
        {
            await _unitRepository.InsertAsync(new BaseUnit(unitHopId, "Hop", "Hộp"), autoSave: true);
        }
        if (await _unitRepository.FindAsync(unitThungId) == null)
        {
            await _unitRepository.InsertAsync(new BaseUnit(unitThungId, "Thung", "Thùng"), autoSave: true);
        }
        if (await _unitRepository.FindAsync(unitGoiId) == null)
        {
            await _unitRepository.InsertAsync(new BaseUnit(unitGoiId, "Goi", "Gói"), autoSave: true);
        }

        // 2. Seed Category
        var catId = Guid.Parse("cccccccc-1111-1111-1111-111111111111");
        if (await _categoryRepository.FindAsync(catId) == null)
        {
            await _categoryRepository.InsertAsync(new Category(catId, "Thuốc Kháng Sinh"), autoSave: true);
        }

        // 3. Seed Manufacturer
        var manuId = Guid.Parse("aaaaa111-1111-1111-1111-111111111111");
        if (await _manufacturerRepository.FindAsync(manuId) == null)
        {
            Continent? asia = await _continentRepository.FirstOrDefaultAsync(x => x.Name == "Asia");
            Country? vietnam = await _countryRepository.FirstOrDefaultAsync(x => x.ISO == "VNM");

            Guid continentId = asia?.Id ?? Guid.NewGuid();
            Guid countryId = vietnam?.Id ?? Guid.NewGuid();

            await _manufacturerRepository.InsertAsync(new Manufacturer(manuId, "M001", "Dược phẩm DHG", continentId, countryId), autoSave: true);
        }

        // 4. Seed DosageForm
        var dosageId = Guid.Parse("dddddddd-1111-1111-1111-111111111111");
        if (await _dosageFormRepository.FindAsync(dosageId) == null)
        {
            await _dosageFormRepository.InsertAsync(new DosageForm(dosageId, "VienNen", "Viên Nén"), autoSave: true);
        }

        // 5. Seed Warehouse
        var warehouseId = Guid.Parse("aaaaa222-2222-2222-2222-222222222222");
        if (await _warehouseRepository.FindAsync(warehouseId) == null)
        {
            await _warehouseRepository.InsertAsync(new Warehouse(warehouseId, "K001", "Kho chính", "Hà Nội", null, null, null), autoSave: true);
        }

        // 6. Seed Medicines
        var medNoTxId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"); // Medicine without transactions
        var medWithTxId = Guid.Parse("22222222-bbbb-bbbb-bbbb-222222222222"); // Medicine with transactions

        if (await _medicineRepository.FindAsync(medNoTxId) == null)
        {
            var medNoTx = new Medicine(
                medNoTxId,
                catId,
                manuId,
                "MED-001",
                "Paracetamol 500mg",
                unitVienId,
                dosageId,
                "VD-11111-20",
                UsageRoute.Oral,
                StorageCondition.Normal,
                isPrescriptionDrug: false
            );
            // Cấu hình chuỗi quy đổi: Viên -> Vỉ (x10) -> Hộp (x10) -> Thùng (x100)
            medNoTx.AddUnit(Guid.NewGuid(), unitViId, 10, 1);
            medNoTx.AddUnit(Guid.NewGuid(), unitHopId, 10, 2);
            medNoTx.AddUnit(Guid.NewGuid(), unitThungId, 100, 3);
            await _medicineRepository.InsertAsync(medNoTx, autoSave: true);
        }

        if (await _medicineRepository.FindAsync(medWithTxId) == null)
        {
            var medWithTx = new Medicine(
                medWithTxId,
                catId,
                manuId,
                "MED-002",
                "Amoxicillin 500mg",
                unitVienId,
                dosageId,
                "VD-22222-20",
                UsageRoute.Oral,
                StorageCondition.Normal,
                isPrescriptionDrug: true
            );
            medWithTx.AddUnit(Guid.NewGuid(), unitViId, 10, 1);
            await _medicineRepository.InsertAsync(medWithTx, autoSave: true);

            // 7. Seed Transactions for medWithTxId (PurchaseRequisition & PurchaseRequisitionLine)
            var prId = Guid.Parse("99999999-9999-9999-9999-999999999999");
            if (await _prRepository.FindAsync(prId) == null)
            {
                var pr = new PurchaseRequisition(
                    prId,
                    "PR-2026-001",
                    warehouseId,
                    DateTime.Now,
                    DateTime.Now.AddDays(7),
                    "Seed transaction for testing unit conversion locking"
                );
                pr.AddLine(Guid.NewGuid(), medWithTxId, unitVienId, 50, "Cần gấp 50 viên");
                await _prRepository.InsertAsync(pr, autoSave: true);
            }
        }
    }
}
