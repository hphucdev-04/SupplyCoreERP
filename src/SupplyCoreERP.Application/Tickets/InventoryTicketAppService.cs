using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.BaseUnits;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.PurchaseOrders.Dtos;
using SupplyCoreERP.Sales.Orders;
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
    private readonly TicketManager _ticketManager;

    // DI
    public InventoryTicketAppService(
    IRepository<InventoryTicket, Guid> ticketRepo,
    IRepository<InventoryTicketLine, Guid> ticketLineRepo,
    IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
    IRepository<PurchaseOrder, Guid> purchaseOrderRepo,
    IRepository<SalesOrder, Guid> salesOrderRepo,
    TicketManager ticketManager)
    {
        _ticketRepo = ticketRepo;
        _ticketLineRepo = ticketLineRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _purchaseOrderRepo = purchaseOrderRepo;
        _salesOrderRepo = salesOrderRepo;
        _ticketManager = ticketManager;
    }

    #region Ticket
    public async Task<PagedResultDto<InventoryTicketDto>> GetListAsync(GetInventoryTicketListDto input)
    {
        IQueryable<InventoryTicket> query = await _ticketRepo.GetQueryableAsync();

        query = query.Include(x => x.Warehouse);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.TicketNumber.Contains(input.Filter))
            .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId);

        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(InventoryTicket.CreationTime) + " DESC")
            .PageBy(input);

        List<InventoryTicket> items = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<InventoryTicketDto>(totalCount, ObjectMapper.Map<List<InventoryTicket>, List<InventoryTicketDto>>(items));
    }

    public async Task<InventoryTicketDto> GetAsync(Guid id)
    {
        IQueryable<InventoryTicket> query = await _ticketRepo.GetQueryableAsync();

        InventoryTicket? ticket = await query
            .Include(x => x.Warehouse)
            .Include(x => x.Lines).ThenInclude(l => l.Product)
            .Include(x => x.Lines).ThenInclude(l => l.Unit)
            .Include(x => x.Lines).ThenInclude(l => l.PurchaseOrderLine)
            .Include(x => x.Lines).ThenInclude(l => l.Details).ThenInclude(d => d.ProductBatch)
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
        InventoryTicket ticket = await _ticketManager.CreateTicketAsync(input.Type, input.WarehouseId, input.ReferenceDocumentId, input.ReferenceDocumentNumber, input.Note);

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
        await _ticketRepo.DeleteAsync(ticket);
    }

    public async Task<List<InventoryTicketDto>> GetRelatedTicketsByPurchaseOrderAsync(Guid poId)
    {
        List<InventoryTicket> tickets = await _ticketRepo.GetListAsync(x => x.ReferenceDocumentId == poId);
        return ObjectMapper.Map<List<InventoryTicket>, List<InventoryTicketDto>>(tickets);
    }
    #endregion

    #region Line & Detail
    public async Task<List<PurchaseOrderLineDto>> GetLinesFromPurchaseOrderAsync(Guid poId)
    {
        IQueryable<PurchaseOrder> query = await _purchaseOrderRepo.GetQueryableAsync();
        PurchaseOrder po = await query.Include(x => x.Lines).ThenInclude(l => l.Product).Include(x => x.Lines).ThenInclude(l => l.Unit).FirstOrDefaultAsync(x => x.Id == poId)
            ?? throw new EntityNotFoundException(typeof(PurchaseOrder), poId);

        // Nếu đơn hàng đã hoàn tất hoặc bị hủy, không cho phép lấy thêm dòng hàng để nhập kho
        if (po.Status == PurchaseOrderStatus.Completed || po.Status == PurchaseOrderStatus.Canceled)
        {
            return new List<PurchaseOrderLineDto>();
        }

        // Lấy tất cả các dòng phiếu kho đã tạo (Draft, Pending, Approved) trừ Rejected
        IQueryable<InventoryTicketLine> ticketLineQuery = await _ticketLineRepo.GetQueryableAsync();
        var poLineIds = po.Lines.Select(x => x.Id).ToList();

        var existingAllocations = await ticketLineQuery
            .Where(x => x.PurchaseOrderLineId.HasValue && poLineIds.Contains(x.PurchaseOrderLineId.Value) && x.Ticket.Status != ApprovalStatus.Rejected)
            .Select(x => new { x.PurchaseOrderLineId, x.Quantity })
            .ToListAsync();

        var result = new List<PurchaseOrderLineDto>();
        foreach (PurchaseOrderLine poLine in po.Lines)
        {
            // Quantity trong TicketLine luôn là đơn vị cơ bản (BaseQuantity)
            decimal alreadyAllocatedBase = existingAllocations.Where(a => a.PurchaseOrderLineId == poLine.Id).Sum(a => a.Quantity);
            decimal remainingBase = poLine.BaseQuantity - alreadyAllocatedBase;

            if (remainingBase > 0.0001m) // Sử dụng epsilon để tránh sai số nhỏ
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
            throw new UserFriendlyException("Chỉ có thể thêm dòng hàng khi phiếu ở trạng thái Nháp!");
        }

        // 1. Chặn thêm trùng Line trong cùng 1 phiếu
        if (await _ticketLineRepo.AnyAsync(x => x.TicketId == id && x.PurchaseOrderLineId == poLineId))
        {
            throw new UserFriendlyException("Sản phẩm này đã có trong phiếu kho hiện tại!");
        }

        // 2. Lấy thông tin PO Line và kiểm tra trạng thái PO
        IQueryable<PurchaseOrder> poQuery = await _purchaseOrderRepo.GetQueryableAsync();
        PurchaseOrder po = await poQuery.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Lines.Any(l => l.Id == poLineId))
            ?? throw new EntityNotFoundException(typeof(PurchaseOrderLine), poLineId);

        if (po.Status == PurchaseOrderStatus.Completed || po.Status == PurchaseOrderStatus.Canceled)
        {
            throw new UserFriendlyException($"Đơn hàng {po.Code} đã ở trạng thái {po.Status}, không thể nhập thêm hàng!");
        }

        PurchaseOrderLine poLine = po.Lines.First(x => x.Id == poLineId);

        // 3. Re-validate số lượng còn lại (Sử dụng BaseQuantity)
        IQueryable<InventoryTicketLine> ticketLineQuery = await _ticketLineRepo.GetQueryableAsync();
        decimal existingSumBase = await ticketLineQuery
            .Where(x => x.PurchaseOrderLineId == poLineId && x.Ticket.Status != ApprovalStatus.Rejected)
            .SumAsync(x => x.Quantity);

        decimal inputBaseQty = quantity * poLine.ConversionFactor;
        decimal maxAllowedBase = poLine.BaseQuantity - existingSumBase;

        if (inputBaseQty > maxAllowedBase + 0.0001m)
        {
            decimal maxAllowedPO = Math.Round(maxAllowedBase / poLine.ConversionFactor, 4);
            throw new UserFriendlyException($"Số lượng nhập ({quantity}) vượt quá số lượng còn lại của đơn hàng ({maxAllowedPO})!");
        }

        // TicketLine.Quantity luôn lưu theo đơn vị cơ bản
        InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(ticket, poLine.ProductId, poLine.Id, inputBaseQty);
        await _ticketLineRepo.InsertAsync(line);
    }

    public async Task DeleteLineAsync(Guid id)
    {
        InventoryTicketLine line = await _ticketLineRepo.GetAsync(id);
        InventoryTicket ticket = await _ticketRepo.GetAsync(line.TicketId);

        if (ticket.Status != ApprovalStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ có thể xóa dòng hàng khi phiếu ở trạng thái Nháp!");
        }

        // Xóa tất cả details của line trước
        List<InventoryTicketDetail> details = await _ticketDetailRepo.GetListAsync(x => x.TicketLineId == id);
        await _ticketDetailRepo.DeleteManyAsync(details);
        await _ticketLineRepo.DeleteAsync(id);
    }

    public async Task AddDetailAsync(Guid id, AddTicketDetailDto input)
    {
        InventoryTicketLine line = await _ticketLineRepo.GetAsync(id);
        InventoryTicket ticket = await _ticketRepo.GetAsync(line.TicketId);

        InventoryTicketDetail detail = await _ticketManager.CreateTicketDetailAsync(
            ticket, line, input.ProductId, input.ProductBatchId, input.BinId, input.UnitId, input.ConversionFactor, input.Quantity);

        await _ticketDetailRepo.InsertAsync(detail);
        await _ticketLineRepo.UpdateAsync(line);
    }

    public async Task DeleteDetailAsync(Guid id)
    {
        InventoryTicketDetail detail = await _ticketDetailRepo.GetAsync(id);
        InventoryTicketLine line = await _ticketLineRepo.GetAsync(detail.TicketLineId);
        InventoryTicket ticket = await _ticketRepo.GetAsync(line.TicketId);

        await _ticketManager.RemoveTicketDetailAsync(ticket, line, detail);
        await _ticketDetailRepo.DeleteAsync(id);
        await _ticketLineRepo.UpdateAsync(line);
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
    #endregion
}
