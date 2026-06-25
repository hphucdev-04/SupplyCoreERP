using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Sales.SalesRecalls;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventory.Tickets;

public class TicketManager : DomainService, ITicketManager
{
    private static readonly Dictionary<TicketType, HashSet<ZoneType>> TicketZoneRules = new()
    {
        { TicketType.GoodsReceipt,  new() { ZoneType.QA } },
        { TicketType.GoodsIssue,    new() { ZoneType.Storage, ZoneType.QA } },
        { TicketType.RecallReceipt, new() { ZoneType.Quarantine } },
        { TicketType.ReturnOutward, new() { ZoneType.Quarantine } }
    };
    // Dependencies
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly IRepository<ProductBatch, Guid> _batchRepo;
    private readonly IRepository<Bin, Guid> _binRepo;
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<PurchaseOrderLine, Guid> _poLineRepo;
    private readonly IRepository<SalesOrderLine, Guid> _soLineRepo;
    private readonly IRepository<PurchaseReturnLine, Guid> _purchaseReturnLineRepo;
    private readonly IRepository<SalesRecallLine, Guid> _salesRecallLineRepo;
    private readonly IRepository<SalesRecall, Guid> _salesRecallRepo;
    private readonly WarehouseManager _warehouseManager;
    private readonly InventoryBalanceManager _balanceManager;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly IDocumentSequenceManager _documentSequenceManager;
    private readonly UnitConversionManager _unitConversionManager;

    // Constructor injection
    public TicketManager(
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        IRepository<InventoryBalance, Guid> balanceRepo,
        IRepository<ProductBatch, Guid> batchRepo,
        IRepository<Bin, Guid> binRepo,
        IRepository<Warehouse, Guid> warehouseRepo,
        IRepository<Product, Guid> productRepo,
        IRepository<PurchaseOrderLine, Guid> poLineRepo,
        IRepository<SalesOrderLine, Guid> soLineRepo,
        IRepository<PurchaseReturnLine, Guid> purchaseReturnLineRepo,
        IRepository<SalesRecallLine, Guid> salesRecallLineRepo,
        IRepository<SalesRecall, Guid> salesRecallRepo,
        WarehouseManager warehouseManager,
        InventoryBalanceManager balanceManager,
        IDocumentSequenceManager documentSequenceManager,
        UnitConversionManager unitConversionManager)
    {
        _ticketRepo = ticketRepo;
        _ticketLineRepo = ticketLineRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _balanceRepo = balanceRepo;
        _batchRepo = batchRepo;
        _binRepo = binRepo;
        _warehouseRepo = warehouseRepo;
        _productRepo = productRepo;
        _poLineRepo = poLineRepo;
        _soLineRepo = soLineRepo;
        _purchaseReturnLineRepo = purchaseReturnLineRepo;
        _salesRecallLineRepo = salesRecallLineRepo;
        _salesRecallRepo = salesRecallRepo;
        _warehouseManager = warehouseManager;
        _balanceManager = balanceManager;
        _documentSequenceManager = documentSequenceManager;
        _unitConversionManager = unitConversionManager;
    }

    #region Helpers
    private bool IsIssueTicket(TicketType type) =>
        type == TicketType.GoodsIssue || type == TicketType.DisposalIssue || type == TicketType.ReturnOutward;

    private bool IsIncomingTicket(TicketType type) =>
        type == TicketType.GoodsReceipt || type == TicketType.ReturnInward || type == TicketType.RecallReceipt;

    private async Task ValidateBinForIncomingAsync(Guid binId, Guid productId, Guid productBatchId, Guid unitId, decimal quantity)
    {
        IQueryable<Bin> binQuery = await _binRepo.WithDetailsAsync(b => b.Zone);
        Bin? bin = await AsyncExecuter.FirstOrDefaultAsync(binQuery.Where(b => b.Id == binId));

        if (bin == null)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", "Không tìm thấy vị trí (Bin)!");
        }

        IQueryable<Product> productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
        Product? product = await AsyncExecuter.FirstOrDefaultAsync(productQuery.Where(p => p.Id == productId));
        if (product == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), productId);
        }
        _warehouseManager.ValidateStorageCompatibility(bin, product.RequiredStorageCondition);

        // 1. Kiểm tra giới hạn SKU tối đa
        IQueryable<InventoryBalance> balancesInBinQuery = await _balanceRepo.GetQueryableAsync();
        int usedSKUCount = await AsyncExecuter.CountAsync(
            balancesInBinQuery
                .Where(b => b.BinBalances.Any(bb => bb.BinId == bin.Id && bb.Quantity > 0))
                .Select(b => b.ProductId)
                .Distinct()
        );

        bool isNewSKU = !await _balanceRepo.AnyAsync(b => b.ProductId == productId && b.BinBalances.Any(bb => bb.BinId == bin.Id && bb.Quantity > 0));
        bin.ValidateSKUCapacity(usedSKUCount, isNewSKU);

        // 2. Kiểm tra giới hạn Thể tích tối đa
        if (bin.MaxVolume > 0)
        {
            decimal newVolume = _unitConversionManager.CalculateVolume(product, unitId, quantity);

            IQueryable<InventoryBalance> balancesQuery = await _balanceRepo.WithDetailsAsync(x => x.BinBalances, x => x.Product);
            balancesQuery = balancesQuery
                .Where(x => x.BinBalances.Any(bb => bb.BinId == binId));

            List<InventoryBalance> balancesInBin = await AsyncExecuter.ToListAsync(balancesQuery);
            decimal currentVolume = 0;
            foreach (InventoryBalance balance in balancesInBin)
            {
                InventoryBinBalance? binBalance = balance.BinBalances.FirstOrDefault(bb => bb.BinId == binId);
                if (binBalance != null && binBalance.Quantity > 0)
                {
                    currentVolume += binBalance.Quantity * balance.Product.BaseUnitVolume;
                }
            }

            bin.ValidateVolumeCapacity(currentVolume, newVolume);
        }
    }

    private async Task ValidateBatchForIssueAsync(Guid productBatchId)
    {
        ProductBatch batch = await _batchRepo.GetAsync(productBatchId);
        if (batch.Status != BatchQAStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBatch", $"Lô hàng '{batch.BatchNumber}' chưa được QA duyệt hoặc đã bị thu hồi/hết hạn. Không thể xuất kho!");
        }

        if (batch.ExpiryDate <= DateTime.Now)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBatch", $"Lô hàng '{batch.BatchNumber}' đã hết hạn sử dụng ({batch.ExpiryDate:dd/MM/yyyy}). Không thể xuất!");
        }
    }

    private async Task ValidateProductForInventoryAsync(Guid productId, TicketType ticketType)
    {
        bool isRecallOrReturn = ticketType == TicketType.RecallReceipt ||
                               ticketType == TicketType.ReturnInward ||
                               ticketType == TicketType.ReturnOutward;

        if (isRecallOrReturn)
        {
            return;
        }

        Product product = await _productRepo.GetAsync(productId);
        if (!product.IsAvailableForInventory)
        {
            throw new BusinessException("SupplyCoreERP:InvalidProduct", $"Sản phẩm '{product.Name}' chưa được duyệt. Không thể nhập/xuất kho!");
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
            throw new BusinessException("SupplyCoreERP:InvalidWarehouse", $"Kho '{warehouse.Name}' đang bị tạm khóa!");
        }

        int draftCount = await _ticketRepo.CountAsync(x => x.WarehouseId == warehouseId && x.Type == type && x.Status == ApprovalStatus.Draft);
        if (draftCount >= 10)
        {
            throw new BusinessException("SupplyCoreERP:TooManyDraftTickets", "Kho đang có quá nhiều phiếu Nhập chưa được xử lý!");
        }

        string ticketNumber = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeInventoryTicket);

        return new InventoryTicket(GuidGenerator.Create(), ticketNumber, type, warehouseId, referenceDocumentId, referenceDocumentNumber, note);
    }

    public void UpdateTicket(InventoryTicket ticket, string? note)
    {
        if (ticket.Status == ApprovalStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Khôn th thể sửa Phiếu đã thực thi!");
        }

        ticket.UpdateNote(note);
    }

    public async Task ValidateBeforeDeleteAsync(InventoryTicket ticket)
    {
        if (ticket.Status == ApprovalStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Khôn th thể xóa Phiếu đã duyệt!");
        }

        if (IsIssueTicket(ticket.Type))
        {
            await _balanceManager.UnlockStockAsync(ticket);
        }
    }
    #endregion

    # region Ticket Line
    public async Task<InventoryTicketLine> CreateTicketLineAsync(
        InventoryTicket ticket,
        Guid productId,
        Guid? referenceDocumentLineId,
        decimal quantity,
        Guid? unitId = null,
        int? conversionFactor = null)
    {
        if (ticket.Status == ApprovalStatus.Approved || ticket.Status == ApprovalStatus.Rejected)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Khôn th thể thao tác trên phiếu đã Duyệt hoặc Từ chối!");
        }

        Product product = await _productRepo.GetAsync(productId);
        bool isRecallOrReturn = ticket.Type == TicketType.RecallReceipt ||
                               ticket.Type == TicketType.ReturnInward ||
                               ticket.Type == TicketType.ReturnOutward;

        if (!isRecallOrReturn && !product.IsAvailableForInventory)
        {
            throw new BusinessException("SupplyCoreERP:InvalidProduct", $"Sản phẩm '{product.Name}' chưa được duyệt. Không thể nhập/xuất kho!");
        }

        Guid finalUnitId = unitId ?? product.BaseUnitId;
        int finalConversionFactor = conversionFactor ?? 1;

        if (referenceDocumentLineId.HasValue)
        {
            if (ticket.Type == TicketType.GoodsReceipt)
            {
                PurchaseOrderLine poLine = await _poLineRepo.GetAsync(referenceDocumentLineId.Value);
                if (poLine.ProductId != productId)
                {
                    throw new BusinessException("SupplyCoreERP:InvalidProduct", "Sản phẩm không khớp với dòng đơn mua!");
                }
                finalUnitId = unitId ?? poLine.UnitId;
                finalConversionFactor = conversionFactor ?? poLine.ConversionFactor;
            }
            else if (ticket.Type == TicketType.GoodsIssue)
            {
                SalesOrderLine soLine = await _soLineRepo.GetAsync(referenceDocumentLineId.Value);
                if (soLine.ProductId != productId)
                {
                    throw new BusinessException("SupplyCoreERP:InvalidProduct", "Sản phẩm không khớp với dòng đơn bán!");
                }
                finalUnitId = unitId ?? soLine.UnitId;
                finalConversionFactor = conversionFactor ?? soLine.ConversionFactor;
            }
            else if (ticket.Type == TicketType.ReturnOutward)
            {
                PurchaseReturnLine prLine = await _purchaseReturnLineRepo.GetAsync(referenceDocumentLineId.Value);
                if (prLine.ProductId != productId)
                {
                    throw new BusinessException("SupplyCoreERP:InvalidProduct", "Sản phẩm không khớp với dòng phiếu trả hàng mua!");
                }
                finalUnitId = unitId ?? prLine.UnitId;
                finalConversionFactor = conversionFactor ?? prLine.ConversionFactor;
            }
            else if (ticket.Type == TicketType.RecallReceipt)
            {
                SalesRecallLine recallLine = await _salesRecallLineRepo.GetAsync(referenceDocumentLineId.Value);
                SalesRecall recall = await _salesRecallRepo.GetAsync(recallLine.SalesRecallId);
                if (recall.ProductId != productId)
                {
                    throw new BusinessException("SupplyCoreERP:InvalidProduct", "Sản phẩm không khớp với phiếu thu hồi hàng bán!");
                }
                finalUnitId = unitId ?? recallLine.UnitId;
                finalConversionFactor = conversionFactor ?? recallLine.ConversionFactor;
            }
        }

        InventoryTicketLine line = new(GuidGenerator.Create(), ticket.Id, productId, finalUnitId, finalConversionFactor, referenceDocumentLineId, quantity);

        return line;
    }


    public void UpdateLineQuantity(InventoryTicket ticket, InventoryTicketLine line, decimal newQuantity)
    {
        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Chỉ có thể sửa số lượng khi phiếu ở trạng thái Nháp!");
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
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Khôn th thể thao tác trên phiếu đã Duyệt hoặc Từ chối!");
        }

        if (line.ProductId != productId)
        {
            throw new BusinessException("SupplyCoreERP:InvalidProduct", "Sản phẩm chi tiết không khớp với dòng phiếu kho!");
        }

        await ValidateProductForInventoryAsync(productId, ticket.Type);
        IQueryable<Bin> binQuery = await _binRepo.WithDetailsAsync(b => b.Zone);
        Bin bin = await AsyncExecuter.FirstOrDefaultAsync(binQuery, b => b.Id == binId);
        if (bin == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Bin), binId);
        }

        if (bin.Zone == null)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", "Lỗi hệ thống: Không tải được thông tin Zone!");
        }

        if (TicketZoneRules.TryGetValue(ticket.Type, out HashSet<ZoneType>? allowedZones)
            && !allowedZones.Contains(bin.Zone.Type))
        {
            throw new BusinessException(SupplyCoreERPDomainErrorCodes.InvalidZoneForTicketType)
                .WithData("ticketType", ticket.Type.ToString())
                .WithData("zoneType", bin.Zone.Type.ToString())
                .WithData("allowedZones", string.Join(", ", allowedZones));
        }

        if (bin.WarehouseId != ticket.WarehouseId)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBin", "Vị trí (Bin) không thuộc kho của phiếu này!");
        }

        if (IsIncomingTicket(ticket.Type))
        {
            await ValidateBinForIncomingAsync(binId, productId, productBatchId, unitId, quantity);
        }

        if (ticket.Type == TicketType.GoodsIssue)
        {
            await ValidateBatchForIssueAsync(productBatchId);
        }

        IQueryable<Product> productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
        Product product = await AsyncExecuter.FirstOrDefaultAsync(productQuery, p => p.Id == productId);
        if (product == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), productId);
        }

        // Kiểm tra chéo ConversionFactor với GetConversionFactor của hệ thống quy đổi đơn vị tuyến tính
        int expectedAbsoluteFactor = _unitConversionManager.GetConversionFactor(product, unitId);
        if (conversionFactor != expectedAbsoluteFactor)
        {
            throw new BusinessException(
                "SupplyCoreERP:InvalidConversionFactor",
                $"Hệ số quy đổi không hợp lệ so với cấu hình sản phẩm '{product.Name}'. Nhận từ Client: {conversionFactor}, Kỳ vọng hệ số tuyệt đối: {expectedAbsoluteFactor}."
            );
        }

        decimal baseQty = _unitConversionManager.ConvertToBaseQuantity(product, unitId, quantity);

        // Kiểm tra tổng số lượng chi tiết đã phân bổ cho dòng hàng này, đảm bảo không vượt quá số lượng của dòng
        IQueryable<InventoryTicketDetail> detailQuery = await _ticketDetailRepo.GetQueryableAsync();
        decimal currentDetailedBaseQty = detailQuery.Where(d => d.TicketLineId == line.Id).Sum(d => d.Quantity * d.ConversionFactor);

        if (currentDetailedBaseQty + baseQty > line.BaseQuantity)
        {
            throw new BusinessException("SupplyCoreERP:InvalidDetail", $"Khôn th thể th thêm chi tiết cho '{product.Name}'. Tổng số lượng phân bổ ({currentDetailedBaseQty + baseQty}) vượt quá số lượng yêu cầu của dòng hàng ({line.BaseQuantity})!");
        }

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
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Khôn th thể sá»­a chi tiáº¿t cá»§a Phiáº¿u Ä‘Ã£ thá»±c thi!");
        }

        decimal oldBaseQty = detail.BaseQuantity;
        IQueryable<Product> productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
        Product product = await AsyncExecuter.FirstOrDefaultAsync(productQuery, p => p.Id == detail.ProductId);
        if (product == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Product), detail.ProductId);
        }

        decimal newBaseQty = _unitConversionManager.ConvertToBaseQuantity(product, detail.UnitId, actualQuantity);
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
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Khôn th thể xóa chi tiết của Phiếu đã duyệt!");
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
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Chỉ có thể gửi duyệt phiếu Nháp!");
        }

        IQueryable<InventoryTicketLine> lines = await _ticketLineRepo.WithDetailsAsync(x => x.Details);
        List<InventoryTicketLine> ticketLines = lines.Where(x => x.TicketId == ticket.Id).ToList();

        if (!ticketLines.Any())
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Phiếu kho chưa có hàng hóa!");
        }

        List<InventoryTicketDetail> allDetails = ticketLines.SelectMany(x => x.Details).ToList();
        if (!allDetails.Any())
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Phiếu kho chưa có chi tiết lô/vị trí!");
        }

        foreach (InventoryTicketLine? line in ticketLines)
        {
            decimal detailedQty = line.Details.Sum(x => x.BaseQuantity);
            if (detailedQty != line.BaseQuantity)
            {
                Product product = await _productRepo.GetAsync(line.ProductId);
                throw new BusinessException("SupplyCoreERP:InvalidDetail", $"Sản phẩm '{product.Name}' có tổng chi tiết ({detailedQty}) không khớp với số lượng dòng hàng ({line.BaseQuantity})!");
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
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Chỉ có thể từ chối phiếu chờ duyệt!");
        }

        if (IsIssueTicket(ticket.Type))
        {
            await _balanceManager.UnlockStockAsync(ticket);
        }

        ticket.Reject();
        ticket.UpdateNote($"[Tá»« chá»‘i: {rejectReason}] " + ticket.Note);
    }

    public async Task ExecuteTicketAsync(InventoryTicket ticket)
    {
        if (ticket.Status != ApprovalStatus.Pending)
        {
            throw new BusinessException("SupplyCoreERP:InvalidTicket", "Chỉ có thể thực thi phiếu chờ duyệt!");
        }

        IQueryable<InventoryTicketLine> lines = await _ticketLineRepo.WithDetailsAsync(x => x.Details);
        List<InventoryTicketLine> ticketLines = lines.Where(x => x.TicketId == ticket.Id).ToList();
        List<InventoryTicketDetail> allDetails = ticketLines.SelectMany(x => x.Details).ToList();

        InventoryTransactionType transType = MapTicketToTransaction(ticket.Type);
        await _balanceManager.ExecuteStockMovementAsync(ticket, allDetails, transType, IsIssueTicket(ticket.Type));

        ticket.Execute(ticketLines);
    }
    #endregion

    #region FEFO
    public async Task AllocateFEFOForLineAsync(InventoryTicket ticket, InventoryTicketLine line)
    {
        IQueryable<Product> productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
        Product product = await AsyncExecuter.FirstOrDefaultAsync(productQuery, p => p.Id == line.ProductId);
        if (product == null)
        {
            throw new EntityNotFoundException(typeof(Product), line.ProductId);
        }

        decimal requiredBaseQuantity = _unitConversionManager.ConvertToBaseQuantity(product, line.UnitId, line.Quantity);

        IQueryable<InventoryBalance> balancesQuery = await _balanceRepo.WithDetailsAsync(x => x.BinBalances);
        balancesQuery = balancesQuery
            .Where(x => x.WarehouseId == ticket.WarehouseId && x.ProductId == line.ProductId && x.Quantity > x.LockedQuantity);

        List<InventoryBalance> balances = await AsyncExecuter.ToListAsync(balancesQuery);

        List<Guid> batchIds = balances.Select(x => x.ProductBatchId).Distinct().ToList();
        List<ProductBatch> batches = await _batchRepo.GetListAsync(x => batchIds.Contains(x.Id) && x.Status == BatchQAStatus.Approved && x.ExpiryDate > DateTime.Now);

        List<InventoryBalance> validBalances = (from b in balances
                                                join ba in batches on b.ProductBatchId equals ba.Id
                                                orderby ba.ExpiryDate ascending, ba.ManufacturingDate ascending
                                                select b).ToList();

        List<InventoryTicketDetail> details = new();
        decimal remaining = requiredBaseQuantity;

        foreach (InventoryBalance? balance in validBalances)
        {
            if (remaining <= 0)
            {
                break;
            }

            List<InventoryBinBalance> validBinBalances = balance.BinBalances
                .Where(bb => bb.AvailableQuantity > 0)
                .ToList();

            foreach (InventoryBinBalance? binBalance in validBinBalances)
            {
                if (remaining <= 0)
                {
                    break;
                }

                decimal available = binBalance.AvailableQuantity;
                decimal toTake = Math.Min(available, remaining);

                InventoryTicketDetail detail = new(GuidGenerator.Create(), line.Id, line.ProductId, balance.ProductBatchId, binBalance.BinId, product.BaseUnitId, 1, toTake);
                details.Add(detail);

                remaining -= toTake;
            }
        }

        if (remaining > 0)
        {
            decimal rawTotal = balances.Sum(x => x.Quantity - x.LockedQuantity);
            if (rawTotal >= requiredBaseQuantity)
            {
                throw new BusinessException("SupplyCoreERP:InsufficientStock", $"Không thể xuất hàng FEFO cho '{product.Name}'. " +
                    $"Tổng kho hiện tại ({rawTotal}) đủ số lượng nhưng các lô hàng chưa được Duyệt QA hoặc đã hết hạn sử dụng.");
            }

            throw new BusinessException("SupplyCoreERP:InsufficientStock", $"Không thể xuất hàng FEFO cho '{product.Name}'. " +
                $"Tổng kho hiện tại ({rawTotal}) không đủ số lượng.");
        }

        await _ticketDetailRepo.InsertManyAsync(details);
    }
    #endregion
}






