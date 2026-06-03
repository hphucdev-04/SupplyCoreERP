using System;
using System.Threading.Tasks;
using SupplyCoreERP.PurchaseReturnRequests.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.PurchaseReturnRequests;

public interface IPurchaseReturnRequestAppService : IApplicationService
{
    Task<PagedResultDto<PurchaseReturnRequestDto>> GetListAsync(GetPurchaseReturnRequestListDto input);
    Task<PurchaseReturnRequestDto> GetAsync(Guid id);
    Task<PurchaseReturnRequestDto> CreateAsync(CreatePurchaseReturnRequestDto input);
    Task<PurchaseReturnRequestDto> UpdateAsync(Guid id, UpdatePurchaseReturnRequestDto input);
    Task DeleteAsync(Guid id);

    Task AddLineAsync(Guid requestId, AddPurchaseReturnRequestLineDto input);
    Task UpdateLineAsync(Guid requestId, Guid lineId, UpdatePurchaseReturnRequestLineDto input);
    Task RemoveLineAsync(Guid requestId, Guid lineId);

    Task SendToApproveAsync(Guid id);
    Task ApproveAndSplitAsync(Guid id); // Gom nhóm & Tách đơn con tự động
    Task RejectAsync(Guid id);
}
