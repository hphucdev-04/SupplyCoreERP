using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Partner;
using SupplyCoreERP.SeedData;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Partner.Suppliers;

public abstract class SupplierManager_Integration_Tests<TStartupModule> : SupplyCoreERPDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ISupplierManager _supplierManager;
    private readonly IMedicineManager _medicineManager;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    protected SupplierManager_Integration_Tests()
    {
        _supplierManager = GetRequiredService<ISupplierManager>();
        _medicineManager = GetRequiredService<IMedicineManager>();
        _supplierRepository = GetRequiredService<IRepository<Supplier, Guid>>();
        _asyncExecuter = GetRequiredService<IAsyncQueryableExecuter>();
    }
    [QATest(scenario: "Tạo nhà cung cấp thành công và tự động sinh mã code tăng dần.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_Supplier_And_Generate_Supplier_Code()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act
            Supplier supplier = await _supplierManager.CreateAsync(
                "Supplier New",
                "MST-NEW-1",
                "0909999111",
                "new_supplier@test.com",
                "Rep New",
                Gender.Male,
                "Note New",
                "789 Le Loi",
                null,
                null,
                null,
                100000000m,
                15
            );

            // Assert
            supplier.ShouldNotBeNull();
            supplier.Code.ShouldNotBeNullOrWhiteSpace();
            supplier.Name.ShouldBe("Supplier New");
            supplier.DebtLimit.ShouldBe(100000000m);
            supplier.PaymentTermDays.ShouldBe(15);
        });
    }
    [QATest(scenario: "Ném ngoại lệ khi tạo trùng mã nhà cung cấp đã có sẵn.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Code_Or_Name_Exists()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act & Assert
            BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _supplierManager.CheckCodeAndNameAsync("SUP-001", "Nhà Cung Cấp A");
            });
            ex.Code.ShouldBe("SupplyCoreERP:SupplierCodeExists");
        });
    }
    [QATest(scenario: "Thêm sản phẩm thành công vào NCC và lưu xuống DB.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Add_Product_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Supplier supplier = await _supplierManager.CreateAsync(
                "Supplier New For Product", "MST-PROD-ADD", null, null, null, null, null, null, null, null, null
            );
            await _supplierRepository.InsertAsync(supplier, autoSave: true); // <-- thêm dòng này

            // Act
            SupplierProduct sp = await _supplierManager.AddProductAsync(
                supplier, TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 5, true, "Preferred Product"
            );

            // Assert
            sp.ShouldNotBeNull();
            sp.ProductId.ShouldBe(TestDataConsts.MedicineParacetamolId);
            sp.DefaultUnitId.ShouldBe(TestDataConsts.UnitBoxId);
            sp.LeadTimeDays.ShouldBe(5);
            sp.IsPreferred.ShouldBeTrue();
            supplier.SupplierProducts.Count.ShouldBe(1);
        });
    }
    [QATest(scenario: "Cập nhật sản phẩm cung cấp thành công và lưu DB.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Update_Product_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            IQueryable<Supplier> supplierQuery = await _supplierRepository.WithDetailsAsync(x => x.SupplierProducts);
            Supplier? supplier = supplierQuery.FirstOrDefault(x => x.Id == TestDataConsts.SupplierAId);

            // Act
            await _supplierManager.UpdateProductAsync(
                supplier, TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitPillId, 10, false, "Updated Note"
            );

            // Assert
            SupplierProduct sp = supplier.SupplierProducts.First(x => x.ProductId == TestDataConsts.MedicineParacetamolId);
            sp.DefaultUnitId.ShouldBe(TestDataConsts.UnitPillId);
            sp.LeadTimeDays.ShouldBe(10);
            sp.IsPreferred.ShouldBeFalse();
            sp.Note.ShouldBe("Updated Note");
        });
    }
    [QATest(scenario: "Xóa sản phẩm cung cấp thành công và đồng bộ DB.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Remove_Product_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            IQueryable<Supplier> supplierQuery = await _supplierRepository.WithDetailsAsync(x => x.SupplierProducts);
            Supplier? supplier = supplierQuery.FirstOrDefault(x => x.Id == TestDataConsts.SupplierAId);

            // Act
            await _supplierManager.RemoveProductAsync(supplier, TestDataConsts.MedicineParacetamolId);

            // Assert
            supplier.SupplierProducts.Any(x => x.ProductId == TestDataConsts.MedicineParacetamolId).ShouldBeFalse();
        });
    }
    [QATest(scenario: "Toggle sản phẩm hoạt động thành công.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Toggle_Product_Active_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            IQueryable<Supplier> supplierQuery = await _supplierRepository.WithDetailsAsync(x => x.SupplierProducts);
            Supplier? supplier = supplierQuery.FirstOrDefault(x => x.Id == TestDataConsts.SupplierAId);
            SupplierProduct sp = supplier.SupplierProducts.First(x => x.ProductId == TestDataConsts.MedicineParacetamolId);
            bool originalActiveState = sp.IsActive;

            // Act
            _supplierManager.ToggleProductActive(supplier, TestDataConsts.MedicineParacetamolId);

            // Assert
            sp.IsActive.ShouldBe(!originalActiveState);
        });
    }
    [QATest(scenario: "Ném ngoại lệ ngoại lệ khi Thêm sản phẩm với unavailable sản phẩm.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_Exception_When_AddProduct_With_Unavailable_Product()
    {
        // Tách việc insert data ra khỏi UoW chính
        Guid pendingMedId = Guid.Empty;

        await WithUnitOfWorkAsync(async () =>
        {
            Medicine pendingMed = await _medicineManager.CreateAsync(
                "Pending Paracetamol",
                TestDataConsts.CategoryMedicineId,
                TestDataConsts.ManufacturerAId,
                TestDataConsts.UnitBoxId,
                TestDataConsts.DosageTabletId,
                "SDK-PENDING",
                UsageRoute.Oral,
                StorageCondition.Normal,
                false
            );

            IRepository<Product, Guid> productRepository =
                GetRequiredService<IRepository<Product, Guid>>();
            await productRepository.InsertAsync(pendingMed, autoSave: true);

            pendingMedId = pendingMed.Id;
        });

        // UoW riêng để test, tránh nested transaction
        await WithUnitOfWorkAsync(async () =>
        {
            // Dùng FirstOrDefaultAsync thay vì FirstOrDefault trên IQueryable
            IQueryable<Supplier> supplierQuery =
                await _supplierRepository.WithDetailsAsync(x => x.SupplierProducts);
            Supplier? supplier = await _asyncExecuter.FirstOrDefaultAsync(
                supplierQuery, x => x.Id == TestDataConsts.SupplierAId
            );

            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _supplierManager.AddProductAsync(
                    supplier!, pendingMedId, TestDataConsts.UnitBoxId, 5
                );
            });

            ex.Code.ShouldBe("SupplyCoreERP:ProductNotAvailable");
        });
    }
    [QATest(scenario: "Ném ngoại lệ ngoại lệ khi Thêm sản phẩm với non existent đơn vị tính.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_Exception_When_AddProduct_With_NonExistent_Unit()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Supplier supplier = await _supplierManager.CreateAsync(
                "Supplier New For Wrong Unit", "MST-PROD-UNIT-ERR", null, null, null, null, null, null, null, null, null
            );
            Guid invalidUnitId = Guid.NewGuid();

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _supplierManager.AddProductAsync(
                    supplier, TestDataConsts.MedicineParacetamolId, invalidUnitId, 5
                );
            });
            ex.Code.ShouldBe("SupplyCoreERP:UnitNotFound");
        });
    }
    [QATest(scenario: "Xóa nhà cung cấp thành công khi không có dư nợ.", feature: "Supplier", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Delete_Supplier_Successfully()
    {
        Guid supplierId = Guid.Empty;

        // UoW 1: Tạo và lưu vào DB
        await WithUnitOfWorkAsync(async () =>
        {
            Supplier supplier = await _supplierManager.CreateAsync(
                "Supplier For Delete", "SUP-DEL-1", null, null, null, null, null, null, null, null, null
            );
            await _supplierRepository.InsertAsync(supplier, autoSave: true);
            supplierId = supplier.Id;
        });

        // UoW 2: Thực hiện xóa
        await WithUnitOfWorkAsync(async () =>
        {
            await _supplierManager.DeleteAsync(supplierId);
        });

        // UoW 3: Kiểm tra đã xóa thật sự chưa
        await WithUnitOfWorkAsync(async () =>
        {
            Supplier? deletedSupplier = await _supplierRepository.FindAsync(supplierId);
            deletedSupplier.ShouldBeNull();
        });
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi deleting nhà cung cấp với còn dư nợ.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Deleting_Supplier_With_Outstanding_Debt()
    {
        Guid supplierId = Guid.Empty;

        await WithUnitOfWorkAsync(async () =>
        {
            Supplier supplier = await _supplierManager.CreateAsync(
                "Supplier For Delete Debt", "SUP-DEL-DEBT",
                null, null, null, null, null, null, null, null, null,
                1000000m, 30
            );
            supplier.AddDebt(50000m);
            await _supplierRepository.InsertAsync(supplier, autoSave: true);
            supplierId = supplier.Id;
        });

        await WithUnitOfWorkAsync(async () =>
        {
            BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _supplierManager.DeleteAsync(supplierId);
            });
            ex.Code.ShouldBe("SupplyCoreERP:CannotDeleteSupplierWithOutstandingDebt");
        });
    }
    [QATest(scenario: "Cập nhật nhà cung cấp thành công.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Update_Supplier_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Supplier supplier = await _supplierManager.CreateAsync(
                "Supplier For Update", "SUP-UPD-1", null, null, null, null, null, null, null, null, null
            );
            await _supplierRepository.InsertAsync(supplier, autoSave: true);

            // Act
            await _supplierManager.UpdateAsync(
                supplier, "Supplier Updated Name", "123456", "0909999999", "supplier_upd@test.com",
                "New Rep", Gender.Female, "New Note", "123 New Address", null, null, null, 20000000m, 45
            );

            // Assert
            Supplier updatedSupplier = await _supplierRepository.GetAsync(supplier.Id);
            updatedSupplier.Name.ShouldBe("Supplier Updated Name");
            updatedSupplier.TaxCode.ShouldBe("123456");
            updatedSupplier.PhoneNumber.ShouldBe("0909999999");
            updatedSupplier.Email.ShouldBe("supplier_upd@test.com");
            updatedSupplier.RepresentativeName.ShouldBe("New Rep");
            updatedSupplier.Gender.ShouldBe(Gender.Female);
            updatedSupplier.Note.ShouldBe("New Note");
            updatedSupplier.Address.ShouldBe("123 New Address");
            updatedSupplier.DebtLimit.ShouldBe(20000000m);
            updatedSupplier.PaymentTermDays.ShouldBe(45);
        });
    }

    [QATest(scenario: "Check mã code and tên Ném ngoại lệ business ngoại lệ khi tên tồn tại.", feature: "Supplier", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_CheckCodeAndName_Throw_BusinessException_When_Name_Exists()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act & Assert
            BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _supplierManager.CheckCodeAndNameAsync("SUP-002", "Nhà Cung Cấp A");
            });
            ex.Code.ShouldBe("SupplyCoreERP:SupplierNameExists");
        });
    }
}
