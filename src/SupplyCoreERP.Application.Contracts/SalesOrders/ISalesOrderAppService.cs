using System;
using System.Threading.Tasks;
using SupplyCoreERP.SalesOrders.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.SalesOrders;

public interface ISalesOrderAppService : IApplicationService
{
    // Sales Orders
    Task<PagedResultDto<SalesOrderDto>> GetListAsync(GetSalesOrderListDto input);
    Task<SalesOrderDto> GetAsync(Guid id);
    Task<SalesOrderDto> CreateAsync(CreateSalesOrderDto input);
    Task<SalesOrderDto> UpdateAsync(Guid id, UpdateSalesOrderDto input);
    Task DeleteAsync(Guid id);
    // Lines
    Task AddLineAsync(Guid orderId, AddSalesOrderLineDto input);
    Task UpdateLineAsync(Guid orderId, Guid lineId, UpdateSalesOrderLineDto input);
    Task RemoveLineAsync(Guid orderId, Guid lineId);
    // Workflow
    Task SendToApproveAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task CompleteAsync(Guid id);
}

