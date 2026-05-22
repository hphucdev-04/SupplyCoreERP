using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Medicines;
using SupplyCoreERP.Medicines.Dtos;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Medicines;

public abstract class MedicineAppService_Integration_Tests<TStartupModule> : SupplyCoreERPApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMedicineAppService _medicineAppService;

    protected MedicineAppService_Integration_Tests()
    {
        _medicineAppService = GetRequiredService<IMedicineAppService>();
    }

    [Fact]
    public async Task Should_Add_Unit_Successfully_When_No_Transactions()
    {
        // Arrange
        var medNoTxId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"); // Med from seed
        var newUnitId = Guid.Parse("bbbbbbbb-5555-5555-5555-555555555555"); // Gói (chưa gán vào sản phẩm)

        var input = new CreateUpdateMedicineUnitDto
        {
            UnitId = newUnitId,
            ConversionFactor = 10,
            Level = 99 // Sẽ tự gán level = 4 (Viên level 0, Vỉ level 1, Hộp level 2, Thùng cũ level 3)
        };

        // Act
        await _medicineAppService.AddUnitAsync(medNoTxId, input);

        // Assert
        MedicineDetailDto medicine = await _medicineAppService.GetAsync(medNoTxId);
        medicine.Units.ShouldNotBeNull();
        MedicineUnitDto? addedUnit = medicine.Units.FirstOrDefault(u => u.UnitId == newUnitId);
        addedUnit.ShouldNotBeNull();
        addedUnit.Level.ShouldBe(4); // max(1, 2, 3) + 1 = 4
        addedUnit.ConversionFactor.ShouldBe(10);
    }

    [Fact]
    public async Task Should_Fail_To_Add_Unit_When_Transactions_Exist()
    {
        // Arrange
        var medWithTxId = Guid.Parse("22222222-bbbb-bbbb-bbbb-222222222222"); // Med with PR transaction in seed
        var newUnitId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222"); // Hộp

        var input = new CreateUpdateMedicineUnitDto
        {
            UnitId = newUnitId,
            ConversionFactor = 10,
            Level = 2
        };

        // Act & Assert
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _medicineAppService.AddUnitAsync(medWithTxId, input);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotChangeUnitWithTransactions");
        exception.Message.ShouldContain("đã phát sinh giao dịch lịch sử");
    }

    [Fact]
    public async Task Should_Fail_To_Remove_Lower_Level_Unit_When_Higher_Level_Exists()
    {
        // Arrange
        var medNoTxId = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111"); // Med has: level 1 (Vỉ), level 2 (Hộp), level 3 (Thùng)
        var unitViId = Guid.Parse("bbbbbbbb-1111-1111-1111-111111111111"); // Vỉ (Level 1)

        // Act & Assert
        // Xóa Vỉ (Level 1) trong khi Hộp (Level 2) và Thùng (Level 3) vẫn tồn tại
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _medicineAppService.RemoveUnitAsync(medNoTxId, unitViId);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotDeleteLowerLevelUnit");
        exception.Message.ShouldContain("cấp độ cao nhất trước");
    }
}
