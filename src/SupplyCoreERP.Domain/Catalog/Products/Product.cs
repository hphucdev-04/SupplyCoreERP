using System;
using System.Collections.Generic;
using System.Linq;
using SupplyCoreERP.Catalog.BaseUnits;
using SupplyCoreERP.Catalog.Categories;
using SupplyCoreERP.Catalog.Manufacturers;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Products;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Catalog.Products;

public abstract class Product : FullAuditedAggregateRoot<Guid>
{
    public Guid CategoryId { get; protected set; }
    public virtual Category Category { get; protected set; }
    public Guid ManufacturerId { get; protected set; }
    public virtual Manufacturer Manufacturer { get; protected set; }
    public string Code { get; protected set; }
    public string Name { get; protected set; }
    public Guid BaseUnitId { get; protected set; }
    public virtual BaseUnit BaseUnit { get; protected set; }
    public ProductType ProductType { get; protected set; }
    public decimal BaseUnitVolume { get; protected set; }
    public virtual ICollection<ProductUnit> Units { get; protected set; }

    /// <summary>
    /// Cho phép override để xác định xem sản phẩm có được phép quản lý tồn kho hay không.
    /// </summary>
    public virtual bool IsAvailableForInventory => true;
    /// <summary>
    /// Cho phép override để xác định điều kiện bảo quản bắt buộc cho sản phẩm, nếu có. 
    /// </summary>
    public virtual StorageCondition? RequiredStorageCondition => null;

    protected Product()
    {
        Units = new List<ProductUnit>();
    }

    protected Product(
        Guid id,
        Guid categoryId,
        Guid manufacturerId,
        string code,
        string name,
        Guid baseUnitId,
        ProductType productType,
        decimal baseUnitVolume = 0)
        : base(id)
    {
        CategoryId = categoryId;
        ManufacturerId = manufacturerId;
        SetCode(code);
        SetName(name);
        BaseUnitId = baseUnitId;
        ProductType = productType;
        BaseUnitVolume = baseUnitVolume;
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

    public void SetCategory(Guid id) => CategoryId = id;
    public void SetManufacturer(Guid id) => ManufacturerId = id;

    public void UpdateInfo(string name, Guid categoryId, Guid manufacturerId, Guid baseUnitId, decimal baseUnitVolume)
    {
        if (Units.Any(u => u.UnitId == baseUnitId))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateBaseUnitInUnits", "Đơn vị gốc không được trùng với các đơn vị quy đổi đang có.");
        }

        SetName(name);
        CategoryId = categoryId;
        ManufacturerId = manufacturerId;
        BaseUnitId = baseUnitId;
        BaseUnitVolume = baseUnitVolume;
    }

    public void AddUnit(Guid id, Guid unitId, int conversionFactor, int level, decimal volume = 0)
    {
        if (unitId == BaseUnitId)
        {
            throw new BusinessException("SupplyCoreERP:DuplicateBaseUnit", "Không được thêm đơn vị trùng với Đơn vị gốc.");
        }

        if (Units.Any(u => u.UnitId == unitId))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateUnit", "Đơn vị quy đổi này đã tồn tại.");
        }

        if (conversionFactor <= 1)
        {
            throw new BusinessException("SupplyCoreERP:InvalidFactor", "Hệ số quy đổi phải lớn hơn 1.");
        }

        int nextLevel = Units.Any() ? Units.Max(u => u.Level) + 1 : 1;
        Units.Add(new ProductUnit(id, Id, unitId, conversionFactor, nextLevel, volume));
    }

    public void UpdateUnit(Guid unitId, int conversionFactor, int level, decimal volume)
    {
        ProductUnit? unit = Units.FirstOrDefault(u => u.UnitId == unitId);
        if (unit == null)
        {
            throw new UserFriendlyException("Đơn vị không tồn tại.");
        }

        unit.UpdateStats(conversionFactor, unit.Level, volume);
    }

    public void RemoveUnit(Guid unitId)
    {
        ProductUnit? unit = Units.FirstOrDefault(u => u.UnitId == unitId);
        if (unit == null)
        {
            return;
        }

        int maxLevel = Units.Max(u => u.Level);
        if (unit.Level < maxLevel)
        {
            throw new BusinessException(
                "SupplyCoreERP:CannotDeleteLowerLevelUnit",
                "Không thể xóa đơn vị ở cấp thấp hơn. Vui lòng xóa đơn vị có cấp độ cao nhất trước."
            );
        }

        Units.Remove(unit);
    }
}







