using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Categories;
using SupplyCoreERP.Enums.Products;
using SupplyCoreERP.Manufacturers;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Products
{
	public abstract class Product : FullAuditedAggregateRoot<Guid>
	{
		public Guid CategoryId { get; protected set; }
		public virtual Category Category { get; protected set; }
		public Guid ManufacturerId { get; protected set; }
		public virtual Manufacturer Manufacturer { get; protected set; }
		public string Code { get; protected set; }
		public string Name { get; protected set; }
		public Guid BaseUnitId { get; protected set; } // Đơn vị gốc (Cái/Viên)
		public virtual BaseUnit BaseUnit { get; protected set; }
		public ProductType ProductType { get; protected set; } 
		public virtual ICollection<ProductUnit> Units { get; protected set; }

		protected Product() { }

		protected Product(
			Guid id,
			Guid categoryId,
			Guid manufacturerId,
			string code,
			string name,
			Guid baseUnitId, 
			ProductType productType)
			: base(id)
		{
			CategoryId = categoryId;
			ManufacturerId = manufacturerId;
			SetCode(code);
			SetName(name);
			BaseUnitId = baseUnitId;
			ProductType = productType;
			Units = new List<ProductUnit>();
		}

		private void SetName(string name) 
		{ 
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255).Trim(); 
		}

		private void SetCode(string code) 
		{ 
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50).Trim().ToUpper(); 
		}

		public void SetCategory(Guid id) 
		{ 
			CategoryId = id; 
		}

		public void SetManufacturer(Guid id) 
		{ 
			ManufacturerId = id; 
		}

		public void UpdateInfo(string name, Guid categoryId, Guid manufacturerId)
		{
			SetName(name);
			CategoryId = categoryId;
			ManufacturerId = manufacturerId;
		}

		public void UpdateCode(string newCode)
		{
			SetCode(newCode);
		}

		public void AddUnit(Guid id, Guid unitId, int conversionFactor, int level)
		{
			if (unitId == BaseUnitId)
				throw new BusinessException("SupplyCoreERP:Error", "Không được thêm đơn vị trùng với Đơn vị gốc.");

			if (Units.Any(u => u.UnitId == unitId))
				throw new BusinessException("SupplyCoreERP:Error", "Đơn vị quy đổi này đã tồn tại.");

			if (conversionFactor <= 1)
				throw new BusinessException("SupplyCoreERP:Error", "Hệ số quy đổi phải lớn hơn 1.");

			if (level < 1)
				throw new BusinessException("SupplyCoreERP:Error", "Cấp độ quy đổi (Level) phải từ 1 trở lên.");

			Units.Add(new ProductUnit(id, Id, unitId, conversionFactor, level));
		}

		public void UpdateUnit(Guid unitId, int conversionFactor, int level)
		{
			var unit = Units.FirstOrDefault(u => u.UnitId == unitId);
			if (unit == null) throw new UserFriendlyException("Đơn vị không tồn tại.");

			unit.UpdateStats(conversionFactor, level);
		}

		public void RemoveUnit(Guid unitId)
		{
			var unit = Units.FirstOrDefault(u => u.UnitId == unitId);
			if (unit != null) Units.Remove(unit);
		}

	}
}