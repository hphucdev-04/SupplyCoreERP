using System;
using Shouldly;
using SupplyCoreERP.Enums.Partner;
using Volo.Abp;
using Xunit;

namespace SupplyCoreERP.Partner.Customers;

public class Customer_Unit_Tests
{
    [Fact]
    public void Should_Create_Customer_With_Valid_Parameters()
    {
        // Arrange & Act
        var id = Guid.NewGuid();
        var priceListId = Guid.NewGuid();
        var customer = new Customer(
            id, "CUS-001", "Customer A", "0901234567", "c@test.com", "Representative Name", Gender.Female,
            CustomerType.Individual, "MST-CUS", "123 Ly Thuong Kiet", null, null, null, "Note", 300000000m, 30, priceListId
        );

        // Assert
        customer.Id.ShouldBe(id);
        customer.Code.ShouldBe("CUS-001");
        customer.Name.ShouldBe("Customer A");
        customer.PhoneNumber.ShouldBe("0901234567");
        customer.Email.ShouldBe("c@test.com");
        customer.RepresentativeName.ShouldBe("Representative Name");
        customer.Gender.ShouldBe(Gender.Female);
        customer.Type.ShouldBe(CustomerType.Individual);
        customer.TaxCode.ShouldBe("MST-CUS");
        customer.Address.ShouldBe("123 Ly Thuong Kiet");
        customer.Note.ShouldBe("Note");
        customer.DebtLimit.ShouldBe(300000000m);
        customer.PaymentTermDays.ShouldBe(30);
        customer.PriceListId.ShouldBe(priceListId);
        customer.IsActive.ShouldBeTrue();
        customer.CurrentDebt.ShouldBe(0m);
    }

    [Fact]
    public void Should_Update_Customer_Info()
    {
        // Arrange
        Customer customer = CreateSampleCustomer();

        // Act
        customer.UpdateInfo(
            "Customer B", "0987654321", "d@test.com", "Rep B", Gender.Male,
            CustomerType.Organization, "MST-CUS-2", "New Note"
        );

        // Assert
        customer.Name.ShouldBe("Customer B");
        customer.PhoneNumber.ShouldBe("0987654321");
        customer.Email.ShouldBe("d@test.com");
        customer.RepresentativeName.ShouldBe("Rep B");
        customer.Gender.ShouldBe(Gender.Male);
        customer.Type.ShouldBe(CustomerType.Organization);
        customer.TaxCode.ShouldBe("MST-CUS-2");
        customer.Note.ShouldBe("New Note");
    }

    [Fact]
    public void Should_Set_Location()
    {
        // Arrange
        Customer customer = CreateSampleCustomer();
        var countryId = Guid.NewGuid();
        var cityId = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        // Act
        customer.SetLocation("456 Tran Hung Dao", countryId, cityId, areaId);

        // Assert
        customer.Address.ShouldBe("456 Tran Hung Dao");
        customer.CountryId.ShouldBe(countryId);
        customer.CityId.ShouldBe(cityId);
        customer.AreaId.ShouldBe(areaId);
    }

    [Fact]
    public void Should_Set_DebtInfo()
    {
        // Arrange
        Customer customer = CreateSampleCustomer();

        // Act
        customer.SetDebtInfo(600000000m, 45);

        // Assert
        customer.DebtLimit.ShouldBe(600000000m);
        customer.PaymentTermDays.ShouldBe(45);
    }

    [Fact]
    public void Should_Set_PriceList()
    {
        // Arrange
        Customer customer = CreateSampleCustomer();
        var newPriceListId = Guid.NewGuid();

        // Act
        customer.SetPriceList(newPriceListId);

        // Assert
        customer.PriceListId.ShouldBe(newPriceListId);
    }

    #region AddDebt / PayDebt

    [Fact]
    public void Should_Add_Debt_Successfully()
    {
        Customer customer = CreateSampleCustomer();
        customer.SetDebtInfo(10_000_000m, 30);

        customer.AddDebt(5_000_000m);

        customer.CurrentDebt.ShouldBe(5_000_000m);
    }

    [Fact]
    public void Should_Add_Debt_When_No_Limit()
    {
        // DebtLimit = 0 nghĩa là không giới hạn
        Customer customer = CreateSampleCustomer();

        customer.AddDebt(999_999_999m);

        customer.CurrentDebt.ShouldBe(999_999_999m);
    }

    [Fact]
    public void Should_Throw_When_AddDebt_With_Invalid_Amount()
    {
        Customer customer = CreateSampleCustomer();

        Assert.Throws<BusinessException>(() => customer.AddDebt(0m))
            .Code.ShouldBe("SupplyCoreERP:InvalidDebtAmount");

        Assert.Throws<BusinessException>(() => customer.AddDebt(-100m))
            .Code.ShouldBe("SupplyCoreERP:InvalidDebtAmount");
    }

    [Fact]
    public void Should_Throw_When_AddDebt_Exceeds_Credit_Limit()
    {
        Customer customer = CreateSampleCustomer();
        customer.SetDebtInfo(1_000_000m, 30);
        customer.AddDebt(800_000m);

        Assert.Throws<BusinessException>(() => customer.AddDebt(300_000m))
            .Code.ShouldBe("SupplyCoreERP:ExceedsCreditLimit");
    }

    [Fact]
    public void Should_Pay_Debt_Successfully()
    {
        Customer customer = CreateSampleCustomer();
        customer.AddDebt(5_000_000m);

        customer.PayDebt(2_000_000m);

        customer.CurrentDebt.ShouldBe(3_000_000m);
    }

    [Fact]
    public void Should_Throw_When_PayDebt_With_Invalid_Amount()
    {
        Customer customer = CreateSampleCustomer();
        customer.AddDebt(1_000_000m);

        Assert.Throws<BusinessException>(() => customer.PayDebt(0m))
            .Code.ShouldBe("SupplyCoreERP:InvalidPaymentAmount");

        Assert.Throws<BusinessException>(() => customer.PayDebt(-100m))
            .Code.ShouldBe("SupplyCoreERP:InvalidPaymentAmount");
    }

    #endregion

    #region SetActive / SetDebtInfo edge cases

    [Fact]
    public void Should_Set_Active()
    {
        Customer customer = CreateSampleCustomer();
        customer.IsActive.ShouldBeTrue();

        customer.SetActive(false);
        customer.IsActive.ShouldBeFalse();

        customer.SetActive(true);
        customer.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_Clamp_Negative_DebtInfo_To_Zero()
    {
        Customer customer = CreateSampleCustomer();

        customer.SetDebtInfo(-500m, -10);

        customer.DebtLimit.ShouldBe(0m);
        customer.PaymentTermDays.ShouldBe(0);
    }

    #endregion

    private Customer CreateSampleCustomer()
    {
        return new Customer(
            Guid.NewGuid(), "CUS-001", "Customer A", null, null, null, null, CustomerType.Individual, null, null, null, null, null, null
        );
    }
}
