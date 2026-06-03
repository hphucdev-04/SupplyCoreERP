using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Orders;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Procurement.PurchaseReturnRequests;

public interface IPurchaseReturnRequestManager : IDomainService
{
    Task<PurchaseReturnRequest> CreateAsync(
        Guid supplierId,
        Guid warehouseId,
        PurchaseReturnType returnType,
        DateTime requestDate,
        string? note);
    Task AddLineAsync(
        PurchaseReturnRequest request,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        Guid purchaseOrderId,
        Guid purchaseOrderLineId,
        decimal quantity,
        decimal originalUnitPrice,
        decimal depreciationRate,
        decimal taxRate);
    Task UpdateLineAsync(
        PurchaseReturnRequest request,
        Guid lineId,
        decimal quantity,
        decimal depreciationRate);

    Task ApproveAndSplitAsync(PurchaseReturnRequest request);
}
