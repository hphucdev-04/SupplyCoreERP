using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Catalog.DosageForms;
using SupplyCoreERP.Catalog.Medicines.Events;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Products;
using Volo.Abp;

namespace SupplyCoreERP.Catalog.Medicines;

public class Medicine : Product
{
    public Guid DosageFormId { get; private set; }
    public virtual DosageForm DosageForm { get; private set; }

    public bool IsActive { get; private set; }
    public UsageRoute UsageRoute { get; private set; }
    public StorageCondition StorageCondition { get; private set; }
    public MedicineStatus Status { get; private set; }
    public bool IsPrescriptionDrug { get; private set; }

    public virtual ICollection<MedicineIngredient> Ingredients { get; private set; } = new List<MedicineIngredient>();
    public virtual ICollection<MedicineRegistration> Registrations { get; private set; } = new List<MedicineRegistration>();


    public override bool IsAvailableForInventory => IsActive && Status == MedicineStatus.Approved;
    public override StorageCondition? RequiredStorageCondition => StorageCondition;

    protected Medicine()
    {

    }

    public Medicine(
        Guid id,
        Guid categoryId,
        Guid manufacturerId,
        string code,
        string name,
        Guid baseUnitId,
        Guid dosageFormId,
        string initialRegNumber,
        UsageRoute usageRoute,
        StorageCondition storageCondition,
        bool isPrescriptionDrug,
        decimal baseUnitVolume = 0)
        : base(id, categoryId, manufacturerId, code, name, baseUnitId, ProductType.Medicine, baseUnitVolume)
    {
        IsActive = true;
        Status = MedicineStatus.Pending;
        DosageFormId = dosageFormId;
        UsageRoute = usageRoute;
        StorageCondition = storageCondition;
        IsPrescriptionDrug = isPrescriptionDrug;

        AddRegistration(Guid.NewGuid(), initialRegNumber);
    }

    public void RaiseCreatedEvent()
    {
        AddLocalEvent(new MedicineCreatedDomainEvent(Id, Name, Code));
    }

    public void UpdatePharmaInfo(
        Guid dosageFormId,
        UsageRoute usageRoute,
        StorageCondition storageCondition,
        bool isPrescriptionDrug)
    {
        DosageFormId = dosageFormId;
        UsageRoute = usageRoute;
        StorageCondition = storageCondition;
        IsPrescriptionDrug = isPrescriptionDrug;
    }

    public void AddRegistration(Guid id, string regNumber, DateTime? from = null, DateTime? to = null, string? note = null)
    {
        if (Registrations.Any(r => r.RegistrationNumber == regNumber.Trim().ToUpper()))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateRegistration", "SĐK đã tồn tại cho thuốc này.");
        }

        foreach (MedicineRegistration reg in Registrations)
        {
            reg.SetActive(false);
        }

        Registrations.Add(new MedicineRegistration(id, Id, regNumber, from, to, true, note));
    }

    public MedicineRegistration? GetCurrentRegistration()
    {
        return Registrations.FirstOrDefault(r => r.IsActive)
               ?? Registrations.OrderByDescending(r => r.CreationTime).FirstOrDefault();
    }

    public void AddIngredient(Guid activeIngredientId, string? strength = null)
    {
        if (Ingredients.Any(x => x.ActiveIngredientId == activeIngredientId))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateIngredient", "Hoạt chất này đã có trong thuốc.");
        }

        Ingredients.Add(new MedicineIngredient(Id, activeIngredientId, strength));
    }

    public void UpdateIngredientStrength(Guid activeIngredientId, string? strength)
    {
        MedicineIngredient? item = Ingredients.FirstOrDefault(x => x.ActiveIngredientId == activeIngredientId);
        if (item == null)
        {
            throw new BusinessException("SupplyCoreERP:IngredientNotFound", "Hoạt chất không tồn tại trong thuốc.");
        }

        item.UpdateStrength(strength);
    }

    public void RemoveIngredient(Guid activeIngredientId)
    {
        MedicineIngredient? item = Ingredients.FirstOrDefault(x => x.ActiveIngredientId == activeIngredientId);
        if (item != null)
        {
            Ingredients.Remove(item);
        }
    }

    public void Approve()
    {
        if (Status != MedicineStatus.Pending)
        {
            throw new BusinessException("SupplyCoreERP:InvalidMedicineStatus", "Chỉ thuốc đang ở trạng thái Pending mới có thể được duyệt.");
        }

        Status = MedicineStatus.Approved;
        AddLocalEvent(new MedicineStatusChangedDomainEvent(Id, Name, Code, Status));
    }
    public void Reject()
    {
        if (Status != MedicineStatus.Pending)
        {
            throw new BusinessException("SupplyCoreERP:InvalidMedicineStatus", "Chỉ thuốc đang ở trạng thái Pending mới có thể bị từ chối.");
        }

        Status = MedicineStatus.Rejected;
        AddLocalEvent(new MedicineStatusChangedDomainEvent(Id, Name, Code, Status));
    }
    public void SetPending() => Status = MedicineStatus.Pending;
    public void SetActive(bool isActive) => IsActive = isActive;
    public void SetStatus(MedicineStatus status) => Status = status;
}







