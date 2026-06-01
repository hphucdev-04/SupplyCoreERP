using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Procurement.PurchaseRequisitions;

public interface IPurchaseRequisitionManager : IDomainService
{
    Task<PurchaseRequisition> CreateAsync(
        Guid warehouseId,
        DateTime requestedDate,
        DateTime? requiredDate,
        string? note);

    Task UpdateAsync(
        PurchaseRequisition requisition,
        Guid warehouseId,
        DateTime? requiredDate,
        string? note);

    Task AddLineAsync(
        PurchaseRequisition requisition,
        Guid productId,
        Guid unitId,
        decimal quantity,
        string? note);

    Task UpdateLineAsync(PurchaseRequisition requisition, Guid lineId, decimal quantity, string? note);
    Task RemoveLineAsync(PurchaseRequisition requisition, Guid lineId);

    Task SendToApproveAsync(PurchaseRequisition requisition);
    Task ApproveAsync(PurchaseRequisition requisition);
    Task RejectAsync(PurchaseRequisition requisition);
}
