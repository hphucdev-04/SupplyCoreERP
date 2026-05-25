using System;
using System.Linq;
using Shouldly;
using SupplyCoreERP.Enums.Partner;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Partner.Suppliers;

public class Supplier_Unit_Tests
{
    [Fact]
    public void Should_Create_Supplier_With_Valid_Parameters()
    {
        // Arrange & Act
        var id = Guid.NewGuid();
        var supplier = new Supplier(
            id, "SUP-001", "Supplier A", "MST-111", "0901234567", "a@test.com", "Representative Name",
            "Note", "Address", null, null, null, Gender.Male, 500000000m, 30
        );

        // Assert
        supplier.Id.ShouldBe(id);
        supplier.Code.ShouldBe("SUP-001");
        supplier.Name.ShouldBe("Supplier A");
        supplier.TaxCode.ShouldBe("MST-111");
        supplier.PhoneNumber.ShouldBe("0901234567");
        supplier.Email.ShouldBe("a@test.com");
        supplier.RepresentativeName.ShouldBe("Representative Name");
        supplier.Note.ShouldBe("Note");
        supplier.Address.ShouldBe("Address");
        supplier.Gender.ShouldBe(Gender.Male);
        supplier.DebtLimit.ShouldBe(500000000m);
        supplier.PaymentTermDays.ShouldBe(30);
        supplier.CurrentDebt.ShouldBe(0m);
        supplier.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_Update_Supplier_Info()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();

        // Act
        supplier.UpdateInfo("Supplier B", Gender.Female, "MST-222", "0987654321", "b@test.com", "Rep B", "Note B");

        // Assert
        supplier.Name.ShouldBe("Supplier B");
        supplier.Gender.ShouldBe(Gender.Female);
        supplier.TaxCode.ShouldBe("MST-222");
        supplier.PhoneNumber.ShouldBe("0987654321");
        supplier.Email.ShouldBe("b@test.com");
        supplier.RepresentativeName.ShouldBe("Rep B");
        supplier.Note.ShouldBe("Note B");
    }

    [Fact]
    public void Should_Set_Location()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();
        var countryId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        // Act
        supplier.SetLocation("456 Le Loi", countryId, cityId, areaId);

        // Assert
        supplier.Address.ShouldBe("456 Le Loi");
        supplier.CountryId.ShouldBe(countryId);
        supplier.CityId.ShouldBe(cityId);
        supplier.AreaId.ShouldBe(areaId);
    }

    [Fact]
    public void Should_Set_DebtInfo()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();

        // Act
        supplier.SetDebtInfo(1000000000m, 60);

        // Assert
        supplier.DebtLimit.ShouldBe(1000000000m);
        supplier.PaymentTermDays.ShouldBe(60);
    }

    [Fact]
    public void Should_Add_Product_To_Supplier()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();
        var productId = Guid.NewGuid();
        var unitId = Guid.NewGuid();

        // Act
        SupplierProduct sp = supplier.AddProduct(Guid.NewGuid(), productId, unitId, 5, true, "Note SP");

        // Assert
        supplier.SupplierProducts.Count.ShouldBe(1);
        SupplierProduct product = supplier.SupplierProducts.First();
        product.ProductId.ShouldBe(productId);
        product.DefaultUnitId.ShouldBe(unitId);
        product.LeadTimeDays.ShouldBe(5);
        product.IsPreferred.ShouldBeTrue();
    }

    [Fact]
    public void Should_Throw_BusinessException_When_Product_Is_Duplicate()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();
        var productId = Guid.NewGuid();
        supplier.AddProduct(Guid.NewGuid(), productId, Guid.NewGuid(), 5);

        // Act & Assert
        Assert.Throws<BusinessException>(() =>
        {
            supplier.AddProduct(Guid.NewGuid(), productId, Guid.NewGuid(), 5);
        }).Code.ShouldBe("SupplyCoreERP:ProductAlreadyExists");
    }

    [Fact]
    public void Should_Update_SupplierProduct()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();
        var productId = Guid.NewGuid();
        supplier.AddProduct(Guid.NewGuid(), productId, Guid.NewGuid(), 5);

        var newUnitId = Guid.NewGuid();

        // Act
        supplier.UpdateProduct(productId, newUnitId, 10, false, "New note");

        // Assert
        SupplierProduct sp = supplier.SupplierProducts.First();
        sp.DefaultUnitId.ShouldBe(newUnitId);
        sp.LeadTimeDays.ShouldBe(10);
        sp.IsPreferred.ShouldBeFalse();
        sp.Note.ShouldBe("New note");
    }

    [Fact]
    public void Should_Remove_Product()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();
        var productId = Guid.NewGuid();
        supplier.AddProduct(Guid.NewGuid(), productId, Guid.NewGuid(), 5);

        // Act
        supplier.RemoveProduct(productId);

        // Assert
        supplier.SupplierProducts.Count.ShouldBe(0);
    }

    [Fact]
    public void Should_Toggle_Product_Active()
    {
        // Arrange
        Supplier supplier = CreateSampleSupplier();
        var productId = Guid.NewGuid();
        supplier.AddProduct(Guid.NewGuid(), productId, Guid.NewGuid(), 5);

        SupplierProduct sp = supplier.SupplierProducts.First();
        sp.IsActive.ShouldBeTrue();

        // Act
        supplier.ToggleProductActive(productId);

        // Assert
        sp.IsActive.ShouldBeFalse();
    }

    #region AddDebt / PayDebt

    [Fact]
    public void Should_Add_Debt_Successfully()
    {
        Supplier supplier = CreateSampleSupplier();
        supplier.SetDebtInfo(10_000_000m, 30);

        supplier.AddDebt(5_000_000m);

        supplier.CurrentDebt.ShouldBe(5_000_000m);
    }

    [Fact]
    public void Should_Add_Debt_When_No_Limit()
    {
        // DebtLimit = 0 nghĩa là không giới hạn
        Supplier supplier = CreateSampleSupplier();

        supplier.AddDebt(999_999_999m);

        supplier.CurrentDebt.ShouldBe(999_999_999m);
    }

    [Fact]
    public void Should_Throw_When_AddDebt_With_Invalid_Amount()
    {
        Supplier supplier = CreateSampleSupplier();

        Assert.Throws<BusinessException>(() => supplier.AddDebt(0m))
            .Code.ShouldBe("SupplyCoreERP:InvalidDebtAmount");

        Assert.Throws<BusinessException>(() => supplier.AddDebt(-100m))
            .Code.ShouldBe("SupplyCoreERP:InvalidDebtAmount");
    }

    [Fact]
    public void Should_Throw_When_AddDebt_Exceeds_Limit()
    {
        Supplier supplier = CreateSampleSupplier();
        supplier.SetDebtInfo(1_000_000m, 30);
        supplier.AddDebt(800_000m); // CurrentDebt = 800k

        Assert.Throws<BusinessException>(() => supplier.AddDebt(300_000m))
            .Code.ShouldBe("SupplyCoreERP:ExceedsDebtLimit");
    }

    [Fact]
    public void Should_Pay_Debt_Successfully()
    {
        Supplier supplier = CreateSampleSupplier();
        supplier.AddDebt(5_000_000m);

        supplier.PayDebt(2_000_000m);

        supplier.CurrentDebt.ShouldBe(3_000_000m);
    }

    [Fact]
    public void Should_Throw_When_PayDebt_With_Invalid_Amount()
    {
        Supplier supplier = CreateSampleSupplier();
        supplier.AddDebt(1_000_000m);

        Assert.Throws<BusinessException>(() => supplier.PayDebt(0m))
            .Code.ShouldBe("SupplyCoreERP:InvalidPaymentAmount");

        Assert.Throws<BusinessException>(() => supplier.PayDebt(-100m))
            .Code.ShouldBe("SupplyCoreERP:InvalidPaymentAmount");
    }

    #endregion

    #region SetActive / SetDebtInfo edge cases

    [Fact]
    public void Should_Set_Active()
    {
        Supplier supplier = CreateSampleSupplier();
        supplier.IsActive.ShouldBeTrue();

        supplier.SetActive(false);
        supplier.IsActive.ShouldBeFalse();

        supplier.SetActive(true);
        supplier.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_Clamp_Negative_DebtInfo_To_Zero()
    {
        Supplier supplier = CreateSampleSupplier();

        supplier.SetDebtInfo(-500m, -10);

        supplier.DebtLimit.ShouldBe(0m);
        supplier.PaymentTermDays.ShouldBe(0);
    }

    #endregion

    #region Entity-level product error branches

    [Fact]
    public void Should_Throw_When_UpdateProduct_Not_Found()
    {
        Supplier supplier = CreateSampleSupplier();

        Assert.Throws<BusinessException>(() =>
            supplier.UpdateProduct(Guid.NewGuid(), Guid.NewGuid(), 5, false, null))
            .Code.ShouldBe("SupplyCoreERP:ProductNotFound");
    }

    [Fact]
    public void Should_Throw_When_RemoveProduct_Not_Found()
    {
        Supplier supplier = CreateSampleSupplier();

        Assert.Throws<BusinessException>(() => supplier.RemoveProduct(Guid.NewGuid()))
            .Code.ShouldBe("SupplyCoreERP:ProductNotFound");
    }

    [Fact]
    public void Should_Throw_When_ToggleProductActive_Not_Found()
    {
        Supplier supplier = CreateSampleSupplier();

        Assert.Throws<BusinessException>(() => supplier.ToggleProductActive(Guid.NewGuid()))
            .Code.ShouldBe("SupplyCoreERP:ProductNotFound");
    }

    #endregion

    private Supplier CreateSampleSupplier()
    {
        return new Supplier(
            Guid.NewGuid(), "SUP-001", "Supplier A", null, null, null, null, null, null, null, null, null, null
        );
    }
}
