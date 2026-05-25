using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Partner.Customers;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.SalesOrders.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.SalesOrders;

public class SalesOrderAppService : SupplyCore, ISalesOrderAppService
{
    // Dependencies
    private readonly IRepository<SalesOrder, Guid> _orderRepo;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly SalesOrderManager _orderManager;

    // Constructor injection
    public SalesOrderAppService(
        IRepository<SalesOrder, Guid> orderRepo,
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        IRepository<Customer, Guid> customerRepo,
        SalesOrderManager orderManager)
    {
        _orderRepo = orderRepo;
        _ticketRepo = ticketRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _customerRepo = customerRepo;
        _orderManager = orderManager;
    }

    #region SaleOrder
    public async Task<PagedResultDto<SalesOrderDto>> GetListAsync(GetSalesOrderListDto input)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Customer)
            .Include(x => x.Warehouse);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter) || x.Customer.Name.Contains(input.Filter))
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId)
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(SalesOrder.CreationTime) + " DESC")
            .PageBy(input);

        List<SalesOrder> items = await AsyncExecuter.ToListAsync(query);

        List<SalesOrderDto> dtos = ObjectMapper.Map<List<SalesOrder>, List<SalesOrderDto>>(items);
        return new PagedResultDto<SalesOrderDto>(totalCount, dtos);
    }

    public async Task<SalesOrderDto> GetAsync(Guid id)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();

        SalesOrder? entity = await query
            .Include(x => x.Customer)
            .Include(x => x.Warehouse)
            .Include(x => x.Lines).ThenInclude(d => d.Product)
            .Include(x => x.Lines).ThenInclude(d => d.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesOrder), id);
        }

        return ObjectMapper.Map<SalesOrder, SalesOrderDto>(entity);
    }

    public async Task<SalesOrderDto> CreateAsync(CreateSalesOrderDto input)
    {
        SalesOrder entity = await _orderManager.CreateOrderAsync(input.CustomerId, input.WarehouseId, input.OrderDate, input.ExpectedDeliveryDate, input.DueDate, input.Note);

        await _orderRepo.InsertAsync(entity);

        return ObjectMapper.Map<SalesOrder, SalesOrderDto>(entity);
    }

    public async Task<SalesOrderDto> UpdateAsync(Guid id, UpdateSalesOrderDto input)
    {
        SalesOrder entity = await _orderRepo.GetAsync(id);

        await _orderManager.UpdateOrderAsync(entity, input.WarehouseId, input.ExpectedDeliveryDate, input.DueDate, input.Note);
        await _orderRepo.UpdateAsync(entity);

        return ObjectMapper.Map<SalesOrder, SalesOrderDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();
        SalesOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

        if (entity != null)
        {
            await _orderManager.CheckBeforeDeleteAsync(entity);
            await _orderRepo.DeleteAsync(entity);
        }
    }
    #endregion

    #region SaleOrder Lines
    public async Task AddLineAsync(Guid orderId, AddSalesOrderLineDto input)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();
        SalesOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == orderId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesOrder), orderId);
        }

        await _orderManager.AddLineAsync(entity, input.ProductId, input.UnitId, input.ConversionFactor, input.Quantity, input.UnitPrice, input.DiscountRate, input.TaxRate);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task UpdateLineAsync(Guid orderId, Guid lineId, UpdateSalesOrderLineDto input)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();
        SalesOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == orderId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesOrder), orderId);
        }

        await _orderManager.UpdateLineAsync(entity, lineId, input.Quantity, input.UnitPrice, input.DiscountRate, input.TaxRate);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task RemoveLineAsync(Guid orderId, Guid lineId)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();
        SalesOrder? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == orderId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesOrder), orderId);
        }

        await _orderManager.RemoveLineAsync(entity, lineId);
        await _orderRepo.UpdateAsync(entity);
    }
    #endregion

    #region Workflow
    public async Task SendToApproveAsync(Guid id)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();
        SalesOrder entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(SalesOrder), id);

        await _orderManager.SendToApproveAsync(entity);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task ApproveAsync(Guid id)
    {
        IQueryable<SalesOrder> query = await _orderRepo.GetQueryableAsync();
        SalesOrder entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(SalesOrder), id);

        // Manager validate tá»“n kho tá»•ng quÃ¡t + táº¡o ticket Header (Draft)
        InventoryTicket ticket = await _orderManager.ApproveAsync(entity);

        await _ticketRepo.InsertAsync(ticket);
        await _orderRepo.UpdateAsync(entity);
    }

    public async Task CompleteAsync(Guid id)
    {
        SalesOrder entity = await _orderRepo.GetAsync(id);

        Customer customer = await _orderManager.CompleteAsync(entity);

        await _customerRepo.UpdateAsync(customer);
        await _orderRepo.UpdateAsync(entity);
    }
    #endregion
}

