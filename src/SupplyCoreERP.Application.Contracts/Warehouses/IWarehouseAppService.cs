using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Warehouses.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Warehouses;

public interface IWarehouseAppService : IApplicationService
{
    // WAREHOUSE
    Task<PagedResultDto<WarehouseDto>> GetListAsync(GetWarehouseListDto input);
    Task<WarehouseDto> GetAsync(Guid id);
    Task<WarehouseDto> CreateAsync(CreateUpdateWarehouseDto input);
    Task<WarehouseDto> UpdateAsync(Guid id, CreateUpdateWarehouseDto input);

    // WORKFLOW
    Task DeleteAsync(Guid id);
    Task SendToApproveAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id);
    Task ToggleActiveAsync(Guid id);

    //ZONES
    Task<List<ZoneDto>> GetZonesAsync(Guid warehouseId);
    Task<ZoneDto> GetZoneAsync(Guid id);
    Task<ZoneDto> CreateZoneAsync(CreateUpdateZoneDto input);
    Task<ZoneDto> UpdateZoneAsync(Guid id, CreateUpdateZoneDto input);
    Task DeleteZoneAsync(Guid id);

    //BINS
    Task<List<BinDto>> GetStorageBinsAsync(Guid warehouseId);
    Task<BinDto> GetStorageBinAsync(Guid id);
    Task<BinDto> CreateStorageBinAsync(CreateUpdateBinDto input);
    Task<BinDto> UpdateStorageBinAsync(Guid id, CreateUpdateBinDto input);
    Task DeleteStorageBinAsync(Guid id);
    Task ToggleBinBlockAsync(Guid id);
}

