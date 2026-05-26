using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.SeedData;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Catalog.Medicines;

public abstract class MedicineManager_Integration_Tests<TStartupModule> : SupplyCoreERPDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly MedicineManager _medicineManager;
    private readonly IRepository<Medicine, Guid> _medicineRepository;

    protected MedicineManager_Integration_Tests()
    {
        _medicineManager = GetRequiredService<MedicineManager>();
        _medicineRepository = GetRequiredService<IRepository<Medicine, Guid>>();
    }

    [QATest(scenario: "Tạo mới medicine khi tham số are hợp lệ.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Create_Medicine_When_Parameters_Are_Valid()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act
            Medicine medicine = await _medicineManager.CreateAsync(
                "Amoxicillin 500mg",
                TestDataConsts.CategoryMedicineId,
                TestDataConsts.ManufacturerAId,
                TestDataConsts.UnitBoxId,
                TestDataConsts.DosageTabletId,
                "SDK-AMOX",
                UsageRoute.Oral,
                StorageCondition.Normal,
                true
            );

            // Assert
            medicine.ShouldNotBeNull();
            medicine.Name.ShouldBe("Amoxicillin 500mg");
            medicine.Code.ShouldNotBeNullOrWhiteSpace();
            medicine.DosageFormId.ShouldBe(TestDataConsts.DosageTabletId);
            medicine.Status.ShouldBe(MedicineStatus.Pending);
        });
    }

    [QATest(scenario: "Cập nhật medicine thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Update_Medicine_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);

            // Act
            await _medicineManager.UpdateAsync(
                medicine,
                "Paracetamol Extra 500mg",
                TestDataConsts.CategoryMedicineId,
                TestDataConsts.ManufacturerAId,
                TestDataConsts.UnitBoxId,
                TestDataConsts.DosageTabletId,
                "SDK-PARA-NEW",
                UsageRoute.Oral,
                StorageCondition.Normal,
                false
            );

            // Assert
            medicine.Name.ShouldBe("Paracetamol Extra 500mg");
            medicine.GetCurrentRegistration().ShouldNotBeNull();
            medicine.GetCurrentRegistration().RegistrationNumber.ShouldBe("SDK-PARA-NEW");
        });
    }

    [QATest(scenario: "Thêm ingredient thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Add_Ingredient_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);

            // Act
            await _medicineManager.AddIngredientAsync(medicine, TestDataConsts.ActiveIngredientParacetamolId);

            // Assert
            medicine.Ingredients.Count.ShouldBe(1);
            medicine.Ingredients.First().ActiveIngredientId.ShouldBe(TestDataConsts.ActiveIngredientParacetamolId);
        });
    }

    [QATest(scenario: "Loại bỏ ingredient thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Remove_Ingredient_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);
            await _medicineManager.AddIngredientAsync(medicine, TestDataConsts.ActiveIngredientParacetamolId);

            // Act
            await _medicineManager.RemoveIngredientAsync(medicine, TestDataConsts.ActiveIngredientParacetamolId);

            // Assert
            medicine.Ingredients.Count.ShouldBe(0);
        });
    }

    [QATest(scenario: "Thêm đơn vị quy đổi thành công cho thuốc.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Add_Unit_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);

            // Act
            await _medicineManager.AddUnitAsync(medicine, TestDataConsts.UnitPillId, 10, 1);

            // Assert
            medicine.Units.Count.ShouldBe(1);
            medicine.Units.First().UnitId.ShouldBe(TestDataConsts.UnitPillId);
            medicine.Units.First().ConversionFactor.ShouldBe(10);
        });
    }

    [QATest(scenario: "Cập nhật đơn vị quy đổi thành công cho thuốc.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Update_Unit_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);
            await _medicineManager.AddUnitAsync(medicine, TestDataConsts.UnitPillId, 10, 1);

            // Act
            await _medicineManager.UpdateUnitAsync(medicine, TestDataConsts.UnitPillId, 20, 1);

            // Assert
            medicine.Units.First().ConversionFactor.ShouldBe(20);
        });
    }

    [QATest(scenario: "Xóa đơn vị quy đổi thành công khỏi thuốc.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Remove_Unit_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);
            await _medicineManager.AddUnitAsync(medicine, TestDataConsts.UnitPillId, 10, 1);

            // Act
            await _medicineManager.RemoveUnitAsync(medicine, TestDataConsts.UnitPillId);

            // Assert
            medicine.Units.Count.ShouldBe(0);
        });
    }

    [QATest(scenario: "Approve pending medicine thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Approve_Pending_Medicine_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);
            medicine.SetPending(); // Đảm bảo trạng thái ban đầu là Pending

            // Act
            medicine.Approve();
            await _medicineRepository.UpdateAsync(medicine);

            // Assert
            medicine.Status.ShouldBe(MedicineStatus.Approved);
        });
    }

    [QATest(scenario: "Reject pending medicine thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Reject_Pending_Medicine_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);
            medicine.SetPending(); // Đảm bảo trạng thái ban đầu là Pending

            // Act
            medicine.Reject();
            await _medicineRepository.UpdateAsync(medicine);

            // Assert
            medicine.Status.ShouldBe(MedicineStatus.Rejected);
        });
    }

    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi approve non pending medicine.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Approve_Non_Pending_Medicine()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine medicine = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);
            medicine.SetPending();
            medicine.Approve(); // Đã duyệt sang Approved

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                medicine.Approve();
            });
            ex.Code.ShouldBe("SupplyCoreERP:InvalidMedicineStatus");
        });
    }

    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi Tạo mới trùng lặp mã code.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Create_Duplicate_Code()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Medicine paracetamol = await _medicineRepository.GetAsync(TestDataConsts.MedicineParacetamolId);
            string existingCode = paracetamol.Code;

            // Act & Assert
            // Trong integration test, việc kiểm tra trùng mã code sẽ ném BusinessException của ProductManager.CheckCodeAsync
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                // Gọi create trực tiếp sẽ sinh mã code tự động, nên để giả lập trùng mã, ta gọi CheckCodeAsync của ProductManager hoặc tạo trùng code qua constructor.
                // Ở đây, ta kiểm tra trực tiếp nghiệp vụ check code trùng của ProductManager.
                ProductManager productManager = GetRequiredService<ProductManager>();
                await productManager.CheckCodeAsync(existingCode);
            });
            ex.Code.ShouldBe("SupplyCoreERP:DuplicateProductCode");
        });
    }
}
