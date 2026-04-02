using SupplyCoreERP.Balances.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Balances
{
	public interface IInventoryBalanceAppService : IApplicationService
	{
		Task<PagedResultDto<InventoryBalanceDto>> GetListAsync(GetInventoryBalanceListDto input);
		Task<InventoryBalanceDetailDto> GetAsync(Guid id);
	}
}