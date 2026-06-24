using System;
using System.Linq;
using Shouldly;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Medicines;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Catalog.Medicines;

public class Medicine_Unit_Tests
{

    [QATest(scenario: "Tạo mới medicine với hợp lệ tham số.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Create_Medicine_With_Valid_Parameters()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Guid manufacturerId = Guid.NewGuid();
        string code = "MED-PARA";
        string name = "Paracetamol 500mg";
        Guid baseUnitId = Guid.NewGuid();
        Guid dosageFormId = Guid.NewGuid();
        string initialRegNumber = "SDK-001";
        UsageRoute usageRoute = UsageRoute.Oral;
        StorageCondition storageCondition = StorageCondition.Normal;
        bool isPrescription = false;

        // Act
        Medicine medicine = new(
            id, categoryId, manufacturerId, code, name, baseUnitId, dosageFormId,
            initialRegNumber, usageRoute, storageCondition, isPrescription
        );

        // Assert
        medicine.Id.ShouldBe(id);
        medicine.CategoryId.ShouldBe(categoryId);
        medicine.ManufacturerId.ShouldBe(manufacturerId);
        medicine.Code.ShouldBe("MED-PARA");
        medicine.Name.ShouldBe(name);
        medicine.BaseUnitId.ShouldBe(baseUnitId);
        medicine.DosageFormId.ShouldBe(dosageFormId);
        medicine.UsageRoute.ShouldBe(usageRoute);
        medicine.StorageCondition.ShouldBe(storageCondition);
        medicine.IsPrescriptionDrug.ShouldBe(isPrescription);
        medicine.IsActive.ShouldBeTrue();
        medicine.Status.ShouldBe(MedicineStatus.Pending);

        medicine.Registrations.Count.ShouldBe(1);
        MedicineRegistration? reg = medicine.GetCurrentRegistration();
        reg.ShouldNotBeNull();
        reg.RegistrationNumber.ShouldBe("SDK-001");
        reg.IsActive.ShouldBeTrue();
    }

    [QATest(scenario: "Cập nhật medicine info.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Update_Medicine_Info()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();
        Guid newCategoryId = Guid.NewGuid();
        Guid newManufacturerId = Guid.NewGuid();
        Guid newBaseUnitId = Guid.NewGuid();

        // Act
        medicine.UpdateInfo("Paracetamol Extra", newCategoryId, newManufacturerId, newBaseUnitId, 0.05m);

        // Assert
        medicine.Name.ShouldBe("Paracetamol Extra");
        medicine.CategoryId.ShouldBe(newCategoryId);
        medicine.ManufacturerId.ShouldBe(newManufacturerId);
        medicine.BaseUnitId.ShouldBe(newBaseUnitId);
    }

    [QATest(scenario: "Thêm số đăng ký khi reg number changes.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Add_Registration_When_RegNumber_Changes()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();

        // Act
        medicine.AddRegistration(Guid.NewGuid(), "SDK-999", DateTime.Now, DateTime.Now.AddYears(5), "Ghi chu moi");

        // Assert
        medicine.Registrations.Count.ShouldBe(2);
        MedicineRegistration? current = medicine.GetCurrentRegistration();
        current.ShouldNotBeNull();
        current.RegistrationNumber.ShouldBe("SDK-999");
        current.IsActive.ShouldBeTrue();
        current.Note.ShouldBe("Ghi chu moi");

        // Old registration should be inactive
        MedicineRegistration old = medicine.Registrations.First(r => r.RegistrationNumber == "SDK-001");
        old.IsActive.ShouldBeFalse();
    }

    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi reg number is trùng lặp.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_BusinessException_When_RegNumber_Is_Duplicate()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
        {
            medicine.AddRegistration(Guid.NewGuid(), "SDK-001");
        }).Code.ShouldBe("SupplyCoreERP:DuplicateRegistration");
    }

    [QATest(scenario: "Cập nhật pharma info với hợp lệ tham số.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Update_PharmaInfo_With_Valid_Parameters()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();
        Guid newDosageFormId = Guid.NewGuid();

        // Act
        medicine.UpdatePharmaInfo(newDosageFormId, UsageRoute.Injection, StorageCondition.Cool, true);

        // Assert
        medicine.DosageFormId.ShouldBe(newDosageFormId);
        medicine.UsageRoute.ShouldBe(UsageRoute.Injection);
        medicine.StorageCondition.ShouldBe(StorageCondition.Cool);
        medicine.IsPrescriptionDrug.ShouldBeTrue();
    }

    [QATest(scenario: "Thêm ingredient to medicine.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Add_Ingredient_To_Medicine()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();
        Guid ingredientId = Guid.NewGuid();

        // Act
        medicine.AddIngredient(ingredientId);

        // Assert
        medicine.Ingredients.Count.ShouldBe(1);
        medicine.Ingredients.First().ActiveIngredientId.ShouldBe(ingredientId);
    }

    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi ingredient is trùng lặp.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_BusinessException_When_Ingredient_Is_Duplicate()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();
        Guid ingredientId = Guid.NewGuid();
        medicine.AddIngredient(ingredientId);

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
        {
            medicine.AddIngredient(ingredientId);
        }).Code.ShouldBe("SupplyCoreERP:DuplicateIngredient");
    }

    [QATest(scenario: "Loại bỏ ingredient from medicine.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Remove_Ingredient_From_Medicine()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();
        Guid ingredientId = Guid.NewGuid();
        medicine.AddIngredient(ingredientId);

        // Act
        medicine.RemoveIngredient(ingredientId);

        // Assert
        medicine.Ingredients.Count.ShouldBe(0);
    }

    [QATest(scenario: "Thêm đơn vị tính với conversion factor.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Add_Unit_With_Conversion_Factor()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();
        Guid unitId = Guid.NewGuid();

        // Act
        medicine.AddUnit(Guid.NewGuid(), unitId, 10, 1);

        // Assert
        medicine.Units.Count.ShouldBe(1);
        ProductUnit unit = medicine.Units.First();
        unit.UnitId.ShouldBe(unitId);
        unit.ConversionFactor.ShouldBe(10);
    }

    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi Thêm đơn vị tính is trùng lặp of base đơn vị tính.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Throw_BusinessException_When_AddUnit_Is_Duplicate_Of_BaseUnit()
    {
        // Arrange
        Guid baseUnitId = Guid.NewGuid();
        Guid id = Guid.NewGuid();
        Guid categoryId = Guid.NewGuid();
        Guid manufacturerId = Guid.NewGuid();
        Guid dosageFormId = Guid.NewGuid();

        Medicine medicine = new(
            id, categoryId, manufacturerId, "MED-001", "Paracetamol", baseUnitId, dosageFormId,
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
        {
            medicine.AddUnit(Guid.NewGuid(), baseUnitId, 10, 1);
        }).Code.ShouldBe("SupplyCoreERP:DuplicateBaseUnit");
    }

    [QATest(scenario: "Approve pending medicine.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Approve_Pending_Medicine()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();
        medicine.Status.ShouldBe(MedicineStatus.Pending);

        // Act
        medicine.Approve();

        // Assert
        medicine.Status.ShouldBe(MedicineStatus.Approved);
        medicine.IsAvailableForInventory.ShouldBeTrue();
    }

    [QATest(scenario: "Reject pending medicine.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public void Should_Reject_Pending_Medicine()
    {
        // Arrange
        Medicine medicine = CreateSampleMedicine();

        // Act
        medicine.Reject();

        // Assert
        medicine.Status.ShouldBe(MedicineStatus.Rejected);
        medicine.IsAvailableForInventory.ShouldBeFalse();
    }

    private Medicine CreateSampleMedicine()
    {
        return new Medicine(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol 500mg", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
    }
}
