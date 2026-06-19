using System;
using SupplyCoreERP.Catalog.BaseUnits;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Catalog.Products;

public class ProductUnit : AuditedEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; private set; }
    public Guid UnitId { get; private set; }
    public virtual BaseUnit Unit { get; private set; }
    public int ConversionFactor { get; private set; }
    public int Level { get; private set; }
    public decimal Volume { get; private set; }

    private ProductUnit() { }

    internal ProductUnit(
        Guid id,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        int level,
        decimal volume = 0)
        : base(id)
    {
        ProductId = productId;
        Volume = volume;
        UpdateInternal(unitId, conversionFactor, level, volume);
    }

    internal void UpdateInternal(Guid unitId, int conversionFactor, int level, decimal volume)
    {
        if (unitId == Guid.Empty)
        {
            throw new BusinessException("SupplyCoreERP:InvalidUnit", "Đơn vị tính không hợp lệ.");
        }

        UnitId = unitId;
        Volume = volume;
        SetFactorAndLevel(conversionFactor, level);
    }
    internal void UpdateStats(int conversionFactor, int level, decimal volume)
    {
        SetFactorAndLevel(conversionFactor, level);
        Volume = volume;
    }

    private void SetFactorAndLevel(int conversionFactor, int level)
    {
        if (conversionFactor <= 1 && level > 1)
        {
            throw new BusinessException("SupplyCoreERP:InvalidFactor", "Tỷ lệ quy đổi phải lớn hơn 1.");
        }

        ConversionFactor = conversionFactor;
        Level = level;
    }
}







