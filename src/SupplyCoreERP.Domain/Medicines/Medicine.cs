using SupplyCoreERP.Categories;
using SupplyCoreERP.Enums.Medicines;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Medicines
{
	public class Medicine : FullAuditedAggregateRoot<Guid>
	{
		public Guid CategoryId { get; private set; }
		public virtual Category Category { get; private set; }
		public string Code { get; private set; }
		public string Name { get; private set; }
		public string RegistrationNumber { get; private set; }
		public string Barcode { get; private set; }

		public string ActiveIngredients { get; private set; }
		public string Concentration { get; private set; }
		public string DosageForm { get; private set; }
		public UsageRoute UsageRoute { get; private set; }

		public ProductType ProductType { get; private set; }
		public StorageCondition StorageCondition { get; private set; }
		public bool IsPrescriptionDrug { get; private set; }

		public string BaseUnit { get; private set; }
		public virtual ICollection<MedicineUnit> Units { get; private set; }

		private Medicine() { }

		public Medicine(
			Guid id,
			Guid categoryId,
			string code,
			string name,
			string baseUnit,
			ProductType productType)
			: base(id)
		{
			CategoryId = categoryId;
			SetCode(code);
			SetName(name);
			BaseUnit = Check.NotNullOrWhiteSpace(baseUnit, nameof(BaseUnit), 50);
			ProductType = productType;

			Units = new List<MedicineUnit>();
		}

		public void SetName(string name)
		{
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255).Trim();
		}

		public void SetCode(string code)
		{
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50).Trim().ToUpper();
		}

		public void SetBarcode(string barcode) { Barcode = barcode?.Trim(); }

		public void SetCategory(Guid categoryId) { CategoryId = categoryId; }

		public void SetPharmaInfo(string activeIngredients, string concentration, string dosageForm, string regNumber, UsageRoute route)
		{
			ActiveIngredients = activeIngredients;
			Concentration = concentration;
			DosageForm = dosageForm;
			RegistrationNumber = regNumber;
			UsageRoute = route;
		}

		public void SetStorageInfo(StorageCondition condition, bool isRx)
		{
			StorageCondition = condition;
			IsPrescriptionDrug = isRx;
		}

		public void AddUnit(Guid id, string unitName, int conversionFactor, int level, decimal salePrice, string barcode = null, decimal weight = 0, decimal volume = 0)
		{
			// Check 1: Không trùng với BaseUnit
			if (unitName.Equals(BaseUnit, StringComparison.OrdinalIgnoreCase))
			{
				throw new BusinessException("PharmaERP:DuplicateUnit", $"Đơn vị quy đổi '{unitName}' trùng với Đơn vị cơ bản.");
			}

			// Check 2: Không trùng với các đơn vị đã có
			if (Units.Any(u => u.UnitName.Equals(unitName, StringComparison.OrdinalIgnoreCase)))
			{
				throw new BusinessException("PharmaERP:DuplicateUnit", $"Đơn vị '{unitName}' đã tồn tại.");
			}

			Units.Add(new MedicineUnit(id, Id, unitName, conversionFactor, level, salePrice, barcode, weight, volume));
		}

		public void UpdateUnit(Guid unitId, string unitName, int conversionFactor, int level, decimal salePrice, string barcode = null, decimal weight = 0, decimal volume = 0)
		{
			var unit = Units.FirstOrDefault(x => x.Id == unitId);
			if (unit == null)
			{
				throw new BusinessException("SupplyCoreERP:UnitNotFound", "Không tìm thấy đơn vị quy đổi này.");
			}

			// Check trùng tên 
			if (Units.Any(u => u.Id != unitId && u.UnitName.Equals(unitName, StringComparison.OrdinalIgnoreCase)))
			{
				throw new BusinessException("SupplyCoreERP:DuplicateUnit", $"Đơn vị '{unitName}' đã tồn tại.");
			}
			// Check trùng với BaseUnit
			if (unitName.Equals(BaseUnit, StringComparison.OrdinalIgnoreCase))
			{
				throw new BusinessException("SupplyCoreERP:DuplicateUnit", $"Đơn vị quy đổi '{unitName}' trùng với Đơn vị cơ bản.");
			}

			unit.UpdateInternal(unitName, conversionFactor, level, salePrice, barcode, weight, volume);
		}

		public void RemoveUnit(Guid unitId)
		{
			var unit = Units.FirstOrDefault(x => x.Id == unitId);
			if (unit == null) return;
			Units.Remove(unit);
		}
	}
}