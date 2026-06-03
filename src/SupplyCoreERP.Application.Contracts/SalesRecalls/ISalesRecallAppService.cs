using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.SalesRecalls.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.SalesRecalls;

public interface ISalesRecallAppService : IApplicationService
{
    Task<PagedResultDto<SalesRecallDto>> GetListAsync(GetSalesRecallListDto input);
    Task<SalesRecallDto> GetAsync(Guid id);
    Task<SalesRecallDto> CreateAsync(CreateSalesRecallDto input);
    Task<SalesRecallDto> UpdateAsync(Guid id, UpdateSalesRecallDto input);
    Task DeleteAsync(Guid id);

    Task AddLineAsync(Guid recallId, AddSalesRecallLineDto input);
    Task UpdateLineAsync(Guid recallId, Guid lineId, UpdateSalesRecallLineDto input);
    Task RemoveLineAsync(Guid recallId, Guid lineId);

    Task SendToApproveAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id);

    Task<List<CustomerRecallTraceDto>> TraceCustomersByBatchAsync(Guid productBatchId);
}
