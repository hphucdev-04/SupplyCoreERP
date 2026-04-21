using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Products;
using SupplyCoreERP.Suppliers;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Suppliers
{
	/// <summary>
	/// Bảng giá / thông tin mua hàng theo từng cặp (Nhà cung cấp – Sản phẩm).
	/// Tương đương Purchasing Info Record trong SAP.
	/// </summary>
	public class SupplierProduct : FullAuditedEntity<Guid>
	{
		public Guid SupplierId { get; private set; }
		public virtual Supplier Supplier { get; protected set; }

		public Guid ProductId { get; private set; }
		public virtual Product Product { get; protected set; }

		// ── Đơn vị mua của nhà cung cấp (có thể ≠ BaseUnit của sản phẩm)
		public Guid DefaultUnitId { get; private set; }
		public virtual BaseUnit DefaultUnit { get; protected set; }
		public int DefaultConversionFactor { get; private set; }    // 1 Hộp NCC = N BaseUnit

		// ── Giá (không lưu trên Product vì phụ thuộc nhà cung cấp)
		public decimal StandardPrice { get; private set; }          // Giá thỏa thuận
		public decimal LastPurchasePrice { get; private set; }      // Giá mua gần nhất (auto-update khi Complete PO)

		// ── Điều kiện mua
		public int LeadTimeDays { get; private set; }       // Thời gian giao hàng (ngày)
		public decimal MinOrderQuantity { get; private set; }       // MOQ (theo DefaultUnit)
		public decimal OverDeliveryTolerancePct { get; private set; }   // % cho phép giao vượt
		public decimal UnderDeliveryTolerancePct { get; private set; }   // % cho phép giao thiếu

		public bool IsPreferred { get; private set; }             // Nhà cung cấp ưu tiên cho sản phẩm này
		public bool IsActive { get; private set; }
		public string? Note { get; private set; }

		protected SupplierProduct() { }

		public SupplierProduct(
			Guid id,
			Guid supplierId,
			Guid productId,
			Guid defaultUnitId,
			int defaultConversionFactor,
			decimal standardPrice,
			int leadTimeDays,
			decimal minOrderQuantity,
			decimal overDeliveryTolerancePct = 0,
			decimal underDeliveryTolerancePct = 0,
			bool isPreferred = false,
			string? note = null) : base(id)
		{
			SupplierId = supplierId;
			ProductId = productId;
			DefaultUnitId = defaultUnitId;
			DefaultConversionFactor = defaultConversionFactor > 0
				? defaultConversionFactor
				: throw new ArgumentException("Hệ số quy đổi không hợp lệ.");
			StandardPrice = standardPrice >= 0 ? standardPrice : throw new ArgumentException("Giá chuẩn không được âm.");
			LastPurchasePrice = standardPrice;
			LeadTimeDays = Math.Max(0, leadTimeDays);
			MinOrderQuantity = minOrderQuantity > 0 ? minOrderQuantity : 1;
			OverDeliveryTolerancePct = Math.Max(0, overDeliveryTolerancePct);
			UnderDeliveryTolerancePct = Math.Max(0, underDeliveryTolerancePct);
			IsPreferred = isPreferred;
			IsActive = true;
			Note = note;
		}

		public void UpdateInfo(
			Guid defaultUnitId,
			int defaultConversionFactor,
			decimal standardPrice,
			int leadTimeDays,
			decimal minOrderQuantity,
			decimal overDeliveryTolerancePct,
			decimal underDeliveryTolerancePct,
			bool isPreferred,
			string? note)
		{
			DefaultUnitId = defaultUnitId;
			DefaultConversionFactor = defaultConversionFactor > 0
				? defaultConversionFactor
				: throw new ArgumentException("Hệ số quy đổi không hợp lệ.");
			StandardPrice = standardPrice >= 0 ? standardPrice : throw new ArgumentException("Giá chuẩn không được âm.");
			LeadTimeDays = Math.Max(0, leadTimeDays);
			MinOrderQuantity = minOrderQuantity > 0 ? minOrderQuantity : 1;
			OverDeliveryTolerancePct = Math.Max(0, overDeliveryTolerancePct);
			UnderDeliveryTolerancePct = Math.Max(0, underDeliveryTolerancePct);
			IsPreferred = isPreferred;
			Note = note;
		}

		/// <summary>Gọi sau khi hoàn tất PO để cập nhật giá mua gần nhất.</summary>
		public void SyncLastPurchasePrice(decimal actualPrice)
		{
			if (actualPrice >= 0) LastPurchasePrice = actualPrice;
		}

		public void SetActive(bool active) => IsActive = active;
	}
}