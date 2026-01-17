using SupplyCoreERP.BaseUnits;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Products
{
	public class ProductUnit : AuditedEntity<Guid>
	{
		public Guid ProductId { get; private set; }
		public virtual Product Product { get; private set; }
		public Guid UnitId { get; private set; }
		public virtual BaseUnit Unit { get; private set; }
		public int ConversionFactor { get; private set; }
		public int Level { get; private set; }
		public decimal SalePrice { get; private set; }

		private ProductUnit() { }

		internal ProductUnit(
			Guid id,
			Guid productId,
			Guid unitId, 
			int conversionFactor,
			int level,
			decimal salePrice,
			string? barcode = null)
			: base(id)
		{
			ProductId = productId;
			UnitId = unitId;
			UpdateInternal(unitId, conversionFactor, level, salePrice, barcode);
		}

		internal void UpdateInternal(Guid unitId, int conversionFactor, int level, decimal salePrice, string? barcode)
		{
			if (unitId == Guid.Empty)
				throw new BusinessException("SupplyCoreERP:InvalidUnit", "Đơn vị tính không hợp lệ.");

			if (conversionFactor <= 1 && level > 1)
				throw new BusinessException("SupplyCoreERP:InvalidFactor", "Tỷ lệ quy đổi phải lớn hơn 1.");

			UnitId = unitId;
			ConversionFactor = conversionFactor;
			Level = level;
			SalePrice = salePrice >= 0 ? salePrice : 0;
		}
	}
}