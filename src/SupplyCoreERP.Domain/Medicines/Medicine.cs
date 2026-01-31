using SupplyCoreERP.DosageForms;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Products;
using SupplyCoreERP.Locations.Countries;
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

		public Guid OriginCountryId { get; private set; }
		public virtual Country OriginCountry { get; private set; }

		public bool IsActive { get; private set; }
		public string RegistrationNumber { get; private set; }
		public UsageRoute UsageRoute { get; private set; }
		public StorageCondition StorageCondition { get; private set; }
		public MedicineStatus Status { get; private set; }
		public bool IsPrescriptionDrug { get; private set; }

		public virtual ICollection<MedicineIngredient> Ingredients { get; private set; }

		private Medicine() { }

		public Medicine(
			Guid id,
			Guid categoryId,
			Guid manufacturerId,
			Guid originCountryId, 
			string code,
			string name,
			Guid baseUnitId,
			Guid dosageFormId,
			string regNumber
			)
			: base(id, categoryId, manufacturerId, code, name, baseUnitId, ProductType.Medicine)
		{
			IsActive = true;
			Status = MedicineStatus.Pending;
			DosageFormId = dosageFormId;
			OriginCountryId = originCountryId; 
			RegistrationNumber = regNumber;
			Ingredients = new List<MedicineIngredient>();
		}

		public void SetPharmaInfo(UsageRoute route, StorageCondition storage, bool isRx)
		{
			UsageRoute = route;
			StorageCondition = storage;
			IsPrescriptionDrug = isRx;
		}

		public void UpdatePharmaInfo(
			Guid dosageFormId,
			Guid originCountryId, 
			string regNumber,
			UsageRoute route,
			StorageCondition storage,
			bool isRx)
		{
			DosageFormId = dosageFormId;
			OriginCountryId = originCountryId;
			RegistrationNumber = regNumber;
			UsageRoute = route;
			StorageCondition = storage;
			IsPrescriptionDrug = isRx;
		}

		public void AddIngredient(Guid activeIngredientId)
		{
			if (Ingredients.Any(x => x.ActiveIngredientId == activeIngredientId))
			{
				throw new BusinessException("SupplyCoreERP:DuplicateIngredient", "Hoạt chất này đã có trong thuốc.");
			}
			Ingredients.Add(new MedicineIngredient(Id, activeIngredientId));
		}

		public void RemoveIngredient(Guid activeIngredientId)
		{
			var item = Ingredients.FirstOrDefault(x => x.ActiveIngredientId == activeIngredientId);
			if (item != null) Ingredients.Remove(item);
		}

		public void Approve()
		{
			Status = MedicineStatus.Approved;
		}

		public void Reject()
		{
			Status = MedicineStatus.Rejected;
		}

		public void SetActive(bool isActive)
		{
			IsActive = isActive;
		}
	}
}