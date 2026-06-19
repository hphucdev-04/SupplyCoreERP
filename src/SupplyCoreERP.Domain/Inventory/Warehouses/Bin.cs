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
    public int Height { get; private set; }
    public decimal MaxVolume { get; private set; }
    public bool IsBlocked { get; private set; }

    protected Bin() { }

    public Bin(Guid id, Guid warehouseId, Guid zoneId, string code,
               int x, int y, int w, int l, float rotation, int maxSKU = 0, int height = 0) : base(id)
    {
        WarehouseId = warehouseId;
        ZoneId = zoneId;
        Code = Check.NotNullOrWhiteSpace(code, nameof(Code)).ToUpper();
        MaxSKU = maxSKU;
        Height = height;
        MaxVolume = (decimal)w * l * height;
        SetCoordinates(x, y, w, l, rotation);
    }

    public void SetCoordinates(int x, int y, int w, int l, float rotation)
    {
        PositionX = x; PositionY = y; Width = w; Length = l; Rotation = rotation;
        MaxVolume = (decimal)w * l * Height;
    }

    public void UpdateInfo(Guid zoneId, int maxSKU, int height)
    {
        ZoneId = zoneId;
        MaxSKU = maxSKU;
        Height = height;
        MaxVolume = (decimal)Width * Length * height;
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

    public void ValidateVolumeCapacity(decimal currentVolume, decimal newVolume)
    {
        if (IsBlocked)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", $"Vị trí '{Code}' đang bị khóa, không thể nhập hàng vào!");
        }

        if (MaxVolume <= 0)
        {
            return;
        }

        decimal allowedVolume = MaxVolume * 0.8m;

        if (currentVolume + newVolume > allowedVolume)
        {
            throw new BusinessException("SupplyCoreERP:BinOverCapacity", 
                $"Vị trí '{Code}' không đủ sức chứa thực tế!\n" +
                $"Thể tích hiện tại: {currentVolume:N2} cm³, Thể tích hàng mới: {newVolume:N2} cm³, Sức chứa tối đa: {MaxVolume:N2} cm³ (Chỉ được phép sử dụng tối đa 80% sức chứa: {allowedVolume:N2} cm³).");
        }
    }
}






