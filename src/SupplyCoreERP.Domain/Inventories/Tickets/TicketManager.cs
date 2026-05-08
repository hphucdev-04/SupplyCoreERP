using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Balances;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.Products;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Warehouses;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventories.Tickets;

public class TicketManager : DomainService
{
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly IRepository<ProductBatch, Guid> _batchRepo;
    private readonly IRepository<Bin, Guid> _binRepo;
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly WarehouseManager _warehouseManager;
    private readonly InventoryBalanceManager _balanceManager;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly DocumentSequenceManager _documentSequenceManager;

    public TicketManager(
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        IRepository<InventoryBalance, Guid> balanceRepo,
        IRepository<ProductBatch, Guid> batchRepo,
        IRepository<Bin, Guid> binRepo,
        IRepository<Warehouse, Guid> warehouseRepo,
        IRepository<Product, Guid> productRepo,
        WarehouseManager warehouseManager,
        InventoryBalanceManager balanceManager,
        DocumentSequenceManager documentSequenceManager)
    {
        _ticketRepo = ticketRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _balanceRepo = balanceRepo;
        _batchRepo = batchRepo;
        _binRepo = binRepo;
        _warehouseRepo = warehouseRepo;
        _productRepo = productRepo;
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

        var usedSKUCount = await _balanceRepo.CountAsync(b => b.BinId == bin.Id && b.Quantity > 0);
        var isNewSKU = !await _balanceRepo.AnyAsync(b => b.BinId == bin.Id && b.ProductId == productId && b.ProductBatchId == productBatchId);
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

        var draftCount = await _ticketRepo.CountAsync(x => x.WarehouseId == warehouseId && x.Type == type && x.Status == ApprovalStatus.Draft);
        if (draftCount >= 10)
        {
            throw new UserFriendlyException("Kho đang có quá nhiều phiếu Nháp chưa được xử lý!");
        }

        var prefix = type.ToString().Substring(0, 3).ToUpper();
        var ticketNumber = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeInventoryTicket);

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
            await _balanceManager.UnlockStockAsync(ticket);
        }
    }
    #endregion

    #region Ticket Detail
    public async Task<InventoryTicketDetail> CreateTicketDetailAsync(
        InventoryTicket ticket, Guid productId, Guid productBatchId, Guid binId, Guid unitId, int conversionFactor, decimal quantity)
    {
        if (ticket.Status == ApprovalStatus.Approved || ticket.Status == ApprovalStatus.Rejected)
        {
            throw new UserFriendlyException("Không thể thao tác trên phiếu đã Duyệt hoặc Từ chối!");
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

        var baseQty = quantity * conversionFactor;
        if (ticket.Status == ApprovalStatus.Pending && IsIssueTicket(ticket.Type))
        {
            await _balanceManager.AdjustLockAsync(ticket, binId, productId, productBatchId, baseQty);
        }

        return new InventoryTicketDetail(GuidGenerator.Create(), ticket.Id, productId, productBatchId, binId, unitId, conversionFactor, quantity);
    }

    public async Task UpdateDetailQuantityAsync(InventoryTicket ticket, InventoryTicketDetail detail, decimal actualQuantity)
    {
        if (ticket.Status == ApprovalStatus.Approved)
        {
            throw new UserFriendlyException("Không thể sửa chi tiết của Phiếu đã thực thi!");
        }

        if (ticket.Status == ApprovalStatus.Pending && IsIssueTicket(ticket.Type))
        {
            var newBaseQty = actualQuantity * detail.ConversionFactor;
            var diff = newBaseQty - detail.BaseQuantity;
            await _balanceManager.AdjustLockAsync(ticket, detail.BinId, detail.ProductId, detail.ProductBatchId, diff);
        }

        detail.UpdateActualQuantity(actualQuantity);
    }

    public async Task RemoveTicketDetailAsync(InventoryTicket ticket, InventoryTicketDetail detail)
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

        List<InventoryTicketDetail> details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);
        if (!details.Any())
        {
            throw new UserFriendlyException("Phiếu kho chưa có hàng hóa!");
        }

        if (IsIssueTicket(ticket.Type))
        {
            await _balanceManager.LockStockAsync(ticket, details);
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

    public async Task ExecuteTicketAsync(InventoryTicket ticket, IList<InventoryTicketDetail> details)
    {
        if (ticket.Status != ApprovalStatus.Pending)
        {
            throw new UserFriendlyException("Chỉ thực thi phiếu chờ duyệt!");
        }

        InventoryTransactionType transType = MapTicketToTransaction(ticket.Type);
        await _balanceManager.ExecuteStockMovementAsync(ticket, details, transType, IsIssueTicket(ticket.Type));

        ticket.Execute();
    }
    #endregion

    #region FEFO
    public async Task<IList<InventoryTicketDetail>> AllocateFEFOAsync(InventoryTicket ticket, Guid productId, decimal requiredBaseQuantity)
    {
        Product product = await _productRepo.GetAsync(productId);
        if (!product.IsAvailableForInventory)
        {
            throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa được duyệt.");
        }

        var remaining = requiredBaseQuantity;
        DateTime now = DateTime.Now;

        var query =
            from bal in await _balanceRepo.GetQueryableAsync()
            join bat in await _batchRepo.GetQueryableAsync() on bal.ProductBatchId equals bat.Id
            join bin in await _binRepo.GetQueryableAsync() on bal.BinId equals bin.Id
            where bal.WarehouseId == ticket.WarehouseId
               && bal.ProductId == productId
               && (bal.Quantity - bal.LockedQuantity) > 0
               && bat.Status == BatchQAStatus.Approved
               && !bin.IsBlocked
               && bat.ExpiryDate > now
            orderby bat.ExpiryDate ascending, (bal.Quantity - bal.LockedQuantity) ascending
            select new { bal, bat, bin };

        var stocks = await AsyncExecuter.ToListAsync(query);
        var detailsToReturn = new List<InventoryTicketDetail>();

        foreach (var stock in stocks)
        {
            if (remaining <= 0)
            {
                break;
            }

            var take = Math.Min(stock.bal.Quantity - stock.bal.LockedQuantity, remaining);
            detailsToReturn.Add(new InventoryTicketDetail(
                GuidGenerator.Create(), ticket.Id, productId,
                stock.bat.Id, stock.bin.Id, product.BaseUnitId, 1, take));
            remaining -= take;
        }

        if (remaining > 0)
        {
            throw new UserFriendlyException(
                $"Không đủ tồn kho! Còn thiếu {remaining:N0} {product.BaseUnit?.Name ?? "đơn vị"}.");
        }

        // Lock stock nếu ticket đã ở trạng thái Pending (gọi thủ công từ AppService)
        if (detailsToReturn.Any() && ticket.Status == ApprovalStatus.Pending)
        {
            await _balanceManager.LockStockAsync(ticket, detailsToReturn);
        }

        return detailsToReturn;
    }
    #endregion
}
