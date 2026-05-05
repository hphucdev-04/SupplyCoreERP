using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Products;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Suppliers
{
    /// <summary>
    /// Bảng giá / thông tin mua hàng theo từng cặp (Nhà cung cấp – Sản phẩm).
    /// Tương đương Purchasing Info Record trong SAP.
    /// </summary>
    public class SupplierProduct : AuditedEntity<Guid>
    {
        public Guid SupplierId { get; private set; }
        public virtual Supplier Supplier { get; protected set; }

        public Guid ProductId { get; private set; }
        public virtual Product Product { get; protected set; }

        // ── Đơn vị mua của nhà cung cấp (có thể ≠ BaseUnit của sản phẩm)
        public Guid DefaultUnitId { get; private set; }
        public virtual BaseUnit DefaultUnit { get; protected set; }

        /// <summary>1 đơn vị mua (DefaultUnit) = N BaseUnit. Ví dụ: 1 Hộp = 50 Viên.</summary>
        public int DefaultConversionFactor { get; private set; }

        // ── Giá
        public decimal StandardPrice { get; private set; }
        public decimal LastPurchasePrice { get; private set; }

        // ── Điều kiện mua
        public int LeadTimeDays { get; private set; }
        public decimal MinOrderQuantity { get; private set; }
        public decimal OverDeliveryTolerancePct { get; private set; }
        public decimal UnderDeliveryTolerancePct { get; private set; }

        public bool IsPreferred { get; private set; }
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
            StandardPrice = standardPrice >= 0
                ? standardPrice
                : throw new ArgumentException("Giá chuẩn không được âm.");
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
            StandardPrice = standardPrice >= 0
                ? standardPrice
                : throw new ArgumentException("Giá chuẩn không được âm.");
            LeadTimeDays = Math.Max(0, leadTimeDays);
            MinOrderQuantity = minOrderQuantity > 0 ? minOrderQuantity : 1;
            OverDeliveryTolerancePct = Math.Max(0, overDeliveryTolerancePct);
            UnderDeliveryTolerancePct = Math.Max(0, underDeliveryTolerancePct);
            IsPreferred = isPreferred;
            Note = note;
        }
        public void SetActive(bool active) => IsActive = active;
    }
}