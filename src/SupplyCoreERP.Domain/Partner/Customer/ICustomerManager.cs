using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Partner;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Partner.Customers;

public interface ICustomerManager : IDomainService
{
    Task<Customer> CreateAsync(
        string name, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, CustomerType type, string? taxCode,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId, string? note,
        decimal debtLimit = 0, int paymentTermDays = 0, Guid? priceListId = null);

    Task UpdateAsync(
        Customer customer, string name, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, CustomerType type, string? taxCode,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId, string? note,
        decimal debtLimit = 0, int paymentTermDays = 0, Guid? priceListId = null);

    Task DeleteAsync(Guid id);

    Task CheckCodeAndNameAsync(string code, string name, Guid? excludeId = null);
}
