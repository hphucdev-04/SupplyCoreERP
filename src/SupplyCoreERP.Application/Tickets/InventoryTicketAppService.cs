using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Tickets.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Tickets;

public class InventoryTicketAppService : SupplyCore, IInventoryTicketAppService
{
    // Dependencies
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepo;
    private readonly IRepository<SalesOrder, Guid> _salesOrderRepo;
    private readonly TicketManager _ticketManager;

    // DI
    public InventoryTicketAppService(
    IRepository<InventoryTicket, Guid> ticketRepo,
    IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
    IRepository<PurchaseOrder, Guid> purchaseOrderRepo,
    IRepository<SalesOrder, Guid> salesOrderRepo,
    TicketManager ticketManager)
    {
        _ticketRepo = ticketRepo;
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
            .Include(x => x.Details).ThenInclude(d => d.Product).ThenInclude(p => p.BaseUnit)
            .Include(x => x.Details).ThenInclude(d => d.ProductBatch)
            .Include(x => x.Details).ThenInclude(d => d.Bin)
            .Include(x => x.Details).ThenInclude(d => d.Unit)
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

        await _ticketDetailRepo.DeleteAsync(x => x.TicketId == id);
        await _ticketRepo.DeleteAsync(ticket);
    }
    #endregion

    #region Ticket Detail
    public async Task CreateTicketDetailAsync(Guid ticketId, AddTicketDetailDto input)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(ticketId);
        InventoryTicketDetail detail = await _ticketManager.CreateTicketDetailAsync(ticket, input.ProductId, input.ProductBatchId, input.BinId, input.UnitId, input.ConversionFactor, input.Quantity);

        await _ticketDetailRepo.InsertAsync(detail);
    }

    public async Task UpdateDetailQuantityAsync(Guid detailId, decimal actualQuantity)
    {
        InventoryTicketDetail detail = await _ticketDetailRepo.GetAsync(detailId);
        InventoryTicket ticket = await _ticketRepo.GetAsync(detail.TicketId);

        await _ticketManager.UpdateDetailQuantityAsync(ticket, detail, actualQuantity);
        await _ticketDetailRepo.UpdateAsync(detail);
    }

    public async Task RemoveDetailAsync(Guid ticketId, Guid detailId)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(ticketId);
        InventoryTicketDetail detail = await _ticketDetailRepo.GetAsync(detailId);

        await _ticketManager.RemoveTicketDetailAsync(ticket, detail);
        await _ticketDetailRepo.DeleteAsync(detail);
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
        IQueryable<InventoryTicket> ticketQuery = await _ticketRepo.GetQueryableAsync();
        InventoryTicket ticket = await ticketQuery
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(InventoryTicket), id);

        var details = ticket.Details.ToList();

        // Managerthực thi xuất/nhập kho (không Sync PO/SO)
        await _ticketManager.ExecuteTicketAsync(ticket, details);
        await _ticketRepo.UpdateAsync(ticket);

        //Cross-aggregate sync 
        if (ticket.ReferenceDocumentId.HasValue)
        {
            if (ticket.Type == TicketType.GoodsReceipt)
            {
                await SyncPurchaseOrderAsync(ticket, details);
            }
            else if (ticket.Type == TicketType.GoodsIssue)
            {
                await SyncSalesOrderAsync(ticket, details);
            }
        }
    }

    public async Task RejectAsync(Guid id, string reason)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);
        await _ticketManager.RejectTicketAsync(ticket, reason);
        await _ticketRepo.UpdateAsync(ticket);
    }
    #endregion

    #region FEFO 
    public async Task AllocateFEFOAsync(Guid id, Guid productId, decimal requiredBaseQuantity)
    {
        InventoryTicket ticket = await _ticketRepo.GetAsync(id);

        // Manager trả về list
        IList<InventoryTicketDetail> details = await _ticketManager.AllocateFEFOAsync(ticket, productId, requiredBaseQuantity);

        if (details.Any())
        {
            await _ticketDetailRepo.InsertManyAsync(details);
        }
    }

    #endregion

    #region Sync 
    private async Task SyncPurchaseOrderAsync(InventoryTicket ticket, IList<InventoryTicketDetail> details)
    {
        IQueryable<PurchaseOrder> poQuery = await _purchaseOrderRepo.GetQueryableAsync();
        PurchaseOrder? po = await poQuery
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == ticket.ReferenceDocumentId!.Value);

        if (po == null)
        {
            return;
        }

        // Cập nhật ReceivedQuantity theo ProductId
        foreach (InventoryTicketDetail d in details)
        {
            PurchaseOrderDetail? poDetail = po.Details.FirstOrDefault(x => x.ProductId == d.ProductId);
            poDetail?.AddReceivedQuantity(d.BaseQuantity);
        }

        if (po.Status == PurchaseOrderStatus.Approved)
        {
            po.StartReceiving();
        }

        await _purchaseOrderRepo.UpdateAsync(po);
    }

    private async Task SyncSalesOrderAsync(InventoryTicket ticket, IList<InventoryTicketDetail> details)
    {
        IQueryable<SalesOrder> soQuery = await _salesOrderRepo.GetQueryableAsync();
        SalesOrder? so = await soQuery
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.Id == ticket.ReferenceDocumentId!.Value);

        if (so == null)
        {
            return;
        }

        foreach (InventoryTicketDetail d in details)
        {
            SalesOrderDetail? soDetail = so.Details.FirstOrDefault(x => x.ProductId == d.ProductId);
            soDetail?.AddDeliveredQuantity(d.BaseQuantity);
        }

        if (so.Status == SalesOrderStatus.Approved)
        {
            so.StartDelivering();
        }

        await _salesOrderRepo.UpdateAsync(so);
    }
    #endregion
}
