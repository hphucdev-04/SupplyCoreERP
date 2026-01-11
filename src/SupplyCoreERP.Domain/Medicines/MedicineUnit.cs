using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Medicines
{
	public class MedicineUnit : AuditedEntity<Guid>
	{
		public Guid MedicineId { get; private set; }
		public virtual Medicine Medicine { get; private set; }
		public string UnitName { get; private set; }
		public string Barcode { get; private set; }

		public int Level { get; private set; }           
		public int ConversionFactor { get; private set; } 

		public decimal Weight { get; private set; }      
		public decimal Volume { get; private set; }       
		public decimal SalePrice { get; private set; }    

		private MedicineUnit() { }

		internal MedicineUnit(
			Guid id,
			Guid medicineId,
			string unitName,
			int conversionFactor,
			int level,
			decimal salePrice,
			string barcode = null,
			decimal weight = 0,
			decimal volume = 0)
			: base(id)
		{
			MedicineId = medicineId;
			UpdateInternal(unitName, conversionFactor, level, salePrice, barcode, weight, volume);
		}

		internal void UpdateInternal(
			string unitName,
			int conversionFactor,
			int level,
			decimal salePrice,
			string barcode,
			decimal weight,
			decimal volume)
		{
			UnitName = Check.NotNullOrWhiteSpace(unitName, nameof(UnitName), 50);

			// Logic: Tỷ lệ quy đổi phải > 1 (Vì =1 là BaseUnit rồi)
			if (conversionFactor <= 1)
			{
				throw new BusinessException("SupplyCoreERP:InvalidFactor", "Tỷ lệ quy đổi phải lớn hơn 1.");
			}
			ConversionFactor = conversionFactor;

			// Logic: Level phải dương
			if (level <= 0)
			{
				throw new BusinessException("SupplyCoreERP:InvalidLevel", "Cấp độ đơn vị phải lớn hơn 0.");
			}
			Level = level;

			SalePrice = salePrice >= 0 ? salePrice : 0;
			Barcode = barcode;
			Weight = weight >= 0 ? weight : 0;
			Volume = volume >= 0 ? volume : 0;
		}
	}
}