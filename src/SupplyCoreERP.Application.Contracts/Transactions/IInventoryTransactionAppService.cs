using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SupplyCoreERP.Transactions.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Transactions;

public interface IInventoryTransactionAppService : IApplicationService
{
    Task<PagedResultDto<InventoryTransactionDto>> GetListAsync(GetInventoryTransactionListDto input);
    Task<InventoryTransactionDto> GetAsync(Guid id);
}

