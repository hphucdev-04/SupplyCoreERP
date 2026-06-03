using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventory.Tickets;

namespace SupplyCoreERP.Sales.SalesRecalls;

public interface ISalesRecallManager
{
    Task<SalesRecall> CreateAsync(
        Guid productId,
        Guid? productBatchId,
        Guid warehouseId,
        DateTime recallDate,
        RecallLevel level,
        string recallDecisionNumber,
        string? note);

    Task UpdateAsync(
        SalesRecall salesRecall,
        Guid warehouseId,
        DateTime recallDate,
        RecallLevel level,
        string recallDecisionNumber,
        string? note);

    Task CheckBeforeDeleteAsync(SalesRecall salesRecall);

    Task AddLineAsync(
        SalesRecall salesRecall,
        Guid customerId,
        Guid salesOrderId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal originalUnitPrice,
        decimal taxRate);

    Task UpdateLineAsync(
        SalesRecall salesRecall,
        Guid lineId,
        decimal quantity);

    Task RemoveLineAsync(SalesRecall salesRecall, Guid lineId);

    Task SendToApproveAsync(SalesRecall salesRecall);

    Task<InventoryTicket> ApproveAsync(SalesRecall salesRecall);

    Task RejectAsync(SalesRecall salesRecall);

    Task CompleteAsync(SalesRecall salesRecall);
}
