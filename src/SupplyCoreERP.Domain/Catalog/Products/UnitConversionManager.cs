using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Catalog.Products;

public class UnitConversionManager : DomainService
{
    /// <summary>
    /// Quy đổi từ một đơn vị tính phụ (sourceUnit) sang đơn vị tính gốc (BaseUnit) của sản phẩm.
    /// Phép tính: BaseQuantity = SourceQuantity * ConversionFactor
    /// </summary>
    public virtual decimal ConvertToBaseQuantity(Product product, Guid sourceUnitId, decimal quantity)
    {
        Check.NotNull(product, nameof(product));

        if (sourceUnitId == Guid.Empty)
        {
            throw new BusinessException("SupplyCoreERP:InvalidUnitId", "Đơn vị tính nguồn không hợp lệ.");
        }

        if (sourceUnitId == product.BaseUnitId)
        {
            return quantity;
        }

        return quantity * GetAbsoluteConversionFactor(product, sourceUnitId);
    }

    /// <summary>
    /// Quy đổi từ số lượng Đơn vị gốc (BaseUnit) sang một Đơn vị phụ đích.
    /// Phép tính: TargetQuantity = BaseQuantity / ConversionFactor
    /// </summary>
    public virtual decimal ConvertFromBaseQuantity(Product product, Guid targetUnitId, decimal baseQuantity, int decimals = 4)
    {
        Check.NotNull(product, nameof(product));

        if (targetUnitId == Guid.Empty)
        {
            throw new BusinessException("SupplyCoreERP:InvalidUnitId", "Đơn vị tính đích không hợp lệ.");
        }

        if (targetUnitId == product.BaseUnitId)
        {
            return baseQuantity;
        }

        decimal rawResult = baseQuantity / GetAbsoluteConversionFactor(product, targetUnitId);

        // Làm tròn kết quả đến số thập phân mong muốn, sử dụng MidpointRounding.AwayFromZero để tránh làm tròn xuống khi phần thập phân là .5
        return Math.Round(rawResult, decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Quy đổi chéo giữa hai đơn vị tính bất kỳ của sản phẩm.
    /// Phép tính: BaseQuantity = SourceQuantity * SourceConversionFactor, sau đó TargetQuantity = BaseQuantity / TargetConversionFactor
    /// </summary>
    public virtual decimal ConvertBetweenUnits(Product product, Guid sourceUnitId, Guid targetUnitId, decimal quantity, int decimals = 4)
    {
        Check.NotNull(product, nameof(product));

        decimal baseQty = ConvertToBaseQuantity(product, sourceUnitId, quantity);
        return ConvertFromBaseQuantity(product, targetUnitId, baseQty, decimals);
    }

    /// <summary>
    /// Lấy hệ số quy đổi (ConversionFactor) của một đơn vị so với Đơn vị gốc.
    /// </summary>
    public virtual int GetConversionFactor(Product product, Guid unitId)
    {
        Check.NotNull(product, nameof(product));

        return GetAbsoluteConversionFactor(product, unitId);
    }

    /// <summary>
    /// Lấy hệ số quy đổi tuyệt đối từ một đơn vị tính bất kỳ về Đơn vị gốc của sản phẩm.
    /// </summary>
    protected virtual int GetAbsoluteConversionFactor(Product product, Guid unitId)
    {
        Check.NotNull(product, nameof(product));

        if (unitId == product.BaseUnitId)
        {
            return 1;
        }

        ProductUnit? targetUnit = product.Units.FirstOrDefault(u => u.UnitId == unitId);
        if (targetUnit == null)
        {
            throw new BusinessException(
                "SupplyCoreERP:UnitNotFound",
                $"Đơn vị tính với Id '{unitId}' không thuộc danh sách quy đổi của sản phẩm '{product.Name}'."
            );
        }

        List<ProductUnit> sortedUnits = product.Units.OrderBy(u => u.Level).ToList();

        int absoluteFactor = 1;
        foreach (ProductUnit? u in sortedUnits)
        {
            absoluteFactor *= u.ConversionFactor;
            if (u.UnitId == unitId)
            {
                break;
            }
        }

        return absoluteFactor;
    }
}







