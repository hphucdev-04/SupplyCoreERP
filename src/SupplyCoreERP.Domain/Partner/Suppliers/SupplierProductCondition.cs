using System;
using SupplyCoreERP.Catalog.BaseUnits;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SupplyCoreERP.Partner.Suppliers;

/// <summary>
/// Chi tiết bảng giá và điều kiện mua hàng của từng quy cách (đơn vị tính) cho một cặp (Nhà cung cấp - Sản phẩm).
/// </summary>
public class SupplierProductCondition : Entity<Guid>
{
    public Guid SupplierProductId { get; private set; }
    public virtual SupplierProduct SupplierProduct { get; protected set; }

    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; protected set; }

    /// <summary>1 đơn vị mua (UnitId) = N BaseUnit. Ví dụ: 1 Hộp = 50 Viên.</summary>
    public int ConversionFactor { get; private set; }

    public decimal StandardPrice { get; private set; }
    public decimal LastPurchasePrice { get; private set; }

    public decimal MinOrderQuantity { get; private set; }

    protected SupplierProductCondition() { }

    public SupplierProductCondition(
        Guid id,
        Guid supplierProductId,
        Guid unitId,
        int conversionFactor,
        decimal standardPrice,
        decimal minOrderQuantity) : base(id)
    {
        SupplierProductId = supplierProductId;
        UnitId = unitId;

        ConversionFactor = conversionFactor > 0
            ? conversionFactor
            : throw new BusinessException("SupplyCoreERP:InvalidConversionFactor", "Hệ số quy đổi phải lớn hơn 0.");

        StandardPrice = standardPrice >= 0
            ? standardPrice
            : throw new BusinessException("SupplyCoreERP:InvalidStandardPrice", "Giá chuẩn không được âm.");

        LastPurchasePrice = standardPrice;

        MinOrderQuantity = minOrderQuantity > 0
            ? minOrderQuantity
            : throw new BusinessException("SupplyCoreERP:InvalidMinOrderQuantity", "Số lượng đặt tối thiểu phải lớn hơn 0.");
    }

    public void UpdateCondition(
        decimal standardPrice,
        decimal minOrderQuantity)
    {
        StandardPrice = standardPrice >= 0
            ? standardPrice
            : throw new BusinessException("SupplyCoreERP:InvalidStandardPrice", "Giá chuẩn không được âm.");

        MinOrderQuantity = minOrderQuantity > 0
            ? minOrderQuantity
            : throw new BusinessException("SupplyCoreERP:InvalidMinOrderQuantity", "Số lượng đặt tối thiểu phải lớn hơn 0.");
    }

    public void UpdateLastPurchasePrice(decimal price)
    {
        LastPurchasePrice = price >= 0
            ? price
            : throw new BusinessException("SupplyCoreERP:InvalidLastPurchasePrice", "Giá mua gần nhất không được âm.");
    }
}
