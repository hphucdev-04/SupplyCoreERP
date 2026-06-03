using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;

namespace SupplyCoreERP.Procurement.PurchaseOrders.Events;

public class PurchaseOrderTicketExecutedEventHandler
    : DomainService, ILocalEventHandler<InventoryTicketExecutedDomainEvent>, ITransientDependency
{
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly UnitConversionManager _unitConversionManager;

    public PurchaseOrderTicketExecutedEventHandler(
        IRepository<PurchaseOrder, Guid> purchaseOrderRepo,
        IRepository<Product, Guid> productRepo,
        UnitConversionManager unitConversionManager)
    {
        _purchaseOrderRepo = purchaseOrderRepo;
        _productRepo = productRepo;
        _unitConversionManager = unitConversionManager;
    }

    public async Task HandleEventAsync(InventoryTicketExecutedDomainEvent eventData)
    {
        // Chỉ xử lý các phiếu nhập kho (GoodsReceipt) và có tài liệu tham chiếu (PurchaseOrder)
        if (eventData.TicketType != TicketType.GoodsReceipt || !eventData.ReferenceDocumentId.HasValue)
        {
            return;
        }

        Guid poId = eventData.ReferenceDocumentId.Value;
        IQueryable<PurchaseOrder> poQuery = await _purchaseOrderRepo.WithDetailsAsync(x => x.Lines);
        PurchaseOrder? po = poQuery.FirstOrDefault(x => x.Id == poId);

        if (po == null)
        {
            return;
        }

        foreach (InventoryTicketLineEto tLine in eventData.Lines)
        {
            if (tLine.ReferenceDocumentLineId.HasValue)
            {
                PurchaseOrderLine? poLine = po.Lines.FirstOrDefault(x => x.Id == tLine.ReferenceDocumentLineId.Value);
                if (poLine != null)
                {
                    IQueryable<Product> productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
                    Product product = await AsyncExecuter.FirstOrDefaultAsync(productQuery, p => p.Id == tLine.ProductId);
                    if (product == null)
                    {
                        throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), tLine.ProductId);
                    }

                    decimal baseQty = _unitConversionManager.ConvertToBaseQuantity(product, tLine.UnitId, tLine.Quantity);
                    decimal receivedQty = _unitConversionManager.ConvertFromBaseQuantity(product, poLine.UnitId, baseQty, 4);
                    poLine.AddReceivedQuantity(receivedQty);
                }
            }
        }

        if (po.Lines.Any(x => x.ReceivedQuantity > 0))
        {
            if (po.Status != PurchaseOrderStatus.Receiving && po.Status != PurchaseOrderStatus.Completed)
            {
                po.StartReceiving();
            }
        }

        await _purchaseOrderRepo.UpdateAsync(po);
    }
}
