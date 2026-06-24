using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
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
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace SupplyCoreERP.Catalog.Medicines;

public class MedicineManager_Unit_Tests
{
    private readonly ProductManager _productManager;
    private readonly IRepository<Category, Guid> _categoryRepository;
    private readonly IRepository<Manufacturer, Guid> _manufacturerRepository;
    private readonly IRepository<BaseUnit, Guid> _unitRepository;
    private readonly IRepository<DosageForm, Guid> _dosageFormRepository;
    private readonly IRepository<ActiveIngredient, Guid> _activeIngredientRepository;
    private readonly IRepository<Country, Guid> _countryRepository;
    private readonly IDocumentSequenceManager _documentSequenceManager;
    private readonly MedicineManager _medicineManager;

    public MedicineManager_Unit_Tests()
    {
        _productManager = Substitute.For<ProductManager>(
            Substitute.For<IRepository<Product, Guid>>(),
            Substitute.For<IRepository<Inventory.Balances.InventoryBalance, Guid>>(),
            Substitute.For<IRepository<Inventory.Tickets.InventoryTicketLine, Guid>>(),
            Substitute.For<IRepository<Procurement.PurchaseOrders.PurchaseOrderLine, Guid>>(),
            Substitute.For<IRepository<Sales.Orders.SalesOrderLine, Guid>>(),
            Substitute.For<IRepository<Procurement.PurchaseRequisitions.PurchaseRequisitionLine, Guid>>()
        );

        _categoryRepository = Substitute.For<IRepository<Category, Guid>>();
        _manufacturerRepository = Substitute.For<IRepository<Manufacturer, Guid>>();
        _unitRepository = Substitute.For<IRepository<BaseUnit, Guid>>();
        _dosageFormRepository = Substitute.For<IRepository<DosageForm, Guid>>();
        _activeIngredientRepository = Substitute.For<IRepository<ActiveIngredient, Guid>>();
        _countryRepository = Substitute.For<IRepository<Country, Guid>>();
        _documentSequenceManager = Substitute.For<IDocumentSequenceManager>();

        _medicineManager = new MedicineManager(
            _productManager,
            _categoryRepository,
            _manufacturerRepository,
            _unitRepository,
            _dosageFormRepository,
            _activeIngredientRepository,
            _countryRepository,
            _documentSequenceManager
        );

        IGuidGenerator guidGenerator = Substitute.For<IGuidGenerator>();
        guidGenerator.Create().Returns(x => Guid.NewGuid());

        IAbpLazyServiceProvider lazyServiceProvider = Substitute.For<IAbpLazyServiceProvider>();
        lazyServiceProvider.LazyGetRequiredService(typeof(IGuidGenerator)).Returns(guidGenerator);

        typeof(Volo.Abp.Domain.Services.DomainService)
            .GetProperty("LazyServiceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            ?.SetValue(_medicineManager, lazyServiceProvider);
    }

    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi foreign keys are không hợp lệ.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Foreign_Keys_Are_Invalid()
    {
        // Arrange
        _categoryRepository.AnyAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _medicineManager.CreateAsync(
                "Paracetamol 500mg", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidCategory");
    }

    [QATest(scenario: "Tạo mới Medicine thành công qua Domain Service.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_Medicine_Successfully()
    {
        // Arrange
        Guid catId = Guid.NewGuid();
        Guid manuId = Guid.NewGuid();
        Guid unitId = Guid.NewGuid();
        Guid dosageId = Guid.NewGuid();

        _categoryRepository.AnyAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(true);
        _manufacturerRepository.AnyAsync(Arg.Any<Expression<Func<Manufacturer, bool>>>()).Returns(true);
        _unitRepository.AnyAsync(Arg.Any<Expression<Func<BaseUnit, bool>>>()).Returns(true);
        _dosageFormRepository.AnyAsync(Arg.Any<Expression<Func<DosageForm, bool>>>()).Returns(true);

        _documentSequenceManager.GenerateAsync(Arg.Any<string>()).Returns("MED-001");
        _productManager.CheckCodeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        // Act
        Medicine medicine = await _medicineManager.CreateAsync(
            "Paracetamol 500mg", catId, manuId, unitId, dosageId,
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false,
            0.05m, DateTime.Now, DateTime.Now.AddYears(5), "Initial Registration"
        );

        // Assert
        medicine.ShouldNotBeNull();
        medicine.Name.ShouldBe("Paracetamol 500mg");
        medicine.Code.ShouldBe("MED-001");
        medicine.CategoryId.ShouldBe(catId);
        medicine.ManufacturerId.ShouldBe(manuId);
        medicine.BaseUnitId.ShouldBe(unitId);
        medicine.DosageFormId.ShouldBe(dosageId);
        medicine.UsageRoute.ShouldBe(UsageRoute.Oral);
        medicine.StorageCondition.ShouldBe(StorageCondition.Normal);
        medicine.IsPrescriptionDrug.ShouldBeFalse();
        medicine.Status.ShouldBe(MedicineStatus.Pending);

        MedicineRegistration reg = medicine.GetCurrentRegistration();
        reg.ShouldNotBeNull();
        reg.RegistrationNumber.ShouldBe("SDK-001");
        reg.Note.ShouldBe("Initial Registration");
    }

    [QATest(scenario: "Cập nhật medicine thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Update_Medicine_Successfully()
    {
        // Arrange
        Guid catId = Guid.NewGuid();
        Guid manuId = Guid.NewGuid();
        Guid unitId = Guid.NewGuid();
        Guid dosageId = Guid.NewGuid();

        _categoryRepository.AnyAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(true);
        _manufacturerRepository.AnyAsync(Arg.Any<Expression<Func<Manufacturer, bool>>>()).Returns(true);
        _unitRepository.AnyAsync(Arg.Any<Expression<Func<BaseUnit, bool>>>()).Returns(true);
        _dosageFormRepository.AnyAsync(Arg.Any<Expression<Func<DosageForm, bool>>>()).Returns(true);
        _productManager.ValidateBaseUnitChangeAsync(Arg.Any<Product>(), Arg.Any<Guid>()).Returns(Task.CompletedTask);

        Medicine medicine = new(
            Guid.NewGuid(), catId, manuId, "MED-001", "Paracetamol", unitId, dosageId,
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );

        // Act
        await _medicineManager.UpdateAsync(
            medicine,
            "Paracetamol Extra 500mg", catId, manuId, unitId, dosageId,
            "SDK-001", UsageRoute.Oral, StorageCondition.Cool, true,
            0.05m, null, null, "Updated validity and storage"
        );

        // Assert
        medicine.Name.ShouldBe("Paracetamol Extra 500mg");
        medicine.StorageCondition.ShouldBe(StorageCondition.Cool);
        medicine.IsPrescriptionDrug.ShouldBeTrue();
        medicine.Status.ShouldBe(MedicineStatus.Pending); // Phải chuyển về pending chờ duyệt lại

        MedicineRegistration reg = medicine.GetCurrentRegistration();
        reg.ShouldNotBeNull();
        reg.RegistrationNumber.ShouldBe("SDK-001");
        reg.Note.ShouldBe("Updated validity and storage");
    }

    [QATest(scenario: "Cập nhật medicine với new số đăng ký thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Update_Medicine_With_New_Registration_Successfully()
    {
        // Arrange
        Guid catId = Guid.NewGuid();
        Guid manuId = Guid.NewGuid();
        Guid unitId = Guid.NewGuid();
        Guid dosageId = Guid.NewGuid();

        _categoryRepository.AnyAsync(Arg.Any<Expression<Func<Category, bool>>>()).Returns(true);
        _manufacturerRepository.AnyAsync(Arg.Any<Expression<Func<Manufacturer, bool>>>()).Returns(true);
        _unitRepository.AnyAsync(Arg.Any<Expression<Func<BaseUnit, bool>>>()).Returns(true);
        _dosageFormRepository.AnyAsync(Arg.Any<Expression<Func<DosageForm, bool>>>()).Returns(true);
        _productManager.ValidateBaseUnitChangeAsync(Arg.Any<Product>(), Arg.Any<Guid>()).Returns(Task.CompletedTask);

        Medicine medicine = new(
            Guid.NewGuid(), catId, manuId, "MED-001", "Paracetamol", unitId, dosageId,
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );

        // Act (Thay đổi số đăng ký mới)
        await _medicineManager.UpdateAsync(
            medicine,
            "Paracetamol 500mg", catId, manuId, unitId, dosageId,
            "SDK-002", UsageRoute.Oral, StorageCondition.Normal, false,
            0.05m
        );

        // Assert
        medicine.GetCurrentRegistration().ShouldNotBeNull();
        medicine.GetCurrentRegistration().RegistrationNumber.ShouldBe("SDK-002");
        medicine.Registrations.Count.ShouldBe(2); // Có 2 bản ghi số đăng ký
    }

    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi Thêm non existent ingredient.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Add_NonExistent_Ingredient()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid activeIngredientId = Guid.NewGuid();
        _activeIngredientRepository.AnyAsync(Arg.Any<Expression<Func<ActiveIngredient, bool>>>()).Returns(false);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _medicineManager.AddIngredientAsync(medicine, activeIngredientId);
        });
        ex.Code.ShouldBe("SupplyCoreERP:InvalidActiveIngredient");
    }

    [QATest(scenario: "Thêm ingredient thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Add_Ingredient_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid activeIngredientId = Guid.NewGuid();
        _activeIngredientRepository.AnyAsync(Arg.Any<Expression<Func<ActiveIngredient, bool>>>()).Returns(true);

        // Act
        await _medicineManager.AddIngredientAsync(medicine, activeIngredientId);

        // Assert
        medicine.Ingredients.Count.ShouldBe(1);
        medicine.Ingredients.First().ActiveIngredientId.ShouldBe(activeIngredientId);
    }

    [QATest(scenario: "Loại bỏ ingredient thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Remove_Ingredient_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid activeIngredientId = Guid.NewGuid();
        _activeIngredientRepository.AnyAsync(Arg.Any<Expression<Func<ActiveIngredient, bool>>>()).Returns(true);
        await _medicineManager.AddIngredientAsync(medicine, activeIngredientId);

        // Act
        await _medicineManager.RemoveIngredientAsync(medicine, activeIngredientId);

        // Assert
        medicine.Ingredients.Count.ShouldBe(0);
    }

    [QATest(scenario: "Thêm ingredient có hàm lượng thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Add_Ingredient_With_Strength_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid activeIngredientId = Guid.NewGuid();
        _activeIngredientRepository.AnyAsync(Arg.Any<Expression<Func<ActiveIngredient, bool>>>()).Returns(true);

        // Act
        await _medicineManager.AddIngredientAsync(medicine, activeIngredientId, "500mg");

        // Assert
        medicine.Ingredients.Count.ShouldBe(1);
        medicine.Ingredients.First().Strength.ShouldBe("500mg");
    }

    [QATest(scenario: "Cập nhật hàm lượng ingredient thành công.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Update_Ingredient_Strength_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid activeIngredientId = Guid.NewGuid();
        _activeIngredientRepository.AnyAsync(Arg.Any<Expression<Func<ActiveIngredient, bool>>>()).Returns(true);
        await _medicineManager.AddIngredientAsync(medicine, activeIngredientId, "250mg");

        // Act
        await _medicineManager.UpdateIngredientStrengthAsync(medicine, activeIngredientId, "500mg");

        // Assert
        medicine.Ingredients.First().Strength.ShouldBe("500mg");
    }

    [QATest(scenario: "Cập nhật hàm lượng ingredient không tồn tại phải ném exception.", feature: "Medicine", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_When_Update_Strength_Of_NonExistent_Ingredient()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid nonExistentIngredientId = Guid.NewGuid();
        _activeIngredientRepository.AnyAsync(Arg.Any<Expression<Func<ActiveIngredient, bool>>>()).Returns(true);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _medicineManager.UpdateIngredientStrengthAsync(medicine, nonExistentIngredientId, "500mg");
        });
        ex.Code.ShouldBe("SupplyCoreERP:IngredientNotFound");
    }

    [QATest(scenario: "Thêm đơn vị quy đổi thành công cho thuốc.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Add_Unit_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid unitId = Guid.NewGuid();
        _productManager.ValidateUnitChangeAsync(Arg.Any<Product>()).Returns(Task.CompletedTask);

        // Act
        await _medicineManager.AddUnitAsync(medicine, unitId, 10, 1);

        // Assert
        medicine.Units.Count.ShouldBe(1);
        medicine.Units.First().UnitId.ShouldBe(unitId);
        medicine.Units.First().ConversionFactor.ShouldBe(10);
        medicine.Units.First().Level.ShouldBe(1);
    }

    [QATest(scenario: "Cập nhật đơn vị quy đổi thành công cho thuốc.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Update_Unit_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid unitId = Guid.NewGuid();
        _productManager.ValidateUnitChangeAsync(Arg.Any<Product>()).Returns(Task.CompletedTask);
        await _medicineManager.AddUnitAsync(medicine, unitId, 10, 1);

        // Act
        await _medicineManager.UpdateUnitAsync(medicine, unitId, 20, 1);

        // Assert
        medicine.Units.First().ConversionFactor.ShouldBe(20);
        medicine.Units.First().Level.ShouldBe(1);
    }

    [QATest(scenario: "Xóa đơn vị quy đổi thành công khỏi thuốc.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Remove_Unit_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );
        Guid unitId = Guid.NewGuid();
        _productManager.ValidateUnitChangeAsync(Arg.Any<Product>()).Returns(Task.CompletedTask);
        await _medicineManager.AddUnitAsync(medicine, unitId, 10, 1);

        // Act
        await _medicineManager.RemoveUnitAsync(medicine, unitId);

        // Assert
        medicine.Units.Count.ShouldBe(0);
    }

    [QATest(scenario: "Thêm số đăng ký mới thành công cho thuốc qua Manager.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Add_Registration_Via_Manager_Successfully()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );

        // Act
        await _medicineManager.AddRegistrationAsync(
            medicine,
            "SDK-002",
            DateTime.Now,
            DateTime.Now.AddYears(5),
            "Ghi chu moi qua Manager"
        );

        // Assert
        medicine.Registrations.Count.ShouldBe(2); // Initial registration (SDK-001) + New registration (SDK-002)
        MedicineRegistration? currentReg = medicine.GetCurrentRegistration();
        currentReg.ShouldNotBeNull();
        currentReg.RegistrationNumber.ShouldBe("SDK-002");
        currentReg.Note.ShouldBe("Ghi chu moi qua Manager");
    }
    [QATest(scenario: "Ném ngoại lệ khi thêm trùng số đăng ký qua Manager.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_Exception_When_Adding_Duplicate_Registration_Via_Manager()
    {
        // Arrange
        Medicine medicine = new(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MED-001", "Paracetamol", Guid.NewGuid(), Guid.NewGuid(),
            "SDK-001", UsageRoute.Oral, StorageCondition.Normal, false
        );

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _medicineManager.AddRegistrationAsync(
                medicine,
                "SDK-001" // Duplicate registration number
            );
        });
        ex.Code.ShouldBe("SupplyCoreERP:DuplicateRegistration");
    }
}
