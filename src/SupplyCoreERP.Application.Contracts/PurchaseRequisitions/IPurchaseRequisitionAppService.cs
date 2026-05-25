using System;
using System.Threading.Tasks;
using SupplyCoreERP.PurchaseRequisitions.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.PurchaseRequisitions;

public interface IPurchaseRequisitionAppService : IApplicationService
{
    Task<PagedResultDto<PurchaseRequisitionDto>> GetListAsync(GetPurchaseRequisitionListDto input);
    Task<PurchaseRequisitionDto> GetAsync(Guid id);
    Task<PurchaseRequisitionDto> CreateAsync(CreatePurchaseRequisitionDto input);
    Task<PurchaseRequisitionDto> UpdateAsync(Guid id, UpdatePurchaseRequisitionDto input);
    Task DeleteAsync(Guid id);

    Task AddLineAsync(Guid requisitionId, AddPurchaseRequisitionLineDto input);
    Task UpdateLineAsync(Guid requisitionId, Guid lineId, UpdatePurchaseRequisitionLineDto input);
    Task RemoveLineAsync(Guid requisitionId, Guid lineId);

    Task SendToApproveAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task RejectAsync(Guid id);

    Task ConvertToPurchaseOrderAsync(Guid id, ConvertToPurchaseOrderDto input);
}

