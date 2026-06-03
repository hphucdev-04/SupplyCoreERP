using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Tickets.Events;
using SupplyCoreERP.Partner.Customers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus;

namespace SupplyCoreERP.Sales.SalesRecalls.Events;

public class SalesRecallTicketExecutedEventHandler
    : DomainService, ILocalEventHandler<InventoryTicketExecutedDomainEvent>, ITransientDependency
{
    private readonly IRepository<SalesRecall, Guid> _salesRecallRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<Medicine, Guid> _medicineRepo;
    private readonly IRepository<ProductBatch, Guid> _batchRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly ISalesRecallManager _salesRecallManager;

    public SalesRecallTicketExecutedEventHandler(
        IRepository<SalesRecall, Guid> salesRecallRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<Medicine, Guid> medicineRepo,
        IRepository<ProductBatch, Guid> batchRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        ISalesRecallManager salesRecallManager)
    {
        _salesRecallRepo = salesRecallRepo;
        _customerRepo = customerRepo;
        _medicineRepo = medicineRepo;
        _batchRepo = batchRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _salesRecallManager = salesRecallManager;
    }

    public async Task HandleEventAsync(InventoryTicketExecutedDomainEvent eventData)
    {
        // Chỉ xử lý khi phiếu kho thuộc loại RecallReceipt (Nhập thu hồi) và có chứng từ tham chiếu
        if (eventData.TicketType != TicketType.RecallReceipt || !eventData.ReferenceDocumentId.HasValue)
        {
            return;
        }

        Guid recallId = eventData.ReferenceDocumentId.Value;
        SalesRecall? rr = await _salesRecallRepo.FindAsync(recallId);
        if (rr == null || rr.Status == Enums.Orders.SalesRecallStatus.Completed)
        {
            return;
        }

        // 1. Hoàn tất chứng từ thương mại thu hồi qua Manager
        await _salesRecallManager.CompleteAsync(rr);
        await _salesRecallRepo.UpdateAsync(rr);

        // 2. Tự động hoàn trả công nợ cho từng khách hàng theo số lượng thực tế trả về
        foreach (var line in rr.Lines)
        {
            Customer customer = await _customerRepo.GetAsync(line.CustomerId);
            customer.PayDebt(line.FinalPrice); // Giảm nợ phải thu của khách hàng tương ứng
            await _customerRepo.UpdateAsync(customer);
        }

        // 3. Khóa hoạt động của sản phẩm thuốc bị thu hồi (Medicine.IsActive = false)
        Medicine? medicine = await _medicineRepo.FindAsync(rr.ProductId);
        if (medicine != null && medicine.IsActive)
        {
            medicine.SetActive(false);
            await _medicineRepo.UpdateAsync(medicine);
        }

        // 4. Khóa/Thu hồi các Lô thuốc lỗi liên quan
        
        // A. Khóa lô thuốc bị chỉ định trực tiếp (nếu có)
        if (rr.ProductBatchId.HasValue)
        {
            ProductBatch? primaryBatch = await _batchRepo.FindAsync(rr.ProductBatchId.Value);
            if (primaryBatch != null)
            {
                primaryBatch.Recall();
                await _batchRepo.UpdateAsync(primaryBatch);
            }
        }

        // B. Khóa/Thu hồi các Lô thuốc lỗi thực tế đã nhập kho về theo chi tiết phiếu kho
        var detailQuery = await _ticketDetailRepo.GetQueryableAsync();
        var ticketDetailQuery = detailQuery.Where(d => d.TicketLine.TicketId == eventData.TicketId);
        
        List<Guid> batchIds = await AsyncExecuter.ToListAsync(
            ticketDetailQuery
                .Select(d => d.ProductBatchId)
                .Distinct()
        );

        // Loại trừ lô chính đã khóa ở trên để tránh trùng lặp
        if (rr.ProductBatchId.HasValue)
        {
            batchIds.Remove(rr.ProductBatchId.Value);
        }

        if (batchIds.Any())
        {
            List<ProductBatch> batches = await _batchRepo.GetListAsync(x => batchIds.Contains(x.Id));
            foreach (var b in batches)
            {
                b.Recall(); // Cập nhật trạng thái Batch thành Recalled
            }
            await _batchRepo.UpdateManyAsync(batches);
        }
    }
}
