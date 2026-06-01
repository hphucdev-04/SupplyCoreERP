using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Procurement.PurchaseOrders;

public interface IPurchaseOrderManager : IDomainService
{

    Task<PurchaseOrder> CreateOrderAsync(
        Guid supplierId, Guid warehouseId, DateTime orderDate,
        DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note,
        Guid? purchaseRequisitionId = null);

    Task<List<PurchaseOrder>> CreateOrdersFromRequisitionAsync(
        PurchaseRequisition requisition,
        List<(Guid RequisitionLineId, Guid SupplierId, Guid WarehouseId, decimal Quantity)> allocations,
        DateTime orderDate,
        string? note);

    Task UpdateOrderAsync(PurchaseOrder order, Guid warehouseId,
        DateTime? expectedDeliveryDate, DateTime? dueDate, string? note);

    Task CheckBeforeDeleteAsync(PurchaseOrder order);

    Task AddLineAsync(
        PurchaseOrder order,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal unitPrice,
        decimal taxRate);

    Task UpdateLineAsync(PurchaseOrder order, Guid lineId,
        decimal quantity, decimal unitPrice, decimal taxRate);

    Task RemoveLineAsync(PurchaseOrder order, Guid lineId);

    Task SendToApproveAsync(PurchaseOrder order);
    Task<InventoryTicket> ApproveAsync(PurchaseOrder order);
    Task<Supplier> CompleteAsync(PurchaseOrder order);
}
