using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Products;
using SupplyCoreERP.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;

namespace SupplyCoreERP.Medicines
{
    public class Medicine : Product
    {
        public Guid DosageFormId { get; private set; }
        public virtual DosageForm DosageForm { get; private set; }

        public bool IsActive { get; private set; }
        public string RegistrationNumber { get; private set; }
        public UsageRoute UsageRoute { get; private set; }
        public StorageCondition StorageCondition { get; private set; }
        public MedicineStatus Status { get; private set; }
        public bool IsPrescriptionDrug { get; private set; }

        public virtual ICollection<MedicineIngredient> Ingredients { get; private set; }

        /// <summary>
        /// Thuốc chỉ được nhập/xuất kho khi đã được duyệt (Approved) và đang hoạt động.
        /// </summary>
        public override bool IsAvailableForInventory => IsActive && Status == MedicineStatus.Approved;
        public override StorageCondition? RequiredStorageCondition => StorageCondition;

        private Medicine() { }

        public Medicine(
            Guid id,
            Guid categoryId,
            Guid manufacturerId,
            string code,
            string name,
            Guid baseUnitId,
            Guid dosageFormId,
            string regNumber,
            UsageRoute usageRoute,
            StorageCondition storageCondition,
            bool isPrescriptionDrug)
            : base(id, categoryId, manufacturerId, code, name, baseUnitId, ProductType.Medicine)
        {
            IsActive = true;
            Status = MedicineStatus.Pending;
            DosageFormId = dosageFormId;
            RegistrationNumber = regNumber;
            UsageRoute = usageRoute;
            StorageCondition = storageCondition;
            IsPrescriptionDrug = isPrescriptionDrug;
            Ingredients = new List<MedicineIngredient>();
        }

        public void UpdatePharmaInfo(
            Guid dosageFormId,
            string regNumber,
            UsageRoute usageRoute,
            StorageCondition storageCondition,
            bool isPrescriptionDrug)
        {
            DosageFormId = dosageFormId;
            RegistrationNumber = regNumber;
            UsageRoute = usageRoute;
            StorageCondition = storageCondition;
            IsPrescriptionDrug = isPrescriptionDrug;
        }

        public void AddIngredient(Guid activeIngredientId)
        {
            if (Ingredients.Any(x => x.ActiveIngredientId == activeIngredientId))
                throw new BusinessException("SupplyCoreERP:DuplicateIngredient", "Hoạt chất này đã có trong thuốc.");
            Ingredients.Add(new MedicineIngredient(Id, activeIngredientId));
        }

        public void RemoveIngredient(Guid activeIngredientId)
        {
            MedicineIngredient? item = Ingredients.FirstOrDefault(x => x.ActiveIngredientId == activeIngredientId);
            if (item != null) Ingredients.Remove(item);
        }

        public void Approve() => Status = MedicineStatus.Approved;
        public void Reject() => Status = MedicineStatus.Rejected;
        public void Penđing() => Status = MedicineStatus.Pending;
        public void SetActive(bool isActive) => IsActive = isActive;
        public void SetStatus(MedicineStatus status) => Status = status;
    }
}