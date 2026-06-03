using System;
using System.Threading.Tasks;
using SupplyCoreERP.Inventory.Tickets;

namespace SupplyCoreERP.Procurement.PurchaseReturns;

public interface IPurchaseReturnManager
{
    Task<PurchaseReturn> CreateAsync(
        Guid purchaseOrderId,
        Guid supplierId,
        Guid warehouseId,
        DateTime returnDate,
        string? note);

    Task UpdateAsync(
        PurchaseReturn purchaseReturn,
        Guid warehouseId,
        DateTime returnDate,
        string? note);

    Task CheckBeforeDeleteAsync(PurchaseReturn purchaseReturn);

    Task AddLineAsync(
        PurchaseReturn purchaseReturn,
        Guid purchaseOrderLineId,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal originalUnitPrice,
        decimal depreciationRate,
        decimal taxRate);

    Task UpdateLineAsync(
        PurchaseReturn purchaseReturn,
        Guid lineId,
        decimal quantity,
        decimal depreciationRate);

    Task RemoveLineAsync(PurchaseReturn purchaseReturn, Guid lineId);

    Task SendToApproveAsync(PurchaseReturn purchaseReturn);

    Task<InventoryTicket> ApproveAsync(PurchaseReturn purchaseReturn);

    Task RejectAsync(PurchaseReturn purchaseReturn);

    Task CompleteAsync(PurchaseReturn purchaseReturn);
}
