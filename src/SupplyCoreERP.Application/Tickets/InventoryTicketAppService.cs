using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.PurchaseOrders.Dtos;
using SupplyCoreERP.PurchaseReturns.Dtos;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Sales.SalesRecalls;
using SupplyCoreERP.SalesOrders.Dtos;
using SupplyCoreERP.SalesRecalls.Dtos;
using SupplyCoreERP.Tickets.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Tickets;

public class InventoryTicketAppService : SupplyCore, IInventoryTicketAppService
{
    // Dependencies
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepo;
    private readonly IRepository<SalesOrder, Guid> _salesOrderRepo;
    private readonly ITicketManager _ticketManager;
    private readonly IRepository<PurchaseReturn, Guid> _purchaseReturnRepo;
    private readonly IRepository<PurchaseReturnLine, Guid> _purchaseReturnLineRepo;
    private readonly IRepository<SalesRecall, Guid> _salesRecallRepo;
    private readonly IRepository<SalesRecallLine, Guid> _salesRecallLineRepo;

    // Constructor injection
    public InventoryTicketAppService(
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        IRepository<PurchaseOrder, Guid> purchaseOrderRepo,
        IRepository<SalesOrder, Guid> salesOrderRepo,
        ITicketManager ticketManager,
        IRepository<PurchaseReturn, Guid> purchaseReturnRepo,
        IRepository<PurchaseReturnLine, Guid> purchaseReturnLineRepo,
        IRepository<SalesRecall, Guid> salesRecallRepo,
        IRepository<SalesRecallLine, Guid> salesRecallLineRepo)
    {
        _ticketRepo = ticketRepo;
        _ticketLineRepo = ticketLineRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _purchaseOrderRepo = purchaseOrderRepo;
        _salesOrderRepo = salesOrderRepo;
        _ticketManager = ticketManager;
        _purchaseReturnRepo = purchaseReturnRepo;
        _purchaseReturnLineRepo = purchaseReturnLineRepo;
        _salesRecallRepo = salesRecallRepo;
        _salesRecallLineRepo = salesRecallLineRepo;
    }

    #region Ticket CRUD
    public async Task<PagedResultDto<InventoryTicketDto>> GetListAsync(GetInventoryTicketListDto input)
    {
        IQueryable<InventoryTicket> query = await _ticketRepo.GetQueryableAsync();
        query = query.Include(x => x.Warehouse);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.TicketNumber.Contains(input.Filter))
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
            .WhereIf(input.ReferenceDocumentId.HasValue, x => x.ReferenceDocumentId == input.ReferenceDocumentId);

        int totalCount = await AsyncExecuter.CountAsync(query);
        List<InventoryTicket> items = await AsyncExecuter.ToListAsync(
            query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime DESC" : input.Sorting)
                 .PageBy(input)
        );

        return new PagedResultDto<InventoryTicketDto>(
            totalCount,
            ObjectMapper.Map<List<InventoryTicket>, List<InventoryTicketDto>>(items)
        );
    }

    public async Task<InventoryTicketDto> GetAsync(Guid id)
    {
        IQueryable<InventoryTicket> query = await _ticketRepo.GetQueryableAsync();

        InventoryTicket? ticket = await query
            .Include(x => x.Warehouse)
            .Include(x => x.Lines).ThenInclude(l => l.Product).ThenInclude(p => p.BaseUnit)
            .Include(x => x.Lines).ThenInclude(l => l.Unit)
            .Include(x => x.Lines).ThenInclude(l => l.Details).ThenInclude(d => d.ProductBatch).ThenInclude(pb => pb.MedicineRegistration)
            .Include(x => x.Lines).ThenInclude(l => l.Details).ThenInclude(d => d.Bin)
            .Include(x => x.Lines).ThenInclude(l => l.Details).ThenInclude(d => d.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket == null)
        {
            throw new EntityNotFoundException(typeof(InventoryTicket), id);
        }

        return ObjectMapper.Map<InventoryTicket, InventoryTicketDto>(ticket);
    }

    public async Task<InventoryTicketDto> CreateAsync(CreateInventoryTicketDto input)
    {
        InventoryTicket ticket = await _ticketManager.CreateTicketAsync(
            input.Type, input.WarehouseId, input.ReferenceDocumentId, input.ReferenceDocumentNumber, input.Note);

        await _ticketRepo.InsertAsync(ticket);
        return ObjectMapper.Map<InventoryTicket, InventoryTicketDto>(ticket);
    }

    public async Task<InventoryTicketDto> UpdateAsync(Guid id, UpdateInventoryTicketDto input)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        _ticketManager.UpdateTicket(ticket, input.Note);
        await _ticketRepo.UpdateAsync(ticket);
        return ObjectMapper.Map<InventoryTicket, InventoryTicketDto>(ticket);
    }

    public async Task DeleteAsync(Guid id)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        await _ticketManager.ValidateBeforeDeleteAsync(ticket);
        await _ticketRepo.DeleteAsync(id);
    }
    #endregion

    #region Line & Detail
    public async Task<List<PurchaseOrderLineDto>> GetLinesFromPurchaseOrderAsync(Guid poId)
    {
        IQueryable<PurchaseOrder> query = await _purchaseOrderRepo.GetQueryableAsync();
        PurchaseOrder po = await query.Include(x => x.Lines).ThenInclude(l => l.Product).Include(x => x.Lines).ThenInclude(l => l.Unit).FirstOrDefaultAsync(x => x.Id == poId)
            ?? throw new EntityNotFoundException(typeof(PurchaseOrder), poId);

        if (po.Status == PurchaseOrderStatus.Completed || po.Status == PurchaseOrderStatus.Canceled)
        {
            return new List<PurchaseOrderLineDto>();
        }

        IQueryable<InventoryTicketLine> ticketLineQuery = await _ticketLineRepo.GetQueryableAsync();
        List<Guid> poLineIds = po.Lines.Select(x => x.Id).ToList();

        var existingAllocations = await ticketLineQuery
            .Where(x => x.ReferenceDocumentLineId.HasValue && poLineIds.Contains(x.ReferenceDocumentLineId.Value) && x.Ticket.Status != ApprovalStatus.Rejected)
            .Select(x => new { x.ReferenceDocumentLineId, x.Quantity, x.ConversionFactor })
            .ToListAsync();

        List<PurchaseOrderLineDto> result = new();
        foreach (PurchaseOrderLine poLine in po.Lines)
        {
            decimal alreadyAllocatedBase = existingAllocations
                .Where(a => a.ReferenceDocumentLineId == poLine.Id)
                .Sum(a => a.Quantity * a.ConversionFactor);
            decimal remainingBase = poLine.BaseQuantity - alreadyAllocatedBase;

            if (remainingBase > 0.0001m)
            {
                PurchaseOrderLineDto dto = ObjectMapper.Map<PurchaseOrderLine, PurchaseOrderLineDto>(poLine);
                dto.Quantity = Math.Round(remainingBase / poLine.ConversionFactor, 4);
                dto.ReceivedQuantity = Math.Round(alreadyAllocatedBase / poLine.ConversionFactor, 4);
                result.Add(dto);
            }
        }
        return result;
    }

    public async Task AddLineFromPurchaseOrderAsync(Guid id, Guid poLineId, decimal quantity)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ thêm dòng khi phiếu ở trạng thái Nháp!");
        }

        IQueryable<PurchaseOrder> poQuery = await _purchaseOrderRepo.GetQueryableAsync();
        PurchaseOrder po = await poQuery.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Lines.Any(l => l.Id == poLineId))
            ?? throw new EntityNotFoundException(typeof(PurchaseOrderLine), poLineId);

        PurchaseOrderLine poLine = po.Lines.First(x => x.Id == poLineId);
        InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(ticket, poLine.ProductId, poLine.Id, quantity);
        await _ticketLineRepo.InsertAsync(line);
    }

    public async Task<List<SalesOrderLineDto>> GetLinesFromSalesOrderAsync(Guid soId)
    {
        IQueryable<SalesOrder> query = await _salesOrderRepo.GetQueryableAsync();
        SalesOrder so = await query.Include(x => x.Lines).ThenInclude(l => l.Product).Include(x => x.Lines).ThenInclude(l => l.Unit).FirstOrDefaultAsync(x => x.Id == soId)
            ?? throw new EntityNotFoundException(typeof(SalesOrder), soId);

        if (so.Status == SalesOrderStatus.Completed || so.Status == SalesOrderStatus.Canceled)
        {
            return new List<SalesOrderLineDto>();
        }

        IQueryable<InventoryTicketLine> ticketLineQuery = await _ticketLineRepo.GetQueryableAsync();
        List<Guid> soLineIds = so.Lines.Select(x => x.Id).ToList();

        var existingAllocations = await ticketLineQuery
            .Where(x => x.ReferenceDocumentLineId.HasValue && soLineIds.Contains(x.ReferenceDocumentLineId.Value) && x.Ticket.Status != ApprovalStatus.Rejected)
            .Select(x => new { x.ReferenceDocumentLineId, x.Quantity, x.ConversionFactor })
            .ToListAsync();

        List<SalesOrderLineDto> result = new();
        foreach (SalesOrderLine soLine in so.Lines)
        {
            decimal alreadyAllocatedBase = existingAllocations
                .Where(a => a.ReferenceDocumentLineId == soLine.Id)
                .Sum(a => a.Quantity * a.ConversionFactor);
            decimal remainingBase = soLine.BaseQuantity - alreadyAllocatedBase;

            if (remainingBase > 0.0001m)
            {
                SalesOrderLineDto dto = ObjectMapper.Map<SalesOrderLine, SalesOrderLineDto>(soLine);
                dto.Quantity = Math.Round(remainingBase / soLine.ConversionFactor, 4);
                dto.DeliveredQuantity = Math.Round(alreadyAllocatedBase / soLine.ConversionFactor, 4);
                result.Add(dto);
            }
        }
        return result;
    }

    public async Task AddLineFromSalesOrderAsync(Guid id, Guid soLineId, decimal quantity)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ thêm dòng khi phiếu ở trạng thái Nháp!");
        }

        IQueryable<SalesOrder> soQuery = await _salesOrderRepo.GetQueryableAsync();
        SalesOrder so = await soQuery.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Lines.Any(l => l.Id == soLineId))
            ?? throw new EntityNotFoundException(typeof(SalesOrderLine), soLineId);

        SalesOrderLine soLine = so.Lines.First(x => x.Id == soLineId);
        InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(ticket, soLine.ProductId, soLine.Id, quantity);
        await _ticketLineRepo.InsertAsync(line);
    }

    public async Task<List<PurchaseReturnLineDto>> GetLinesFromPurchaseReturnAsync(Guid returnId)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn pr = await query.Include(x => x.Lines).ThenInclude(l => l.Product).Include(x => x.Lines).ThenInclude(l => l.Unit).FirstOrDefaultAsync(x => x.Id == returnId)
            ?? throw new EntityNotFoundException(typeof(PurchaseReturn), returnId);

        if (pr.Status == PurchaseReturnStatus.Completed)
        {
            return new List<PurchaseReturnLineDto>();
        }

        IQueryable<InventoryTicketLine> ticketLineQuery = await _ticketLineRepo.GetQueryableAsync();
        List<Guid> prLineIds = pr.Lines.Select(x => x.Id).ToList();

        var existingAllocations = await ticketLineQuery
            .Where(x => x.ReferenceDocumentLineId.HasValue && prLineIds.Contains(x.ReferenceDocumentLineId.Value) && x.Ticket.Status != ApprovalStatus.Rejected)
            .Select(x => new { x.ReferenceDocumentLineId, x.Quantity, x.ConversionFactor })
            .ToListAsync();

        List<PurchaseReturnLineDto> result = new();
        foreach (PurchaseReturnLine prLine in pr.Lines)
        {
            decimal alreadyAllocatedBase = existingAllocations
                .Where(a => a.ReferenceDocumentLineId == prLine.Id)
                .Sum(a => a.Quantity * a.ConversionFactor);
            decimal remainingBase = prLine.BaseQuantity - alreadyAllocatedBase;

            if (remainingBase > 0.0001m)
            {
                PurchaseReturnLineDto dto = ObjectMapper.Map<PurchaseReturnLine, PurchaseReturnLineDto>(prLine);
                dto.Quantity = Math.Round(remainingBase / prLine.ConversionFactor, 4);
                result.Add(dto);
            }
        }
        return result;
    }

    public async Task AddLineFromPurchaseReturnAsync(Guid id, Guid prLineId, decimal quantity)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ thêm dòng khi phiếu ở trạng thái Nháp!");
        }

        IQueryable<PurchaseReturn> prQuery = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn pr = await prQuery.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Lines.Any(l => l.Id == prLineId))
            ?? throw new EntityNotFoundException(typeof(PurchaseReturnLine), prLineId);

        PurchaseReturnLine prLine = pr.Lines.First(x => x.Id == prLineId);
        InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(ticket, prLine.ProductId, prLine.Id, quantity);
        await _ticketLineRepo.InsertAsync(line);
    }

    public async Task<List<SalesRecallLineDto>> GetLinesFromSalesRecallAsync(Guid recallId)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall recall = await query.Include(x => x.Lines).ThenInclude(l => l.Customer).Include(x => x.Lines).ThenInclude(l => l.Unit).FirstOrDefaultAsync(x => x.Id == recallId)
            ?? throw new EntityNotFoundException(typeof(SalesRecall), recallId);

        if (recall.Status == SalesRecallStatus.Completed)
        {
            return new List<SalesRecallLineDto>();
        }

        IQueryable<InventoryTicketLine> ticketLineQuery = await _ticketLineRepo.GetQueryableAsync();
        List<Guid> recallLineIds = recall.Lines.Select(x => x.Id).ToList();

        var existingAllocations = await ticketLineQuery
            .Where(x => x.ReferenceDocumentLineId.HasValue && recallLineIds.Contains(x.ReferenceDocumentLineId.Value) && x.Ticket.Status != ApprovalStatus.Rejected)
            .Select(x => new { x.ReferenceDocumentLineId, x.Quantity, x.ConversionFactor })
            .ToListAsync();

        List<SalesRecallLineDto> result = new();
        foreach (SalesRecallLine recallLine in recall.Lines)
        {
            decimal alreadyAllocatedBase = existingAllocations
                .Where(a => a.ReferenceDocumentLineId == recallLine.Id)
                .Sum(a => a.Quantity * a.ConversionFactor);
            decimal remainingBase = recallLine.BaseQuantity - alreadyAllocatedBase;

            if (remainingBase > 0.0001m)
            {
                SalesRecallLineDto dto = ObjectMapper.Map<SalesRecallLine, SalesRecallLineDto>(recallLine);
                dto.Quantity = Math.Round(remainingBase / recallLine.ConversionFactor, 4);
                result.Add(dto);
            }
        }
        return result;
    }

    public async Task AddLineFromSalesRecallAsync(Guid id, Guid recallLineId, decimal quantity)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ thêm dòng khi phiếu ở trạng thái Nháp!");
        }

        IQueryable<SalesRecall> recallQuery = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall recall = await recallQuery.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Lines.Any(l => l.Id == recallLineId))
            ?? throw new EntityNotFoundException(typeof(SalesRecallLine), recallLineId);

        SalesRecallLine recallLine = recall.Lines.First(x => x.Id == recallLineId);
        InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(ticket, recall.ProductId, recallLine.Id, quantity);
        await _ticketLineRepo.InsertAsync(line);
    }

    public async Task DeleteLineAsync(Guid id)
    {
        await _ticketLineRepo.DeleteAsync(id);
    }

    public async Task AddDetailAsync(Guid lineId, AddTicketDetailDto input)
    {
        InventoryTicketLine line = await _ticketLineRepo.GetAsync(lineId);
        InventoryTicket ticket = await _ticketRepo.GetAsync(line.TicketId);

        InventoryTicketDetail detail = await _ticketManager.CreateTicketDetailAsync(
            ticket,
            line,
            input.ProductId,
            input.ProductBatchId,
            input.BinId,
            input.UnitId,
            input.ConversionFactor,
            input.Quantity);

        await _ticketDetailRepo.InsertAsync(detail);
    }

    public async Task DeleteDetailAsync(Guid id)
    {
        InventoryTicketDetail detail = await _ticketDetailRepo.GetAsync(id);
        InventoryTicketLine line = await _ticketLineRepo.GetAsync(detail.TicketLineId);
        InventoryTicket ticket = await _ticketRepo.GetAsync(line.TicketId);

        await _ticketManager.RemoveTicketDetailAsync(ticket, line, detail);
        await _ticketDetailRepo.DeleteAsync(id);
    }
    #endregion

    #region Workflow
    public async Task SendToApproveAsync(Guid id)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        await _ticketManager.SendToApproveAsync(ticket);
        await _ticketRepo.UpdateAsync(ticket);
    }

    public async Task ExecuteAsync(Guid id)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        await _ticketManager.ExecuteTicketAsync(ticket);
        await _ticketRepo.UpdateAsync(ticket);
    }

    public async Task ApplyFEFOAsync(Guid id)
    {
        IQueryable<InventoryTicket> query = await _ticketRepo.GetQueryableAsync();
        InventoryTicket? ticket = await query
            .Include(x => x.Lines).ThenInclude(l => l.Details)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (ticket == null)
        {
            throw new EntityNotFoundException(typeof(InventoryTicket), id);
        }

        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ có thể áp dụng FEFO cho phiếu ở trạng thái Nháp!");
        }

        // FEFO chỉ áp dụng cho các loại phiếu xuất (GoodsIssue, Disposal, ReturnOutward)
        if (ticket.Type != TicketType.GoodsIssue && ticket.Type != TicketType.DisposalIssue && ticket.Type != TicketType.ReturnOutward)
        {
            throw new UserFriendlyException("FEFO chỉ áp dụng cho các loại phiếu xuất kho!");
        }

        foreach (InventoryTicketLine line in ticket.Lines)
        {
            // 1. Xóa các chi tiết cũ (nếu có)
            if (line.Details != null && line.Details.Any())
            {
                List<InventoryTicketDetail> detailsToRemove = line.Details.ToList();
                foreach (InventoryTicketDetail? det in detailsToRemove)
                {
                    await _ticketManager.RemoveTicketDetailAsync(ticket, line, det);
                    await _ticketDetailRepo.DeleteAsync(det.Id);
                }
                line.Details.Clear();
            }

            // 2. Cập phát FEFO mới
            await _ticketManager.AllocateFEFOForLineAsync(ticket, line);
        }
    }
    #endregion

    #region Traceability
    public async Task<List<InventoryTicketDto>> GetRelatedTicketsByPurchaseOrderAsync(Guid poId)
    {
        IQueryable<InventoryTicket> query = await _ticketRepo.GetQueryableAsync();

        List<InventoryTicketDto> tickets = await AsyncExecuter.ToListAsync(
           query.Where(x => x.ReferenceDocumentId == poId)
        .Select(x => new InventoryTicketDto
        {
            Id = x.Id,
            TicketNumber = x.TicketNumber,
            Type = x.Type,
            Status = x.Status,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            ReferenceDocumentId = x.ReferenceDocumentId,
            ReferenceDocumentNumber = x.ReferenceDocumentNumber,
            Note = x.Note,
            CreationTime = x.CreationTime
        }));
        return tickets;
    }

    public async Task<List<InventoryTicketDto>> GetRelatedTicketsBySaleOrderAsync(Guid soId)
    {
        IQueryable<InventoryTicket> query = await _ticketRepo.GetQueryableAsync();

        List<InventoryTicketDto> tickets = await AsyncExecuter.ToListAsync(
           query.Where(x => x.ReferenceDocumentId == soId)
        .Select(x => new InventoryTicketDto
        {
            Id = x.Id,
            TicketNumber = x.TicketNumber,
            Type = x.Type,
            Status = x.Status,
            WarehouseId = x.WarehouseId,
            WarehouseName = x.Warehouse.Name,
            ReferenceDocumentId = x.ReferenceDocumentId,
            ReferenceDocumentNumber = x.ReferenceDocumentNumber,
            Note = x.Note,
            CreationTime = x.CreationTime
        }));
        return tickets;
    }
    #endregion
}

