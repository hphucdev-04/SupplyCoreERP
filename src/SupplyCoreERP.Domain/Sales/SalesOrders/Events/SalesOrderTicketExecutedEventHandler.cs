using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets.Events;
using SupplyCoreERP.Sales.Orders;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;

namespace SupplyCoreERP.Sales.SalesOrders.Events;

public class SalesOrderTicketExecutedEventHandler
    : DomainService, ILocalEventHandler<InventoryTicketExecutedDomainEvent>, ITransientDependency
{
    private readonly IRepository<SalesOrder, Guid> _salesOrderRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly UnitConversionManager _unitConversionManager;

    public SalesOrderTicketExecutedEventHandler(
        IRepository<SalesOrder, Guid> salesOrderRepo,
        IRepository<Product, Guid> productRepo,
        UnitConversionManager unitConversionManager)
    {
        _salesOrderRepo = salesOrderRepo;
        _productRepo = productRepo;
        _unitConversionManager = unitConversionManager;
    }

    public async Task HandleEventAsync(InventoryTicketExecutedDomainEvent eventData)
    {
        // Chỉ xử lý các phiếu xuất kho và có tài liệu tham chiếu (SalesOrder)
        if (!IsIssueTicket(eventData.TicketType) || !eventData.ReferenceDocumentId.HasValue)
        {
            return;
        }

        Guid soId = eventData.ReferenceDocumentId.Value;
        IQueryable<SalesOrder> soQuery = await _salesOrderRepo.WithDetailsAsync(x => x.Lines);
        SalesOrder? so = soQuery.FirstOrDefault(x => x.Id == soId);

        if (so == null)
        {
            return;
        }

        foreach (InventoryTicketLineEto tLine in eventData.Lines)
        {
            if (tLine.ReferenceDocumentLineId.HasValue)
            {
                SalesOrderLine? soLine = so.Lines.FirstOrDefault(x => x.Id == tLine.ReferenceDocumentLineId.Value);
                if (soLine != null)
                {
                    var productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
                    Product product = await AsyncExecuter.FirstOrDefaultAsync(productQuery, p => p.Id == tLine.ProductId);
                    if (product == null)
                    {
                        throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), tLine.ProductId);
                    }

                    decimal baseQty = _unitConversionManager.ConvertToBaseQuantity(product, tLine.UnitId, tLine.Quantity);
                    decimal deliveredQty = _unitConversionManager.ConvertFromBaseQuantity(product, soLine.UnitId, baseQty, 4);
                    soLine.AddDeliveredQuantity(deliveredQty);
                }
            }
        }

        bool allDelivered = true;
        foreach (SalesOrderLine x in so.Lines)
        {
            var productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
            Product product = await AsyncExecuter.FirstOrDefaultAsync(productQuery, p => p.Id == x.ProductId);
            if (product == null)
            {
                throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), x.ProductId);
            }

            decimal deliveredBaseQty = _unitConversionManager.ConvertToBaseQuantity(product, x.UnitId, x.DeliveredQuantity);
            if (deliveredBaseQty < x.BaseQuantity - 0.0001m)
            {
                allDelivered = false;
                break;
            }
        }

        if (allDelivered)
        {
            so.Complete();
        }
        else if (so.Lines.Any(x => x.DeliveredQuantity > 0))
        {
            if (so.Status != SalesOrderStatus.Delivering && so.Status != SalesOrderStatus.Completed)
            {
                so.StartDelivering();
            }
        }

        await _salesOrderRepo.UpdateAsync(so);
    }

    private bool IsIssueTicket(TicketType type) =>
        type == TicketType.GoodsIssue || type == TicketType.DisposalIssue || type == TicketType.ReturnOutward;
}
