using System;
using System.Threading.Tasks;
using SupplyCoreERP.PurchaseReturns.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.PurchaseReturns;

public interface IPurchaseReturnAppService : IApplicationService
{
    Task<PagedResultDto<PurchaseReturnDto>> GetListAsync(GetPurchaseReturnListDto input);
    Task<PurchaseReturnDto> GetAsync(Guid id);
    Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto input);
    Task<PurchaseReturnDto> UpdateAsync(Guid id, UpdatePurchaseReturnDto input);
    Task DeleteAsync(Guid id);

    Task AddLineAsync(Guid returnId, AddPurchaseReturnLineDto input);
    Task UpdateLineAsync(Guid returnId, Guid lineId, UpdatePurchaseReturnLineDto input);
    Task RemoveLineAsync(Guid returnId, Guid lineId);

    Task SendToApproveAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id);
}
