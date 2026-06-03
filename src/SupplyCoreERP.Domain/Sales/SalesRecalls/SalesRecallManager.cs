using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Sales.Orders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Sales.SalesRecalls;

public class SalesRecallManager : DomainService, ISalesRecallManager
{
    private readonly IRepository<SalesRecall, Guid> _salesRecallRepo;
    private readonly IRepository<SalesRecallLine, Guid> _lineRepo;
    private readonly IRepository<SalesOrderLine, Guid> _soLineRepo;
    private readonly ITicketManager _ticketManager;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IDocumentSequenceManager _documentManager;
    private readonly IRepository<Product, Guid> _productRepo;

    public SalesRecallManager(
        IRepository<SalesRecall, Guid> salesRecallRepo,
        IRepository<SalesRecallLine, Guid> lineRepo,
        IRepository<SalesOrderLine, Guid> soLineRepo,
        ITicketManager ticketManager,
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        IDocumentSequenceManager documentManager,
        IRepository<Product, Guid> productRepo)
    {
        _salesRecallRepo = salesRecallRepo;
        _lineRepo = lineRepo;
        _soLineRepo = soLineRepo;
        _ticketManager = ticketManager;
        _ticketRepo = ticketRepo;
        _ticketLineRepo = ticketLineRepo;
        _documentManager = documentManager;
        _productRepo = productRepo;
    }

    public async Task<SalesRecall> CreateAsync(
        Guid productId,
        Guid? productBatchId,
        Guid warehouseId,
        DateTime recallDate,
        RecallLevel level,
        string recallDecisionNumber,
        string? note)
    {
        Product product = await _productRepo.GetAsync(productId);
        if (product is Medicine medicine && !medicine.IsActive)
        {
            throw new BusinessException("SupplyCoreERP:InactiveProduct", $"Sản phẩm thuốc '{product.Name}' đang bị khóa hoạt động!");
        }

        // Tự động sinh mã code từ DocumentSequenceManager với sequence code RC
        string code = await _documentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeSalesRecall);

        return new SalesRecall(
            GuidGenerator.Create(),
            code,
            recallDecisionNumber,
            productId,
            productBatchId,
            warehouseId,
            recallDate,
            level,
            note
        );
    }

    public Task UpdateAsync(
        SalesRecall salesRecall,
        Guid warehouseId,
        DateTime recallDate,
        RecallLevel level,
        string recallDecisionNumber,
        string? note)
    {
        salesRecall.UpdateInfo(warehouseId, recallDate, level, recallDecisionNumber, note);
        return Task.CompletedTask;
    }

    public Task CheckBeforeDeleteAsync(SalesRecall salesRecall)
    {
        if (salesRecall.Status != SalesRecallStatus.Draft && salesRecall.Status != SalesRecallStatus.Rejected)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể xóa quyết định thu hồi khi đang ở trạng thái Nháp hoặc Từ chối!");
        }
        return Task.CompletedTask;
    }

    public async Task AddLineAsync(
        SalesRecall salesRecall,
        Guid customerId,
        Guid salesOrderId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal originalUnitPrice,
        decimal taxRate)
    {
        await ValidateRecallQuantityAsync(salesRecall.Id, customerId, salesOrderId, salesRecall.ProductId, quantity, conversionFactor);

        salesRecall.AddLine(
            GuidGenerator.Create(),
            customerId,
            salesOrderId,
            unitId,
            conversionFactor,
            quantity,
            originalUnitPrice,
            taxRate
        );
    }

    public async Task UpdateLineAsync(
        SalesRecall salesRecall,
        Guid lineId,
        decimal quantity)
    {
        SalesRecallLine? line = salesRecall.Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng chi tiết thu hồi!");
        }

        await ValidateRecallQuantityAsync(salesRecall.Id, line.CustomerId, line.SalesOrderId, salesRecall.ProductId, quantity, line.ConversionFactor);

        salesRecall.UpdateLine(lineId, quantity);
    }

    public Task RemoveLineAsync(SalesRecall salesRecall, Guid lineId)
    {
        salesRecall.RemoveLine(lineId);
        return Task.CompletedTask;
    }

    public Task SendToApproveAsync(SalesRecall salesRecall)
    {
        salesRecall.SendToApprove();
        return Task.CompletedTask;
    }

    public async Task<InventoryTicket> ApproveAsync(SalesRecall salesRecall)
    {
        salesRecall.Approve();
        salesRecall.StartRecalling(); // Chuyển sang trạng thái Recalling

        // 1. Tự động sinh Phiếu nhập kho liên kết (TicketType = RecallReceipt)
        InventoryTicket ticket = await _ticketManager.CreateTicketAsync(
            TicketType.RecallReceipt,
            salesRecall.WarehouseId,
            salesRecall.Id,
            salesRecall.Code,
            $"Phiếu nhập kho thu hồi hàng lỗi cho chứng từ {salesRecall.Code}"
        );

        // Lưu ticket trước
        await _ticketRepo.InsertAsync(ticket);

        // 2. Tự động tạo các dòng phiếu kho tương ứng
        foreach (SalesRecallLine line in salesRecall.Lines)
        {
            InventoryTicketLine ticketLine = await _ticketManager.CreateTicketLineAsync(
                ticket,
                salesRecall.ProductId, // ID thuốc bị thu hồi
                line.Id, // Link ReferenceDocumentLineId to SalesRecallLine.Id
                line.Quantity,
                line.UnitId,
                line.ConversionFactor
            );
            await _ticketLineRepo.InsertAsync(ticketLine);
        }

        return ticket;
    }

    public Task RejectAsync(SalesRecall salesRecall)
    {
        salesRecall.Reject();
        return Task.CompletedTask;
    }

    public Task CompleteAsync(SalesRecall salesRecall)
    {
        salesRecall.Complete();
        return Task.CompletedTask;
    }

    private async Task ValidateRecallQuantityAsync(
        Guid salesRecallId,
        Guid customerId,
        Guid salesOrderId,
        Guid productId,
        decimal requestQty,
        int conversionFactor)
    {
        // Tìm dòng hàng bán gốc trong SalesOrder của đơn hàng
        IQueryable<SalesOrderLine> soLineQuery = await _soLineRepo.GetQueryableAsync();
        SalesOrderLine? soLine = await AsyncExecuter.FirstOrDefaultAsync(
            soLineQuery.Where(x => x.SalesOrderId == salesOrderId && x.ProductId == productId)
        );

        if (soLine == null)
        {
            throw new BusinessException(
                "SupplyCoreERP:ProductNotPurchased",
                "Sản phẩm thu hồi không tồn tại trong đơn hàng bán gốc đã chọn!"
            );
        }

        // Lấy tổng số lượng đã nhập thu hồi lũy tiến trước đó của khách hàng này cho đơn hàng này
        IQueryable<SalesRecallLine> lineQuery = await _lineRepo.GetQueryableAsync();
        IQueryable<SalesRecallLine> recalledQtyQuery = lineQuery.Where(x =>
            x.SalesOrderId == salesOrderId &&
            x.CustomerId == customerId &&
            x.SalesRecall.ProductId == productId &&
            x.SalesRecallId != salesRecallId &&
            x.SalesRecall.Status != SalesRecallStatus.Rejected);

        decimal alreadyRecalledBase = await AsyncExecuter.SumAsync(
            recalledQtyQuery,
            x => x.Quantity * x.ConversionFactor
        );

        decimal requestBase = requestQty * conversionFactor;

        if (alreadyRecalledBase + requestBase > soLine.BaseQuantity)
        {
            throw new BusinessException(
                "SupplyCoreERP:RecallQuantityExceedsLimit",
                $"Tổng số lượng nhập thu hồi vượt quá số lượng đã giao cho khách hàng này trên đơn bán gốc! " +
                $"Đã thu hồi trước đó: {alreadyRecalledBase:N2} (đơn vị gốc), " +
                $"Yêu cầu thu hồi thêm: {requestBase:N2} (đơn vị gốc), " +
                $"Định mức giao tối đa trên SO: {soLine.BaseQuantity:N2} (đơn vị gốc)."
            );
        }
    }
}
