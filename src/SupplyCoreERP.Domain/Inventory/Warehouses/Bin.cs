using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Warehouses;

public class Bin : FullAuditedEntity<Guid>
{
    public Guid WarehouseId { get; private set; }
    public Guid ZoneId { get; private set; }
    public virtual Zone Zone { get; protected set; }

    public string Code { get; private set; }

    public int PositionX { get; private set; }
    public int PositionY { get; private set; }
    public int Width { get; private set; }
    public int Length { get; private set; }
    public float Rotation { get; private set; }

    public int MaxSKU { get; private set; }
    public bool IsBlocked { get; private set; }

    protected Bin() { }

    public Bin(Guid id, Guid warehouseId, Guid zoneId, string code,
               int x, int y, int w, int l, float rotation, int maxSKU = 0) : base(id)
    {
        WarehouseId = warehouseId;
        ZoneId = zoneId;
        Code = Check.NotNullOrWhiteSpace(code, nameof(Code)).ToUpper();
        MaxSKU = maxSKU;
        SetCoordinates(x, y, w, l, rotation);
    }

    public void SetCoordinates(int x, int y, int w, int l, float rotation)
    {
        PositionX = x; PositionY = y; Width = w; Length = l; Rotation = rotation;
    }

    public void UpdateInfo(Guid zoneId, int maxSKU)
    {
        ZoneId = zoneId;
        MaxSKU = maxSKU;
    }

    public void ToggleBlock(bool isBlocked) => IsBlocked = isBlocked;

    public void ValidateSKUCapacity(int usedSKUCount, bool isNewSKU)
    {
        if (IsBlocked)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", $"Vị trí '{Code}' đang bị khóa, không thể nhập hàng vào!");
        }

        if (MaxSKU <= 0)
        {
            return;
        }

        if (isNewSKU && usedSKUCount >= MaxSKU)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", $"Vị trí '{Code}' đã đạt giới hạn {MaxSKU} SKU/Lô!\n" +
                $"Hiện đang chứa {usedSKUCount} loại. Vui lòng chọn vị trí khác hoặc tăng giới hạn.");
        }
    }
}






