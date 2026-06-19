using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Medicines;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Catalog.Medicines;

public interface IMedicineManager : IDomainService
{
    Task<Medicine> CreateAsync(
        string name,
        Guid categoryId,
        Guid manufacturerId,
        Guid baseUnitId,
        Guid dosageFormId,
        string regNumber,
        UsageRoute usageRoute,
        StorageCondition storageCondition,
        bool isPrescriptionDrug,
        decimal baseUnitVolume = 0,
        DateTime? regValidFrom = null,
        DateTime? regValidTo = null,
        string? regNote = null,
        bool raiseEvent = true);

    Task UpdateAsync(
        Medicine medicine,
        string name,
        Guid categoryId,
        Guid manufacturerId,
        Guid baseUnitId,
        Guid dosageFormId,
        string regNumber,
        UsageRoute usageRoute,
        StorageCondition storageCondition,
        bool isPrescriptionDrug,
        decimal baseUnitVolume,
        DateTime? regValidFrom = null,
        DateTime? regValidTo = null,
        string? regNote = null);

    Task AddIngredientAsync(Medicine medicine, Guid activeIngredientId);
    Task RemoveIngredientAsync(Medicine medicine, Guid activeIngredientId);

    Task AddUnitAsync(Medicine medicine, Guid unitId, int conversionFactor, int level, decimal volume = 0);
    Task UpdateUnitAsync(Medicine medicine, Guid unitId, int conversionFactor, int level, decimal volume = 0);
    Task RemoveUnitAsync(Medicine medicine, Guid unitId);

    Task AddRegistrationAsync(
        Medicine medicine,
        string regNumber,
        DateTime? validFrom = null,
        DateTime? validTo = null,
        string? regNote = null);
}
