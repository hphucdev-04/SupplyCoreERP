using System;
using System.Threading.Tasks;
using SupplyCoreERP.Customers.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Customers;

public interface ICustomerAppService : IApplicationService
{
    Task<CustomerDetailDto> GetAsync(Guid id);
    Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomerListDto input);
    Task<CustomerDetailDto> CreateAsync(CreateUpdateCustomerDto input);
    Task<CustomerDetailDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input);
    Task DeleteAsync(Guid id);
    Task ToggleActiveAsync(Guid id);
    Task<CustomerSummaryDto> GetSummaryAsync();
}

