using SupplyCoreERP.PurchaseOrders.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.PurchaseOrders
{
	public interface IPurchaseOrderAppService : IApplicationService
	{
		// Purchase Orders
		Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input);
		Task<PurchaseOrderDto> GetAsync(Guid id);
		Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input);
		Task<PurchaseOrderDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto input);
		Task DeleteAsync(Guid id);
		// Details
		Task AddDetailAsync(Guid orderId, AddPurchaseOrderDetailDto input);
		Task UpdateDetailAsync(Guid orderId, Guid detailId, UpdatePurchaseOrderDetailDto input);
		Task RemoveDetailAsync(Guid orderId, Guid detailId);
		// Workflow
		Task SendToApproveAsync(Guid id);
		Task ApproveAsync(Guid id);
		Task CompleteAsync(Guid id);
		Task CancelAsync(Guid id, string reason);
	}
}