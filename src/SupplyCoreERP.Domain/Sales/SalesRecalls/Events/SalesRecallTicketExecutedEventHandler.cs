using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Medicines;
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
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly ISalesRecallManager _salesRecallManager;

    public SalesRecallTicketExecutedEventHandler(
        IRepository<SalesRecall, Guid> salesRecallRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<Medicine, Guid> medicineRepo,
        IRepository<ProductBatch, Guid> batchRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        ISalesRecallManager salesRecallManager)
    {
        _salesRecallRepo = salesRecallRepo;
        _customerRepo = customerRepo;
        _medicineRepo = medicineRepo;
        _batchRepo = batchRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _ticketLineRepo = ticketLineRepo;
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
        IQueryable<SalesRecall> query = await _salesRecallRepo.WithDetailsAsync(x => x.Lines);
        SalesRecall? rr = await AsyncExecuter.FirstOrDefaultAsync(query, x => x.Id == recallId);
        if (rr == null || rr.Status == Enums.Orders.SalesRecallStatus.Completed)
        {
            return;
        }

        // 1. Tự động hoàn trả công nợ cho từng khách hàng theo số lượng thực tế trả về của phiếu kho này
        // Và cập nhật lũy kế số lượng đã thu hồi thực tế (RecalledQuantity) cho từng dòng
        foreach (InventoryTicketLineEto eventLine in eventData.Lines)
        {
            if (eventLine.ReferenceDocumentLineId.HasValue)
            {
                SalesRecallLine? recallLine = rr.Lines.FirstOrDefault(l => l.Id == eventLine.ReferenceDocumentLineId.Value);
                if (recallLine != null)
                {
                    // Cộng lũy kế số lượng đã thu hồi thực tế vào dòng
                    recallLine.AddRecalledQuantity(eventLine.Quantity);

                    // Công nợ thực tế cần trả lại = (số lượng thực nhận) * đơn giá * (1 + thuế suất/100)
                    decimal lineTotalPrice = eventLine.Quantity * recallLine.OriginalUnitPrice;
                    decimal lineTaxAmount = lineTotalPrice * (recallLine.TaxRate / 100);
                    decimal lineFinalPrice = lineTotalPrice + lineTaxAmount;

                    if (lineFinalPrice > 0)
                    {
                        Customer customer = await _customerRepo.GetAsync(recallLine.CustomerId);
                        customer.PayDebt(lineFinalPrice); // Giảm nợ phải thu của khách hàng tương ứng
                        await _customerRepo.UpdateAsync(customer);
                    }
                }
            }
        }

        // 2. Kiểm tra điều kiện hoàn tất quyết định thu hồi
        if (rr.Lines.All(l => l.RecalledQuantity >= l.Quantity))
        {
            // A. Hoàn tất chứng từ thương mại thu hồi qua Manager
            await _salesRecallManager.CompleteAsync(rr);

            // B. Khóa hoạt động của sản phẩm thuốc bị thu hồi (Medicine.IsActive = false)
            Medicine? medicine = await _medicineRepo.FindAsync(rr.ProductId);
            if (medicine != null && medicine.IsActive)
            {
                medicine.SetActive(false);
                await _medicineRepo.UpdateAsync(medicine);
            }

            // C. Khóa/Thu hồi các Lô thuốc lỗi liên quan
            // Khóa lô thuốc bị chỉ định trực tiếp (nếu có)
            if (rr.ProductBatchId.HasValue)
            {
                ProductBatch? primaryBatch = await _batchRepo.FindAsync(rr.ProductBatchId.Value);
                if (primaryBatch != null)
                {
                    primaryBatch.Recall();
                    await _batchRepo.UpdateAsync(primaryBatch);
                }
            }

            // Khóa/Thu hồi các Lô thuốc lỗi thực tế đã nhập kho về theo chi tiết các phiếu kho của quyết định này
            IQueryable<InventoryTicketDetail> detailQuery = await _ticketDetailRepo.GetQueryableAsync();
            var recallLineIds = rr.Lines.Select(l => l.Id).ToList();

            // Lấy tất cả detail của các phiếu kho đã approved liên kết với quyết định thu hồi này
            List<Guid> batchIds = await AsyncExecuter.ToListAsync(
                detailQuery
                    .Where(d => d.TicketLine.ReferenceDocumentLineId.HasValue &&
                                recallLineIds.Contains(d.TicketLine.ReferenceDocumentLineId.Value) &&
                                d.TicketLine.Ticket.Status == ApprovalStatus.Approved)
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
                foreach (ProductBatch b in batches)
                {
                    b.Recall(); // Cập nhật trạng thái Batch thành Recalled
                }
                await _batchRepo.UpdateManyAsync(batches);
            }
        }
        else if (rr.Lines.Any(l => l.RecalledQuantity > 0))
        {
            // Chuyển sang trạng thái Recalling (Đang thu hồi)
            if (rr.Status != Enums.Orders.SalesRecallStatus.Recalling)
            {
                rr.StartRecalling();
            }
        }

        // 3. Cập nhật thay đổi của SalesRecall và các Lines vào DB
        await _salesRecallRepo.UpdateAsync(rr);
    }
}
