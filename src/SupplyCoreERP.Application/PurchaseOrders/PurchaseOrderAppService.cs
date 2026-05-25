using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.PurchaseOrders.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.PurchaseOrders;

public class PurchaseOrderAppService : SupplyCore, IPurchaseOrderAppService
{
    // Dependencies
    private readonly IRepository<PurchaseOrder, Guid> _orderRepo;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<Supplier, Guid> _supplierRepo;
    private readonly PurchaseOrderManager _orderManager;

    // Constructor injection
    public PurchaseOrderAppService(
    IRepository<PurchaseOrder, Guid> orderRepo,
    IRepository<InventoryTicket, Guid> ticketRepo,
    IRepository<Supplier, Guid> supplierRepo,
    PurchaseOrderManager orderManager)
    {
        _orderRepo = orderRepo;
        _ticketRepo = ticketRepo;
        _supplierRepo = supplierRepo;
        _orderManager = orderManager;
    }

    #region Purchase Order
    public async Task<PagedResultDto<PurchaseOrderDto>> GetListAsync(GetPurchaseOrderListDto input)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Supplier)
            .Include(x => x.Warehouse)
            .Include(x => x.PurchaseRequisition);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter) || x.Supplier.Name.Contains(input.Filter))
            .WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(PurchaseOrder.CreationTime) + " DESC")
            .PageBy(input);

        List<PurchaseOrder> items = await AsyncExecuter.ToListAsync(query);

        List<PurchaseOrderDto> dtos = ObjectMapper.Map<List<PurchaseOrder>, List<PurchaseOrderDto>>(items);
        return new PagedResultDto<PurchaseOrderDto>(totalCount, dtos);
    }

    public async Task<PurchaseOrderDto> GetAsync(Guid id)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();

        PurchaseOrder? entity = await query
            .Include(x => x.Supplier)
            .Include(x => x.Warehouse)
            .Include(x => x.PurchaseRequisition)
            .Include(x => x.Lines).ThenInclude(d => d.Product)
            .Include(x => x.Lines).ThenInclude(d => d.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseOrder), id);
        }

        PurchaseOrderDto dto = ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(entity);

        // Traceability: PO -> Tickets
        List<InventoryTicket> tickets = await _ticketRepo.GetListAsync(x => x.ReferenceDocumentId == id);
        dto.RelatedTickets = tickets.Select(t => new RelatedTicketDto
        {
            Id = t.Id,
            TicketNumber = t.TicketNumber,
            Type = t.Type,
            Status = t.Status,
            CreationTime = t.CreationTime
        }).ToList();

        return dto;
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input)
    {
        PurchaseOrder entity = await _orderManager.CreateOrderAsync(input.SupplierId, input.WarehouseId, input.OrderDate, input.ExpectedDeliveryDate, input.DueDate, input.Note);

        await _orderRepo.InsertAsync(entity);

        return ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(entity);
    }

    public async Task<PurchaseOrderDto> UpdateAsync(Guid id, UpdatePurchaseOrderDto input)
    {
        PurchaseOrder entity = await _orderRepo.GetAsync(id);

        await _orderManager.UpdateOrderAsync(entity, input.WarehouseId, input.ExpectedDeliveryDate, input.DueDate, input.Note);
        await _orderRepo.UpdateAsync(entity);

        return ObjectMapper.Map<PurchaseOrder, PurchaseOrderDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();
        PurchaseOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

        if (entity != null)
        {
            await _orderManager.CheckBeforeDeleteAsync(entity);
            await _orderRepo.DeleteAsync(entity);
        }
    }
    #endregion

    #region Purchase Line
    public async Task AddLineAsync(Guid orderId, AddPurchaseOrderLineDto input)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();
        PurchaseOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == orderId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseOrder), orderId);
        }

        await _orderManager.AddLineAsync(entity, input.ProductId, input.UnitId, input.ConversionFactor, input.Quantity, input.UnitPrice, input.TaxRate);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task UpdateLineAsync(Guid orderId, Guid lineId, UpdatePurchaseOrderLineDto input)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();
        PurchaseOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == orderId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseOrder), orderId);
        }

        await _orderManager.UpdateLineAsync(entity, lineId, input.Quantity, input.UnitPrice, input.TaxRate);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task RemoveLineAsync(Guid orderId, Guid lineId)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();
        PurchaseOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == orderId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseOrder), orderId);
        }

        await _orderManager.RemoveLineAsync(entity, lineId);
        await _orderRepo.UpdateAsync(entity);
    }
    #endregion

    #region Workflow
    public async Task SendToApproveAsync(Guid id)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();
        PurchaseOrder entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(PurchaseOrder), id);

        await _orderManager.SendToApproveAsync(entity);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task ApproveAsync(Guid id)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();
        PurchaseOrder entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(PurchaseOrder), id);

        InventoryTicket ticket = await _orderManager.ApproveAsync(entity);

        await _ticketRepo.InsertAsync(ticket);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task CompleteAsync(Guid id)
    {
        IQueryable<PurchaseOrder> query = await _orderRepo.GetQueryableAsync();
        PurchaseOrder entity = await query
            .Include(x => x.Lines).ThenInclude(l => l.Product)
            .Include(x => x.Lines).ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(PurchaseOrder), id);

        Supplier supplier = await _orderManager.CompleteAsync(entity);

        await _supplierRepo.UpdateAsync(supplier);
        await _orderRepo.UpdateAsync(entity);
    }
    #endregion
}

