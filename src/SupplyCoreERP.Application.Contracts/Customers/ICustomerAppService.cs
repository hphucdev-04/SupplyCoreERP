using SupplyCoreERP.Customers.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Customers
{
	public interface ICustomerAppService : IApplicationService
	{
		Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomerListDto input);
		Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);
		Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input);
		Task DeleteAsync(Guid id);
		Task ToggleActiveAsync(Guid id);
	}
}
