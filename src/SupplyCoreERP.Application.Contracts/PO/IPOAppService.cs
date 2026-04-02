using SupplyCoreERP.PO.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.PO
{
	public interface IPOAppService : IApplicationService
	{
		Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input);
		Task<PurchaseOrderDto> GetAsync(Guid id);
		Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input);
		Task<PurchaseOrderDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto input);
		Task DeleteAsync(Guid id);

		// Thao tác chi tiết
		Task<PurchaseOrderDto> AddDetailAsync(Guid orderId, AddPurchaseOrderDetailDto input);
		Task<PurchaseOrderDto> UpdateDetailAsync(Guid orderId, Guid detailId, UpdatePurchaseOrderDetailDto input);
		Task<PurchaseOrderDto> RemoveDetailAsync(Guid orderId, Guid detailId);

		// Quy trình duyệt
		Task SendToApproveAsync(Guid id);
		Task ApproveAsync(Guid id);
	}
}
