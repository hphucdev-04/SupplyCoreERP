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
    // Details
    Task AddDetailAsync(Guid orderId, AddSalesOrderDetailDto input);
    Task UpdateDetailAsync(Guid orderId, Guid detailId, UpdateSalesOrderDetailDto input);
    Task RemoveDetailAsync(Guid orderId, Guid detailId);
    // Workflow
    Task SendToApproveAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task CompleteAsync(Guid id);
    Task CancelAsync(Guid id, string reason);
}
