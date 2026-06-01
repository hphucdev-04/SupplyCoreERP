using System;
using System.Threading.Tasks;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Partner.Customers;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Sales.Orders;

public interface ISalesOrderManager : IDomainService
{
    Task<SalesOrder> CreateOrderAsync(
        Guid customerId, Guid warehouseId, DateTime orderDate,
        DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note);

    Task UpdateOrderAsync(SalesOrder order, Guid warehouseId,
        DateTime? expectedDeliveryDate, DateTime? dueDate, string? note);

    Task CheckBeforeDeleteAsync(SalesOrder order);

    Task AddLineAsync(
        SalesOrder order,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal? unitPrice,
        decimal discountRate,
        decimal taxRate);

    Task UpdateLineAsync(
        SalesOrder order,
        Guid lineId,
        decimal quantity,
        decimal? unitPrice,
        decimal discountRate,
        decimal taxRate);

    Task RemoveLineAsync(SalesOrder order, Guid lineId);

    Task SendToApproveAsync(SalesOrder order);
    Task<InventoryTicket> ApproveAsync(SalesOrder order);
    Task<Customer> CompleteAsync(SalesOrder order);
}
