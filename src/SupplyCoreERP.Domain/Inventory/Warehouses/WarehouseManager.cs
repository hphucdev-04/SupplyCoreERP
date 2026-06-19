using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Balances;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventory.Warehouses;

public class WarehouseManager : DomainService
{
    // Dependencies
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly IRepository<Zone, Guid> _zoneRepo;
    private readonly IRepository<Bin, Guid> _binRepo;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly IDocumentSequenceManager _documentSequenceManager;

    // Constructor injection
    public WarehouseManager(
        IRepository<Warehouse, Guid> warehouseRepo,
        IRepository<Zone, Guid> zoneRepo,
        IRepository<Bin, Guid> binRepo,
        IRepository<InventoryBalance, Guid> balanceRepo,
        IDocumentSequenceManager documentSequenceManager
        )
    {
        _warehouseRepo = warehouseRepo;
        _zoneRepo = zoneRepo;
        _binRepo = binRepo;
        _balanceRepo = balanceRepo;
        _documentSequenceManager = documentSequenceManager;
    }

    #region Warehouse
    public async Task<Warehouse> CreateAsync(string name, string? address, Guid? countryId, Guid? cityId, Guid? areaId, int width, int length)
    {
        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeWarehouse);

        if (await _warehouseRepo.AnyAsync(x => x.Code == code))
        {
            throw new BusinessException("SupplyCoreERP:InvalidWarehouse", $"Mã kho '{code}' đã tồn tại!");
        }

        return new Warehouse(GuidGenerator.Create(), code, name, address, countryId, cityId, areaId, width, length);
    }

    public async Task UpdateAsync(Warehouse warehouse, string name, string? address, Guid? countryId, Guid? cityId, Guid? areaId, int width, int length)
    {

        warehouse.UpdateInfo(name, address, countryId, cityId, areaId);
        warehouse.UpdateMapSize(width, length);
    }

    public async Task DeleteAsync(Guid warehouseId)
    {
        if (await _balanceRepo.AnyAsync(x => x.WarehouseId == warehouseId && x.Quantity > 0))
        {
            throw new BusinessException("SupplyCoreERP:InvalidWarehouse", "Không thể xóa Kho đang chứa hàng tồn!");
        }

        await _binRepo.DeleteAsync(x => x.WarehouseId == warehouseId);
        await _zoneRepo.DeleteAsync(x => x.WarehouseId == warehouseId);
        await _warehouseRepo.DeleteAsync(warehouseId);
    }
    #endregion

    #region Warehouse Workflow
    public async Task SendToApproveAsync(Warehouse warehouse)
    {
        warehouse.SentToApprove();
        await _warehouseRepo.UpdateAsync(warehouse);
    }
    public async Task ApproveAsync(Warehouse warehouse)
    {
        warehouse.Approve();
        await _warehouseRepo.UpdateAsync(warehouse);
    }
    public async Task RejectAsync(Warehouse warehouse)
    {
        warehouse.Reject();
        await _warehouseRepo.UpdateAsync(warehouse);
    }
    #endregion

    #region Zone
    public async Task<Zone> CreateZoneAsync(Guid warehouseId, string name, ZoneType type, StorageCondition condition, string color, int x, int y, int w, int l, float rotation)
    {
        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeZone);

        if (await _zoneRepo.AnyAsync(z => z.WarehouseId == warehouseId && z.Code == code))
        {
            throw new BusinessException("SupplyCoreERP:InvalidZone", $"Mã khu vực '{code}' đã tồn tại trong kho này!");
        }

        return new Zone(GuidGenerator.Create(), warehouseId, code, name, type, condition, color, x, y, w, l, rotation);
    }

    public async Task UpdateZoneAsync(Zone zone, string name, ZoneType type, StorageCondition condition, string? color, int x, int y, int w, int l, float rotation)
    {

        zone.UpdateInfo(name, type, condition, color);
        zone.SetCoordinates(x, y, w, l, rotation);
    }

    public async Task DeleteZoneAsync(Guid zoneId)
    {
        if (await _binRepo.AnyAsync(b => b.ZoneId == zoneId))
        {
            throw new BusinessException("SupplyCoreERP:InvalidZone", "Khôn th thể xóa Khu vực đang chứa các vị trí (Bin). Vui lòng xóa Bin trước!");
        }

        await _zoneRepo.DeleteAsync(zoneId);
    }
    #endregion

    #region Bin
    public async Task<Bin> CreateBinAsync(
        Guid warehouseId, Guid zoneId,
        int x, int y, int w, int l, float rotation, int maxSKU, int height)
    {
        Zone zone = await _zoneRepo.GetAsync(zoneId);
        if (zone.WarehouseId != warehouseId)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", "Dữ liệu không hợp lệ: Zone không thuộc Warehouse này!");
        }

        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeBin);

        if (await _binRepo.AnyAsync(b => b.WarehouseId == warehouseId && b.Code == code))
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", $"Mã vị trí '{code}' đã tồn tại trong kho này!");
        }

        return new Bin(GuidGenerator.Create(), warehouseId, zoneId, code, x, y, w, l, rotation, maxSKU, height);
    }

    public async Task UpdateBinAsync(
        Bin bin, Guid zoneId,
        int x, int y, int w, int l, float rotation, int maxSKU, int height, bool isBlocked)
    {
        if (bin.ZoneId != zoneId)
        {
            Zone zone = await _zoneRepo.GetAsync(zoneId);
            if (zone.WarehouseId != bin.WarehouseId)
            {
                throw new UserFriendlyException("Dữ liệu không hợp lệ: Zone không thuộc Warehouse này!");
            }
        }

        if (maxSKU > 0)
        {
            int usedSKU = await _balanceRepo.CountAsync(b => b.BinBalances.Any(bb => bb.BinId == bin.Id && bb.Quantity > 0));
            if (maxSKU < usedSKU)
            {
                throw new BusinessException("SupplyCoreERP:InvalidBin", $"Không thể đặt giới hạn {maxSKU} SKU vì vị trí '{bin.Code}' đang chứa {usedSKU} loại hàng!");
            }
        }

        if (height > 0)
        {
            var balancesQuery = await _balanceRepo.WithDetailsAsync(x => x.BinBalances, x => x.Product);
            balancesQuery = balancesQuery
                .Where(x => x.BinBalances.Any(bb => bb.BinId == bin.Id));

            List<InventoryBalance> balancesInBin = await AsyncExecuter.ToListAsync(balancesQuery);
            decimal currentVolume = 0;
            foreach (var balance in balancesInBin)
            {
                var binBalance = balance.BinBalances.FirstOrDefault(bb => bb.BinId == bin.Id);
                if (binBalance != null && binBalance.Quantity > 0)
                {
                    currentVolume += binBalance.Quantity * balance.Product.BaseUnitVolume;
                }
            }

            decimal calculatedMaxVolume = (decimal)w * l * height;
            decimal allowedVolume = calculatedMaxVolume * 0.8m;
            if (allowedVolume < currentVolume)
            {
                throw new BusinessException("SupplyCoreERP:InvalidBin", $"Không thể đặt giới hạn chiều cao {height} cm (tương đương {calculatedMaxVolume:N2} cm³ tối đa, 80% là {allowedVolume:N2} cm³) vì vị trí '{bin.Code}' đang chứa hàng có tổng thể tích là {currentVolume:N2} cm³!");
            }
        }

        bin.UpdateInfo(zoneId, maxSKU, height);
        bin.SetCoordinates(x, y, w, l, rotation);
        bin.ToggleBlock(isBlocked);
    }

    public async Task DeleteBinAsync(Guid binId)
    {
        if (await _balanceRepo.AnyAsync(bal => bal.BinBalances.Any(bb => bb.BinId == binId && bb.Quantity > 0)))
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", "Không thể xóa Vị trí đang chứa hàng tồn kho!");
        }

        await _binRepo.DeleteAsync(binId);
    }
    #endregion

    #region Logic Validation
    public void ValidateStorageCompatibility(Bin bin, StorageCondition? productCondition)
    {
        if (bin.Zone == null)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", "Lỗi hệ thống: Không tải được thông tin Zone!");
        }

        if (productCondition.HasValue && bin.Zone.StorageCondition != productCondition.Value)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", "Sai điều kiện bảo quản: Vị trí này yêu cầu điều kiện '" + bin.Zone.StorageCondition + "' nhưng sản phẩm có điều kiện '" + productCondition.Value + "'!"
            );
        }

        if (bin.Zone.Type == ZoneType.Inbound || bin.Zone.Type == ZoneType.Outbound
            || bin.Zone.Type == ZoneType.ForkliftParking)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", $"Vị trí '{bin.Code}' thuộc khu vực '{bin.Zone.Name}' có loại '{bin.Zone.Type}' không phù hợp để chứa hàng hóa!");
        }
    }
    #endregion
}






