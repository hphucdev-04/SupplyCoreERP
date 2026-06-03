using System;
using System.Threading.Tasks;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets.Events;
using SupplyCoreERP.Partner.Suppliers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;

namespace SupplyCoreERP.Procurement.PurchaseReturns.Events;

public class PurchaseReturnTicketExecutedEventHandler
    : DomainService, ILocalEventHandler<InventoryTicketExecutedDomainEvent>, ITransientDependency
{
    private readonly IRepository<PurchaseReturn, Guid> _purchaseReturnRepo;
    private readonly IRepository<Supplier, Guid> _supplierRepo;
    private readonly IPurchaseReturnManager _purchaseReturnManager;

    public PurchaseReturnTicketExecutedEventHandler(
        IRepository<PurchaseReturn, Guid> purchaseReturnRepo,
        IRepository<Supplier, Guid> supplierRepo,
        IPurchaseReturnManager purchaseReturnManager)
    {
        _purchaseReturnRepo = purchaseReturnRepo;
        _supplierRepo = supplierRepo;
        _purchaseReturnManager = purchaseReturnManager;
    }

    public async Task HandleEventAsync(InventoryTicketExecutedDomainEvent eventData)
    {
        // Chỉ xử lý khi phiếu kho thuộc loại ReturnOutward (Xuất trả NCC) và có chứng từ tham chiếu
        if (eventData.TicketType != TicketType.ReturnOutward || !eventData.ReferenceDocumentId.HasValue)
        {
            return;
        }

        Guid roId = eventData.ReferenceDocumentId.Value;
        PurchaseReturn? ro = await _purchaseReturnRepo.FindAsync(roId);
        if (ro == null || ro.Status == Enums.Orders.PurchaseReturnStatus.Completed)
        {
            return;
        }

        // 1. Hoàn tất chứng từ thương mại xuất trả qua Manager
        await _purchaseReturnManager.CompleteAsync(ro);
        await _purchaseReturnRepo.UpdateAsync(ro);

        // 2. Tự động giảm công nợ phải trả Nhà cung cấp
        Supplier supplier = await _supplierRepo.GetAsync(ro.SupplierId);
        supplier.PayDebt(ro.TotalAmount); // PayDebt với giá trị dương = giảm nợ phải trả
        await _supplierRepo.UpdateAsync(supplier);
    }
}
