using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Partner;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Partner.Suppliers;

public interface ISupplierManager : IDomainService
{
    Task<Supplier> CreateAsync(
        string name, string? taxCode, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, string? note,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId,
        decimal debtLimit = 0, int paymentTermDays = 0);

    Task UpdateAsync(
        Supplier supplier,
        string name, string? taxCode, string? phoneNumber, string? email,
        string? representativeName, Gender? gender, string? note,
        string? address, Guid? countryId, Guid? cityId, Guid? areaId,
        decimal debtLimit = 0, int paymentTermDays = 0);

    Task DeleteAsync(Guid id);

    Task<SupplierProduct> AddProductAsync(
        Supplier supplier,
        Guid productId,
        Guid defaultUnitId,
        int leadTimeDays,
        bool isPreferred = false,
        string? note = null);

    Task UpdateProductAsync(
        Supplier supplier,
        Guid productId,
        Guid defaultUnitId,
        int leadTimeDays,
        bool isPreferred,
        string? note);

    Task RemoveProductAsync(Supplier supplier, Guid productId);
    void ToggleProductActive(Supplier supplier, Guid productId);

    Task CheckCodeAndNameAsync(string code, string name, Guid? excludeId = null);
}
