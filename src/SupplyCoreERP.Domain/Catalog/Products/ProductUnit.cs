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

    private ProductUnit() { }

    internal ProductUnit(
        Guid id,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        int level)
        : base(id)
    {
        ProductId = productId;
        UpdateInternal(unitId, conversionFactor, level);
    }

    internal void UpdateInternal(Guid unitId, int conversionFactor, int level)
    {
        if (unitId == Guid.Empty)
        {
            throw new BusinessException("SupplyCoreERP:InvalidUnit", "Đơn vị tính không hợp lệ.");
        }

        UnitId = unitId;
        SetFactorAndLevel(conversionFactor, level);
    }
    internal void UpdateStats(int conversionFactor, int level)
    {
        SetFactorAndLevel(conversionFactor, level);
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







