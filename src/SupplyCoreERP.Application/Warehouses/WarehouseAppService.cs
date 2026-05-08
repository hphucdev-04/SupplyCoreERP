using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Warehouses.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Warehouses;

public class WarehouseAppService : SupplyCore, IWarehouseAppService
{
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly IRepository<Zone, Guid> _zoneRepo;
    private readonly IRepository<Bin, Guid> _binRepo;
    private readonly WarehouseManager _warehouseManager;

    public WarehouseAppService(
        IRepository<Warehouse, Guid> warehouseRepo,
        IRepository<Zone, Guid> zoneRepo,
        IRepository<Bin, Guid> binRepo,
        WarehouseManager warehouseManager)
    {
        _warehouseRepo = warehouseRepo;
        _zoneRepo = zoneRepo;
        _binRepo = binRepo;
        _warehouseManager = warehouseManager;
    }

    #region Warehouse 
    public async Task<PagedResultDto<WarehouseDto>> GetListAsync(GetWarehouseListDto input)
    {
        IQueryable<Warehouse> query = await _warehouseRepo.WithDetailsAsync(x => x.City, x => x.Area);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter) || x.Name.Contains(input.Filter))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        int totalCount = await AsyncExecuter.CountAsync(query);
        List<Warehouse> items = await AsyncExecuter.ToListAsync(
            query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime DESC" : input.Sorting)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        return new PagedResultDto<WarehouseDto>(totalCount, ObjectMapper.Map<List<Warehouse>, List<WarehouseDto>>(items));
    }

    public async Task<WarehouseDto> GetAsync(Guid id)
    {
        Warehouse warehouse = await _warehouseRepo.GetAsync(id);
        return ObjectMapper.Map<Warehouse, WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateUpdateWarehouseDto input)
    {
        Warehouse warehouse = await _warehouseManager.CreateAsync(
            input.Name, input.Address, input.CountryId, input.CityId, input.AreaId, input.MapWidth, input.MapLength);

        await _warehouseRepo.InsertAsync(warehouse);
        return ObjectMapper.Map<Warehouse, WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> UpdateAsync(Guid id, CreateUpdateWarehouseDto input)
    {
        Warehouse warehouse = await _warehouseRepo.GetAsync(id);

        await _warehouseManager.UpdateAsync(
            warehouse, input.Name, input.Address, input.CountryId, input.CityId, input.AreaId, input.MapWidth, input.MapLength);

        await _warehouseRepo.UpdateAsync(warehouse);
        return ObjectMapper.Map<Warehouse, WarehouseDto>(warehouse);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _warehouseManager.DeleteAsync(id);
    }

    public async Task ApproveAsync(Guid id)
    {
        Warehouse warehouse = await _warehouseRepo.GetAsync(id);
        warehouse.Approve();
        await _warehouseRepo.UpdateAsync(warehouse);
    }

    public async Task RejectAsync(Guid id)
    {
        Warehouse warehouse = await _warehouseRepo.GetAsync(id);
        warehouse.Reject();
        await _warehouseRepo.UpdateAsync(warehouse);
    }

    public async Task ToggleActiveAsync(Guid id)
    {
        Warehouse warehouse = await _warehouseRepo.GetAsync(id);
        warehouse.SetActive(!warehouse.IsActive);
        await _warehouseRepo.UpdateAsync(warehouse);
    }
    #endregion

    #region Zone 
    public async Task<List<ZoneDto>> GetZonesAsync(Guid warehouseId)
    {
        List<Zone> zones = await _zoneRepo.GetListAsync(x => x.WarehouseId == warehouseId);
        return ObjectMapper.Map<List<Zone>, List<ZoneDto>>(zones);
    }

    public async Task<ZoneDto> GetZoneAsync(Guid id)
    {
        Zone zone = await _zoneRepo.GetAsync(id);
        return ObjectMapper.Map<Zone, ZoneDto>(zone);
    }

    public async Task<ZoneDto> CreateZoneAsync(CreateUpdateZoneDto input)
    {
        Zone zone = await _warehouseManager.CreateZoneAsync(
            input.WarehouseId, input.Name, input.Type,
            input.StorageCondition, input.Color,
            input.PositionX, input.PositionY, input.Width, input.Length, input.Rotation);

        await _zoneRepo.InsertAsync(zone);
        return ObjectMapper.Map<Zone, ZoneDto>(zone);
    }

    public async Task<ZoneDto> UpdateZoneAsync(Guid id, CreateUpdateZoneDto input)
    {
        Zone zone = await _zoneRepo.GetAsync(id);

        await _warehouseManager.UpdateZoneAsync(
            zone, input.Name, input.Type,
            input.StorageCondition, input.Color,
            input.PositionX, input.PositionY, input.Width, input.Length, input.Rotation);

        await _zoneRepo.UpdateAsync(zone);
        return ObjectMapper.Map<Zone, ZoneDto>(zone);
    }

    public async Task DeleteZoneAsync(Guid id)
    {
        await _warehouseManager.DeleteZoneAsync(id);
    }
    #endregion

    #region Bin 
    public async Task<List<BinDto>> GetStorageBinsAsync(Guid warehouseId)
    {
        IQueryable<Bin> query = await _binRepo.WithDetailsAsync(x => x.Zone);
        List<Bin> bins = await AsyncExecuter.ToListAsync(query.Where(x => x.WarehouseId == warehouseId));

        return ObjectMapper.Map<List<Bin>, List<BinDto>>(bins);
    }

    public async Task<BinDto> GetStorageBinAsync(Guid id)
    {
        IQueryable<Bin> query = await _binRepo.WithDetailsAsync(x => x.Zone);
        Bin? bin = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (bin == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy vị trí (Bin) này!");
        }

        return ObjectMapper.Map<Bin, BinDto>(bin);
    }

    public async Task<BinDto> CreateStorageBinAsync(CreateUpdateBinDto input)
    {
        Bin bin = await _warehouseManager.CreateBinAsync(
            input.WarehouseId, input.ZoneId,
            input.PositionX, input.PositionY, input.Width, input.Length, input.Rotation, input.MaxSKU);

        await _binRepo.InsertAsync(bin);

        return ObjectMapper.Map<Bin, BinDto>(bin);
    }

    public async Task<BinDto> UpdateStorageBinAsync(Guid id, CreateUpdateBinDto input)
    {
        IQueryable<Bin> query = await _binRepo.WithDetailsAsync(x => x.Zone);
        Bin? bin = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));

        if (bin == null)
        {
            throw new Volo.Abp.UserFriendlyException("Không tìm thấy vị trí (Bin) này!");
        }

        await _warehouseManager.UpdateBinAsync(
            bin, input.ZoneId,
            input.PositionX, input.PositionY, input.Width, input.Length, input.Rotation,
            input.MaxSKU, input.IsBlocked);

        await _binRepo.UpdateAsync(bin);

        // Nếu ZoneId thay đổi, cần reload details để map ZoneName mới
        if (bin.Zone == null || bin.Zone.Id != input.ZoneId)
        {
            bin = await _binRepo.GetAsync(id, includeDetails: true);
        }

        return ObjectMapper.Map<Bin, BinDto>(bin);
    }

    public async Task DeleteStorageBinAsync(Guid id)
    {
        await _warehouseManager.DeleteBinAsync(id);
    }

    public async Task ToggleBinBlockAsync(Guid id)
    {
        Bin bin = await _binRepo.GetAsync(id);
        bin.ToggleBlock(!bin.IsBlocked);
        await _binRepo.UpdateAsync(bin);
    }
    #endregion
}
