using System;
using System.Threading.Tasks;
using SupplyCoreERP.PurchaseOrders.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.PurchaseOrders;

public interface IPurchaseOrderAppService : IApplicationService
{
    // Purchase Orders
    Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input);
    Task<PurchaseOrderDto> GetAsync(Guid id);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input);
    Task<PurchaseOrderDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto input);
    Task DeleteAsync(Guid id);

    // Lines
    Task AddLineAsync(Guid orderId, AddPurchaseOrderLineDto input);
    Task UpdateLineAsync(Guid orderId, Guid lineId, UpdatePurchaseOrderLineDto input);
    Task RemoveLineAsync(Guid orderId, Guid lineId);

    // Workflow
    Task SendToApproveAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task CompleteAsync(Guid id);
    Task CancelAsync(Guid id, string reason);
}
