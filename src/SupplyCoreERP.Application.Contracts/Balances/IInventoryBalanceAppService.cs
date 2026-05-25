using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Balances.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Balances;

public interface IInventoryBalanceAppService : IApplicationService
{
    Task<PagedResultDto<InventoryBalanceDto>> GetListAsync(GetInventoryBalanceListDto input);
    Task<InventoryBalanceDetailDto> GetAsync(Guid id);
    Task<PagedResultDto<InventoryReservationDto>> GetReservationListAsync(GetInventoryReservationListDto input);
}

