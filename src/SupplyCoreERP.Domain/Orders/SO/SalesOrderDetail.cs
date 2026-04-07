using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Products;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Sales.Orders
{
	public class SalesOrderDetail : FullAuditedEntity<Guid>
	{
		public Guid SalesOrderId { get; private set; }
		public virtual SalesOrder SalesOrder { get; protected set; }

		public Guid ProductId { get; private set; }
		public virtual Product Product { get; protected set; }

		public Guid UnitId { get; private set; }
		public virtual BaseUnit Unit { get; protected set; }

		public int ConversionFactor { get; private set; }
		public decimal Quantity { get; private set; }
		public decimal BaseQuantity => Quantity * ConversionFactor;
		public decimal DeliveredQuantity { get; private set; }

		public decimal UnitPrice { get; private set; }
		public decimal DiscountRate { get; private set; }
		public decimal TaxRate { get; private set; }

		public decimal TotalPrice => Quantity * UnitPrice;
		public decimal DiscountAmount => TotalPrice * (DiscountRate / 100);
		public decimal PriceAfterDiscount => TotalPrice - DiscountAmount;
		public decimal TaxAmount => PriceAfterDiscount * (TaxRate / 100);
		public decimal FinalPrice => PriceAfterDiscount + TaxAmount;

		protected SalesOrderDetail() { }

		public SalesOrderDetail(
			Guid id, Guid salesOrderId, Guid productId, Guid unitId,
			int conversionFactor, decimal quantity, decimal unitPrice,
			decimal discountRate, decimal taxRate) : base(id)
		{
			SalesOrderId = salesOrderId;
			ProductId = productId;
			UnitId = unitId;
			ConversionFactor = conversionFactor > 0 ? conversionFactor : throw new ArgumentException("Hệ số quy đổi không hợp lệ.");
			Quantity = quantity > 0 ? quantity : throw new ArgumentException("Số lượng phải lớn hơn 0.");
			UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("Đơn giá không được âm.");
			DiscountRate = (discountRate >= 0 && discountRate <= 100) ? discountRate : throw new ArgumentException("Chiết khấu từ 0-100%.");
			TaxRate = taxRate >= 0 ? taxRate : throw new ArgumentException("Thuế suất không được âm.");
			DeliveredQuantity = 0;
		}

		public void UpdateInfo(decimal quantity, decimal unitPrice, decimal discountRate, decimal taxRate)
		{
			Quantity = quantity > 0 ? quantity : throw new ArgumentException("Số lượng phải lớn hơn 0.");
			UnitPrice = unitPrice >= 0 ? unitPrice : throw new ArgumentException("Đơn giá không được âm.");
			DiscountRate = (discountRate >= 0 && discountRate <= 100) ? discountRate : throw new ArgumentException("Chiết khấu từ 0-100%.");
			TaxRate = taxRate >= 0 ? taxRate : throw new ArgumentException("Thuế suất không được âm.");
		}

		public void AddDeliveredQuantity(decimal qty) => DeliveredQuantity += qty;
	}
}