using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Medicines;
using SupplyCoreERP.Medicines.Dtos;
using SupplyCoreERP.SeedData;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Catalog.Medicines;

public abstract class MedicineAppService_Integration_Tests<TStartupModule> : SupplyCoreERPApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMedicineAppService _medicineAppService;

    protected MedicineAppService_Integration_Tests()
    {
        _medicineAppService = GetRequiredService<IMedicineAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Medicines()
    {
        // Act
        PagedResultDto<MedicineDto> result = await _medicineAppService.GetListAsync(new GetMedicineListDto
        {
            MaxResultCount = 10,
            SkipCount = 0
        });

        // Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(x => x.Id == TestDataConsts.MedicineParacetamolId);
    }

    [Fact]
    public async Task Should_Create_Medicine_When_Input_Is_Valid()
    {
        // Act
        MedicineDetailDto result = await _medicineAppService.CreateAsync(new CreateUpdateMedicineDto
        {
            Name = "Ibuprofen 400mg",
            CategoryId = TestDataConsts.CategoryMedicineId,
            ManufacturerId = TestDataConsts.ManufacturerAId,
            BaseUnitId = TestDataConsts.UnitBoxId,
            DosageFormId = TestDataConsts.DosageTabletId,
            RegistrationNumber = "SDK-IBU",
            UsageRoute = UsageRoute.Oral,
            StorageCondition = StorageCondition.Normal,
            IsPrescriptionDrug = true
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Ibuprofen 400mg");
        result.Code.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Update_Medicine_When_Input_Is_Valid()
    {
        // Act
        MedicineDetailDto result = await _medicineAppService.UpdateAsync(
            TestDataConsts.MedicineParacetamolId,
            new CreateUpdateMedicineDto
            {
                Name = "Paracetamol Extra 650mg",
                CategoryId = TestDataConsts.CategoryMedicineId,
                ManufacturerId = TestDataConsts.ManufacturerAId,
                BaseUnitId = TestDataConsts.UnitBoxId,
                DosageFormId = TestDataConsts.DosageTabletId,
                RegistrationNumber = "SDK-PARA-NEW",
                UsageRoute = UsageRoute.Oral,
                StorageCondition = StorageCondition.Normal,
                IsPrescriptionDrug = false
            }
        );

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Paracetamol Extra 650mg");
    }

    [Fact]
    public async Task Should_Approve_Pending_Medicine()
    {
        // Arrange
        MedicineDetailDto newMedicine = await _medicineAppService.CreateAsync(new CreateUpdateMedicineDto
        {
            Name = "Decolgen Forte",
            CategoryId = TestDataConsts.CategoryMedicineId,
            ManufacturerId = TestDataConsts.ManufacturerAId,
            BaseUnitId = TestDataConsts.UnitBoxId,
            DosageFormId = TestDataConsts.DosageTabletId,
            RegistrationNumber = "SDK-DEC",
            UsageRoute = UsageRoute.Oral,
            StorageCondition = StorageCondition.Normal,
            IsPrescriptionDrug = false
        });

        // Act
        await _medicineAppService.ApproveAsync(newMedicine.Id);

        // Assert
        MedicineDetailDto updated = await _medicineAppService.GetAsync(newMedicine.Id);
        updated.Status.ShouldBe(MedicineStatus.Approved);
    }

    [Fact]
    public async Task Should_Get_Medicine_Detail_Successfully()
    {
        // Act
        MedicineDetailDto result = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(TestDataConsts.MedicineParacetamolId);
        result.Name.ShouldBe("Paracetamol 500mg");
        result.Code.ShouldBe("MED-001");
        result.HasTransactions.ShouldBeFalse(); // SQLite in-memory mới khởi tạo chưa có giao dịch
    }

    [Fact]
    public async Task Should_Throw_EntityNotFoundException_When_Get_NonExistent_Medicine()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(async () =>
        {
            await _medicineAppService.GetAsync(nonExistentId);
        });
    }

    [Fact]
    public async Task Should_Delete_Medicine_Successfully()
    {
        // Arrange
        MedicineDetailDto newMedicine = await _medicineAppService.CreateAsync(new CreateUpdateMedicineDto
        {
            Name = "Medicine To Delete",
            CategoryId = TestDataConsts.CategoryMedicineId,
            ManufacturerId = TestDataConsts.ManufacturerAId,
            BaseUnitId = TestDataConsts.UnitBoxId,
            DosageFormId = TestDataConsts.DosageTabletId,
            RegistrationNumber = "SDK-DEL",
            UsageRoute = UsageRoute.Oral,
            StorageCondition = StorageCondition.Normal,
            IsPrescriptionDrug = false
        });

        // Act
        await _medicineAppService.DeleteAsync(newMedicine.Id);

        // Assert
        await Should.ThrowAsync<Volo.Abp.Domain.Entities.EntityNotFoundException>(async () =>
        {
            await _medicineAppService.GetAsync(newMedicine.Id);
        });
    }

    [Fact]
    public async Task Should_Reject_Pending_Medicine_Successfully()
    {
        // Arrange
        MedicineDetailDto newMedicine = await _medicineAppService.CreateAsync(new CreateUpdateMedicineDto
        {
            Name = "Medicine To Reject",
            CategoryId = TestDataConsts.CategoryMedicineId,
            ManufacturerId = TestDataConsts.ManufacturerAId,
            BaseUnitId = TestDataConsts.UnitBoxId,
            DosageFormId = TestDataConsts.DosageTabletId,
            RegistrationNumber = "SDK-REJ",
            UsageRoute = UsageRoute.Oral,
            StorageCondition = StorageCondition.Normal,
            IsPrescriptionDrug = false
        });

        // Act
        await _medicineAppService.RejectAsync(newMedicine.Id);

        // Assert
        MedicineDetailDto updated = await _medicineAppService.GetAsync(newMedicine.Id);
        updated.Status.ShouldBe(MedicineStatus.Rejected);
    }

    [Fact]
    public async Task Should_Toggle_Active_Medicine_Successfully()
    {
        // Arrange
        MedicineDetailDto medicine = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);
        bool originalActiveState = medicine.IsActive;

        // Act
        await _medicineAppService.ToggleActiveAsync(TestDataConsts.MedicineParacetamolId);

        // Assert
        MedicineDetailDto updated = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);
        updated.IsActive.ShouldBe(!originalActiveState);
    }

    [Fact]
    public async Task Should_Get_Medicine_Summary_Successfully()
    {
        // Act
        MedicineSummaryDto result = await _medicineAppService.GetSummaryAsync();

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThan(0);
        result.TotalActive.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Should_Manage_Ingredients_Via_AppService_Successfully()
    {
        // Act (Thêm hoạt chất)
        await _medicineAppService.AddIngredientAsync(TestDataConsts.MedicineParacetamolId, new CreateUpdateMedicineIngredientDto
        {
            ActiveIngredientId = TestDataConsts.ActiveIngredientParacetamolId
        });

        // Assert (Kiểm tra thêm thành công)
        MedicineDetailDto medicine = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);
        medicine.Ingredients.Count.ShouldBe(1);
        medicine.Ingredients.First().ActiveIngredientId.ShouldBe(TestDataConsts.ActiveIngredientParacetamolId);

        // Act (Xóa hoạt chất)
        await _medicineAppService.RemoveIngredientAsync(TestDataConsts.MedicineParacetamolId, TestDataConsts.ActiveIngredientParacetamolId);

        // Assert (Kiểm tra xóa thành công)
        MedicineDetailDto updatedMedicine = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);
        updatedMedicine.Ingredients.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Manage_Units_Via_AppService_Successfully()
    {
        // Act (Thêm đơn vị quy đổi)
        await _medicineAppService.AddUnitAsync(TestDataConsts.MedicineParacetamolId, new CreateUpdateMedicineUnitDto
        {
            UnitId = TestDataConsts.UnitPillId,
            ConversionFactor = 10,
            Level = 1
        });

        // Assert (Kiểm tra thêm thành công)
        MedicineDetailDto medicine = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);
        medicine.Units.Count.ShouldBe(1);
        medicine.Units.First().UnitId.ShouldBe(TestDataConsts.UnitPillId);
        medicine.Units.First().ConversionFactor.ShouldBe(10);

        // Act (Cập nhật đơn vị quy đổi)
        await _medicineAppService.UpdateUnitAsync(TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitPillId, new CreateUpdateMedicineUnitDto
        {
            ConversionFactor = 20,
            Level = 1
        });

        // Assert (Kiểm tra cập nhật thành công)
        MedicineDetailDto medicineUpdated = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);
        medicineUpdated.Units.First().ConversionFactor.ShouldBe(20);

        // Act (Xóa đơn vị quy đổi)
        await _medicineAppService.RemoveUnitAsync(TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitPillId);

        // Assert (Kiểm tra xóa thành công)
        MedicineDetailDto medicineDeleted = await _medicineAppService.GetAsync(TestDataConsts.MedicineParacetamolId);
        medicineDeleted.Units.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Manage_Registrations_Via_AppService_Successfully()
    {
        // Act (Lấy danh sách số đăng ký)
        List<MedicineRegistrationDto> regs = await _medicineAppService.GetRegistrationsAsync(TestDataConsts.MedicineParacetamolId);
        regs.Count.ShouldBeGreaterThan(0);
        regs.First().RegistrationNumber.ShouldBe("SDK-12345");

        // Act (Thêm số đăng ký mới)
        await _medicineAppService.AddRegistrationAsync(TestDataConsts.MedicineParacetamolId, new AddMedicineRegistrationDto
        {
            RegistrationNumber = "SDK-PARA-002",
            ValidFrom = DateTime.Now,
            ValidTo = DateTime.Now.AddYears(3),
            Note = "New Extension Registration"
        });

        // Assert (Kiểm tra thêm thành công)
        List<MedicineRegistrationDto> updatedRegs = await _medicineAppService.GetRegistrationsAsync(TestDataConsts.MedicineParacetamolId);
        updatedRegs.Count.ShouldBe(2);
        updatedRegs.ShouldContain(x => x.RegistrationNumber == "SDK-PARA-002");
    }
}
