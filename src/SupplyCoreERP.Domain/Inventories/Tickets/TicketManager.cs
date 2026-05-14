using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.Products;
using SupplyCoreERP.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventories.Tickets;

public class TicketManager : DomainService
{
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly IRepository<ProductBatch, Guid> _batchRepo;
    private readonly IRepository<Bin, Guid> _binRepo;
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepo;
    private readonly IRepository<PurchaseOrderLine, Guid> _poLineRepo;
    private readonly WarehouseManager _warehouseManager;
    private readonly InventoryBalanceManager _balanceManager;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly DocumentSequenceManager _documentSequenceManager;

    public TicketManager(
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        IRepository<InventoryBalance, Guid> balanceRepo,
        IRepository<ProductBatch, Guid> batchRepo,
        IRepository<Bin, Guid> binRepo,
        IRepository<Warehouse, Guid> warehouseRepo,
        IRepository<Product, Guid> productRepo,
        IRepository<PurchaseOrder, Guid> purchaseOrderRepo,
        IRepository<PurchaseOrderLine, Guid> poLineRepo,
        WarehouseManager warehouseManager,
        InventoryBalanceManager balanceManager,
        DocumentSequenceManager documentSequenceManager)
    {
        _ticketRepo = ticketRepo;
        _ticketLineRepo = ticketLineRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _balanceRepo = balanceRepo;
        _batchRepo = batchRepo;
        _binRepo = binRepo;
        _warehouseRepo = warehouseRepo;
        _productRepo = productRepo;
        _purchaseOrderRepo = purchaseOrderRepo;
        _poLineRepo = poLineRepo;
        _warehouseManager = warehouseManager;
        _balanceManager = balanceManager;
        _documentSequenceManager = documentSequenceManager;
    }

    #region Helpers
    private bool IsIssueTicket(TicketType type) =>
        type == TicketType.GoodsIssue || type == TicketType.DisposalIssue || type == TicketType.ReturnOutward;

    private bool IsIncomingTicket(TicketType type) =>
        type == TicketType.GoodsReceipt || type == TicketType.ReturnInward || type == TicketType.RecallReceipt;

    private async Task ValidateBinForIncomingAsync(Guid binId, Guid productId, Guid productBatchId)
    {
        IQueryable<Bin> binQuery = await _binRepo.WithDetailsAsync(b => b.Zone);
        Bin? bin = await AsyncExecuter.FirstOrDefaultAsync(binQuery.Where(b => b.Id == binId));

        if (bin == null)
        {
            throw new UserFriendlyException("Không tìm thấy vị trí (Bin)!");
        }

        Product product = await _productRepo.GetAsync(productId);
        _warehouseManager.ValidateStorageCompatibility(bin, product.RequiredStorageCondition);

        int usedSKUCount = await _balanceRepo.CountAsync(b => b.BinId == bin.Id && b.Quantity > 0);
        bool isNewSKU = !await _balanceRepo.AnyAsync(b => b.BinId == bin.Id && b.ProductId == productId && b.ProductBatchId == productBatchId);
        bin.ValidateSKUCapacity(usedSKUCount, isNewSKU);
    }

    private async Task ValidateBatchForIssueAsync(Guid productBatchId)
    {
        ProductBatch batch = await _batchRepo.GetAsync(productBatchId);
        if (batch.Status != BatchQAStatus.Approved)
        {
            throw new UserFriendlyException($"Lô hàng '{batch.BatchNumber}' chưa được QA duyệt hoặc đã bị thu hồi/hết hạn. Không thể xuất kho!");
        }

        if (batch.ExpiryDate <= DateTime.Now)
        {
            throw new UserFriendlyException($"Lô hàng '{batch.BatchNumber}' đã hết hạn sử dụng ({batch.ExpiryDate:dd/MM/yyyy}). Không thể xuất!");
        }
    }

    private async Task ValidateProductForInventoryAsync(Guid productId)
    {
        Product product = await _productRepo.GetAsync(productId);
        if (!product.IsAvailableForInventory)
        {
            throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa được duyệt. Không thể nhập/xuất kho!");
        }
    }

    private InventoryTransactionType MapTicketToTransaction(TicketType type)
    {
        return type switch
        {
            TicketType.GoodsReceipt => InventoryTransactionType.PurchaseReceipt,
            TicketType.GoodsIssue => InventoryTransactionType.SaleDelivery,
            TicketType.ReturnInward => InventoryTransactionType.ReturnInward,
            TicketType.ReturnOutward => InventoryTransactionType.ReturnOutward,
            TicketType.RecallReceipt => InventoryTransactionType.RecallReceipt,
            TicketType.DisposalIssue => InventoryTransactionType.Disposal,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<bool> HasStatusAsync(Guid referenceId, ApprovalStatus status)
    {
        InventoryTicket? ticket = await _ticketRepo.FirstOrDefaultAsync(x => x.ReferenceDocumentId == referenceId);
        return ticket?.Status == status;
    }
    #endregion

    #region Ticket
    public async Task<InventoryTicket> CreateTicketAsync(TicketType type, Guid warehouseId, Guid? referenceDocumentId, string? referenceDocumentNumber, string? note)
    {
        Warehouse warehouse = await _warehouseRepo.GetAsync(warehouseId);
        if (!warehouse.IsActive)
        {
            throw new UserFriendlyException($"Kho '{warehouse.Name}' đang bị tạm khóa!");
        }

        int draftCount = await _ticketRepo.CountAsync(x => x.WarehouseId == warehouseId && x.Type == type && x.Status == ApprovalStatus.Draft);
        if (draftCount >= 10)
        {
            throw new UserFriendlyException("Kho đang có quá nhiều phiếu Nháp chưa được xử lý!");
        }

        string ticketNumber = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeInventoryTicket);

        return new InventoryTicket(GuidGenerator.Create(), ticketNumber, type, warehouseId, referenceDocumentId, referenceDocumentNumber, note);
    }

    public void UpdateTicket(InventoryTicket ticket, string? note)
    {
        if (ticket.Status == ApprovalStatus.Approved)
        {
            throw new UserFriendlyException("Không thể sửa Phiếu đã thực thi!");
        }

        ticket.UpdateNote(note);
    }

    public async Task ValidateBeforeDeleteAsync(InventoryTicket ticket)
    {
        if (ticket.Status == ApprovalStatus.Approved)
        {
            throw new UserFriendlyException("Không thể xóa Phiếu đã duyệt!");
        }

        if (IsIssueTicket(ticket.Type))
        {
            // Lock details
            List<InventoryTicketDetail> details = await _ticketDetailRepo.GetListAsync(x => x.TicketLine.TicketId == ticket.Id);
            await _balanceManager.UnlockStockAsync(ticket);
        }
    }
    #endregion

    # region Ticket Line
    public async Task<InventoryTicketLine> CreateTicketLineAsync(
        InventoryTicket ticket, Guid productId, Guid? purchaseOrderLineId, decimal quantity, Guid? unitId = null, int? conversionFactor = null)
    {
        if (ticket.Status == ApprovalStatus.Approved || ticket.Status == ApprovalStatus.Rejected)
        {
            throw new UserFriendlyException("Không thể thao tác trên phiếu đã Duyệt hoặc Từ chối!");
        }

        Product product = await _productRepo.GetAsync(productId);
        if (!product.IsAvailableForInventory)
        {
            throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa được duyệt. Không thể nhập/xuất kho!");
        }

        Guid finalUnitId = unitId ?? product.BaseUnitId;
        int finalConversionFactor = conversionFactor ?? 1;

        if (purchaseOrderLineId.HasValue)
        {
            PurchaseOrderLine poLine = await _poLineRepo.GetAsync(purchaseOrderLineId.Value);
            if (poLine.ProductId != productId)
            {
                throw new UserFriendlyException("Sản phẩm không khớp với dòng đơn hàng!");
            }
            // Ưu tiên lấy từ PO nếu có truyền vào hoặc mặc định từ PO Line
            finalUnitId = unitId ?? poLine.UnitId;
            finalConversionFactor = conversionFactor ?? poLine.ConversionFactor;
        }

        var line = new InventoryTicketLine(GuidGenerator.Create(), ticket.Id, productId, finalUnitId, finalConversionFactor, purchaseOrderLineId, quantity);
        return line;
    }

    public void UpdateLineQuantity(InventoryTicket ticket, InventoryTicketLine line, decimal newQuantity)
    {
        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ có thể sửa số lượng khi phiếu ở trạng thái Nháp!");
        }

        line.UpdateQuantity(newQuantity);
    }
    #endregion

    #region Ticket Detail
    public async Task<InventoryTicketDetail> CreateTicketDetailAsync(
        InventoryTicket ticket, InventoryTicketLine line, Guid productId, Guid productBatchId, Guid binId, Guid unitId, int conversionFactor, decimal quantity)
    {
        if (ticket.Status == ApprovalStatus.Approved || ticket.Status == ApprovalStatus.Rejected)
        {
            throw new UserFriendlyException("Không thể thao tác trên phiếu đã Duyệt hoặc Từ chối!");
        }

        if (line.ProductId != productId)
        {
            throw new UserFriendlyException("Sản phẩm chi tiết không khớp với dòng phiếu kho!");
        }

        await ValidateProductForInventoryAsync(productId);
        Bin bin = await _binRepo.GetAsync(binId);
        if (bin.WarehouseId != ticket.WarehouseId)
        {
            throw new UserFriendlyException("Vị trí (Bin) không thuộc kho của phiếu này!");
        }

        if (IsIncomingTicket(ticket.Type))
        {
            await ValidateBinForIncomingAsync(binId, productId, productBatchId);
        }

        if (IsIssueTicket(ticket.Type))
        {
            await ValidateBatchForIssueAsync(productBatchId);
        }

        decimal baseQty = quantity * conversionFactor;

        if (ticket.Status == ApprovalStatus.Pending && IsIssueTicket(ticket.Type))
        {
            await _balanceManager.AdjustLockAsync(ticket, binId, productId, productBatchId, baseQty);
        }

        return new InventoryTicketDetail(GuidGenerator.Create(), line.Id, productId, productBatchId, binId, unitId, conversionFactor, quantity);
    }

    public async Task UpdateDetailQuantityAsync(InventoryTicket ticket, InventoryTicketLine line, InventoryTicketDetail detail, decimal actualQuantity)
    {
        if (ticket.Status == ApprovalStatus.Approved)
        {
            throw new UserFriendlyException("Không thể sửa chi tiết của Phiếu đã thực thi!");
        }

        decimal oldBaseQty = detail.BaseQuantity;
        decimal newBaseQty = actualQuantity * detail.ConversionFactor;
        decimal diff = newBaseQty - oldBaseQty;

        if (ticket.Status == ApprovalStatus.Pending && IsIssueTicket(ticket.Type))
        {
            await _balanceManager.AdjustLockAsync(ticket, detail.BinId, detail.ProductId, detail.ProductBatchId, diff);
        }

        detail.UpdateActualQuantity(actualQuantity);
    }

    public async Task RemoveTicketDetailAsync(InventoryTicket ticket, InventoryTicketLine line, InventoryTicketDetail detail)
    {
        if (ticket.Status == ApprovalStatus.Approved)
        {
            throw new UserFriendlyException("Không thể xóa chi tiết của Phiếu đã duyệt!");
        }

        if (ticket.Status == ApprovalStatus.Pending && IsIssueTicket(ticket.Type))
        {
            await _balanceManager.AdjustLockAsync(ticket, detail.BinId, detail.ProductId, detail.ProductBatchId, -detail.BaseQuantity);
        }
    }
    #endregion

    #region Ticket Workflow
    public async Task SendToApproveAsync(InventoryTicket ticket)
    {
        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ gửi duyệt phiếu Nháp!");
        }

        IQueryable<InventoryTicketLine> lines = await _ticketLineRepo.WithDetailsAsync(x => x.Details);
        var ticketLines = lines.Where(x => x.TicketId == ticket.Id).ToList();

        if (!ticketLines.Any())
        {
            throw new UserFriendlyException("Phiếu kho chưa có hàng hóa!");
        }

        var allDetails = ticketLines.SelectMany(x => x.Details).ToList();
        if (!allDetails.Any())
        {
            throw new UserFriendlyException("Phiếu kho chưa có chi tiết lô/vị trí!");
        }

        // ✅ Validation quan trọng: Tổng chi tiết phải bằng đúng số lượng của Line
        foreach (InventoryTicketLine? line in ticketLines)
        {
            decimal detailedQty = line.Details.Sum(x => x.BaseQuantity);
            if (detailedQty != line.Quantity)
            {
                Product product = await _productRepo.GetAsync(line.ProductId);
                throw new UserFriendlyException($"Sản phẩm '{product.Name}' có tổng chi tiết ({detailedQty}) không khớp với số lượng dòng hàng ({line.Quantity})!");
            }
        }

        if (IsIssueTicket(ticket.Type))
        {
            await _balanceManager.LockStockAsync(ticket, allDetails);
        }

        ticket.RequestApprove();
    }

    public async Task RejectTicketAsync(InventoryTicket ticket, string rejectReason)
    {
        if (ticket.Status != ApprovalStatus.Pending)
        {
            throw new UserFriendlyException("Chỉ từ chối phiếu chờ duyệt!");
        }

        if (IsIssueTicket(ticket.Type))
        {
            await _balanceManager.UnlockStockAsync(ticket);
        }

        ticket.Reject();
        ticket.UpdateNote($"[Từ chối: {rejectReason}] " + ticket.Note);
    }

    public async Task ExecuteTicketAsync(InventoryTicket ticket)
    {
        if (ticket.Status != ApprovalStatus.Pending)
        {
            throw new UserFriendlyException("Chỉ thực thi phiếu chờ duyệt!");
        }

        IQueryable<InventoryTicketLine> lines = await _ticketLineRepo.WithDetailsAsync(x => x.Details);
        var ticketLines = lines.Where(x => x.TicketId == ticket.Id).ToList();
        var allDetails = ticketLines.SelectMany(x => x.Details).ToList();

        InventoryTransactionType transType = MapTicketToTransaction(ticket.Type);
        await _balanceManager.ExecuteStockMovementAsync(ticket, allDetails, transType, IsIssueTicket(ticket.Type));

        // Cập nhật ReceivedQuantity cho PurchaseOrderLine và kiểm tra đóng PO
        if (ticket.Type == TicketType.GoodsReceipt && ticket.ReferenceDocumentId.HasValue)
        {
            await SyncPurchaseOrderProgressAsync(ticket.ReferenceDocumentId.Value, ticketLines);
        }

        ticket.Execute();
    }

    // Auxiliary method to handle PO progress
    public async Task SyncPurchaseOrderProgressAsync(Guid poId, List<InventoryTicketLine> ticketLines)
    {
        IQueryable<PurchaseOrder> poQuery = await _purchaseOrderRepo.WithDetailsAsync(x => x.Lines);
        PurchaseOrder? po = await AsyncExecuter.FirstOrDefaultAsync(poQuery.Where(x => x.Id == poId));
        if (po == null) return;

        foreach (InventoryTicketLine tLine in ticketLines)
        {
            if (tLine.PurchaseOrderLineId.HasValue)
            {
                PurchaseOrderLine? poLine = po.Lines.FirstOrDefault(x => x.Id == tLine.PurchaseOrderLineId.Value);
                if (poLine != null)
                {
                    // tLine.Quantity luôn là đơn vị cơ bản, quy đổi về đơn vị PO để cộng vào ReceivedQuantity
                    poLine.AddReceivedQuantity(Math.Round(tLine.Quantity / poLine.ConversionFactor, 4));
                }
            }
        }

        // Cập nhật trạng thái PO
        if (po.Lines.Any(x => x.ReceivedQuantity > 0))
        {
            if (po.Status != PurchaseOrderStatus.Receiving && po.Status != PurchaseOrderStatus.Completed)
            {
                po.StartReceiving();
            }
        }

        await _purchaseOrderRepo.UpdateAsync(po);
    }
    #endregion

    #region FEFO
    public async Task<IList<InventoryTicketDetail>> AllocateFEFOAsync(InventoryTicket ticket, Guid productId, decimal requiredBaseQuantity)
    {
        await ValidateProductForInventoryAsync(productId);

        Product product = await _productRepo.GetAsync(productId);

        // Tạo một Line mới cho sản phẩm này trong Ticket - Sử dụng Base Unit
        var line = new InventoryTicketLine(GuidGenerator.Create(), ticket.Id, productId, product.BaseUnitId, 1, null, requiredBaseQuantity);
        await _ticketLineRepo.InsertAsync(line);

        // Logic FEFO: Lấy từ InventoryBalance
        List<InventoryBalance> balances = await _balanceRepo.GetListAsync(x => x.WarehouseId == ticket.WarehouseId && x.ProductId == productId && x.Quantity > x.LockedQuantity);

        var batchIds = balances.Select(x => x.ProductBatchId).Distinct().ToList();
        List<ProductBatch> batches = await _batchRepo.GetListAsync(x => batchIds.Contains(x.Id) && x.Status == BatchQAStatus.Approved && x.ExpiryDate > DateTime.Now);

        var validBalances = (from b in balances
                             join ba in batches on b.ProductBatchId equals ba.Id
                             orderby ba.ExpiryDate ascending
                             select b).ToList();

        var details = new List<InventoryTicketDetail>();
        decimal remaining = requiredBaseQuantity;

        foreach (InventoryBalance? balance in validBalances)
        {
            if (remaining <= 0) break;

            decimal available = balance.Quantity - balance.LockedQuantity;
            decimal toTake = Math.Min(available, remaining);

            var detail = new InventoryTicketDetail(GuidGenerator.Create(), line.Id, productId, balance.ProductBatchId, balance.BinId, product.BaseUnitId, 1, toTake);
            details.Add(detail);

            remaining -= toTake;
        }

        if (remaining > 0)
        {
            throw new UserFriendlyException("Không đủ tồn kho khả dụng để cấp phát FEFO!");
        }

        await _ticketDetailRepo.InsertManyAsync(details);

        return details;
    }
    #endregion
}
