using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using SupplyCoreERP.PurchaseRequisitions.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.PurchaseRequisitions;

public class PurchaseRequisitionAppService : SupplyCore, IPurchaseRequisitionAppService
{
    private readonly IRepository<PurchaseRequisition, Guid> _requisitionRepo;
    private readonly IRepository<PurchaseOrder, Guid> _orderRepo;
    private readonly IPurchaseRequisitionManager _requisitionManager;
    private readonly IPurchaseOrderManager _orderManager;

    public PurchaseRequisitionAppService(
        IRepository<PurchaseRequisition, Guid> requisitionRepo,
        IRepository<PurchaseOrder, Guid> orderRepo,
        IPurchaseRequisitionManager requisitionManager,
        IPurchaseOrderManager orderManager)
    {
        _requisitionRepo = requisitionRepo;
        _orderRepo = orderRepo;
        _requisitionManager = requisitionManager;
        _orderManager = orderManager;
    }

    public async Task<PagedResultDto<PurchaseRequisitionDto>> GetListAsync(GetPurchaseRequisitionListDto input)
    {
        IQueryable<PurchaseRequisition> query = await _requisitionRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Warehouse)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter))
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(PurchaseRequisition.CreationTime) + " DESC")
            .PageBy(input);

        List<PurchaseRequisition> items = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<PurchaseRequisitionDto>(
            totalCount,
            ObjectMapper.Map<List<PurchaseRequisition>, List<PurchaseRequisitionDto>>(items)
        );
    }

    public async Task<PurchaseRequisitionDto> GetAsync(Guid id)
    {
        IQueryable<PurchaseRequisition> query = await _requisitionRepo.GetQueryableAsync();
        PurchaseRequisition? entity = await query
            .Include(x => x.Warehouse)
            .Include(x => x.Lines).ThenInclude(l => l.Product)
            .Include(x => x.Lines).ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseRequisition), id);
        }

        PurchaseRequisitionDto dto = ObjectMapper.Map<PurchaseRequisition, PurchaseRequisitionDto>(entity);

        IQueryable<PurchaseOrder> relatedOrders = await _orderRepo.GetQueryableAsync();
        List<PurchaseOrder> orders = await relatedOrders
            .Include(o => o.Supplier)
            .Where(o => o.PurchaseRequisitionId == id)
            .ToListAsync();

        dto.RelatedOrders = orders.Select(o => new RelatedPurchaseOrderDto
        {
            Id = o.Id,
            Code = o.Code,
            SupplierName = o.Supplier.Name,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            CreationTime = o.CreationTime
        }).ToList();

        return dto;
    }

    public async Task<PurchaseRequisitionDto> CreateAsync(CreatePurchaseRequisitionDto input)
    {
        PurchaseRequisition entity = await _requisitionManager.CreateAsync(
            input.WarehouseId,
            input.RequestedDate,
            input.RequiredDate,
            input.Note);

        await _requisitionRepo.InsertAsync(entity);
        return ObjectMapper.Map<PurchaseRequisition, PurchaseRequisitionDto>(entity);
    }


    public async Task<PurchaseRequisitionDto> UpdateAsync(Guid id, UpdatePurchaseRequisitionDto input)
    {
        PurchaseRequisition entity = await _requisitionRepo.GetAsync(id);
        await _requisitionManager.UpdateAsync(entity, input.WarehouseId, input.RequiredDate, input.Note);
        await _requisitionRepo.UpdateAsync(entity);
        return ObjectMapper.Map<PurchaseRequisition, PurchaseRequisitionDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        PurchaseRequisition entity = await _requisitionRepo.GetAsync(id);
        if (entity.Status != Enums.Orders.PurchaseRequisitionStatus.Draft)
        {
            throw new UserFriendlyException("Chá»‰ cÃ³ thá»ƒ xÃ³a yÃªu cáº§u Ä‘ang á»Ÿ tráº¡ng thÃ¡i NhÃ¡p.");
        }
        await _requisitionRepo.DeleteAsync(entity);
    }

    public async Task AddLineAsync(Guid requisitionId, AddPurchaseRequisitionLineDto input)
    {
        IQueryable<PurchaseRequisition> query = await _requisitionRepo.GetQueryableAsync();
        PurchaseRequisition? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == requisitionId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseRequisition), requisitionId);
        }

        await _requisitionManager.AddLineAsync(entity, input.ProductId, input.UnitId, input.Quantity, input.Note);
        await _requisitionRepo.UpdateAsync(entity);
    }

    public async Task UpdateLineAsync(Guid requisitionId, Guid lineId, UpdatePurchaseRequisitionLineDto input)
    {
        IQueryable<PurchaseRequisition> query = await _requisitionRepo.GetQueryableAsync();
        PurchaseRequisition? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == requisitionId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseRequisition), requisitionId);
        }

        await _requisitionManager.UpdateLineAsync(entity, lineId, input.Quantity, input.Note);
        await _requisitionRepo.UpdateAsync(entity);
    }

    public async Task RemoveLineAsync(Guid requisitionId, Guid lineId)
    {
        IQueryable<PurchaseRequisition> query = await _requisitionRepo.GetQueryableAsync();
        PurchaseRequisition? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == requisitionId);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseRequisition), requisitionId);
        }

        await _requisitionManager.RemoveLineAsync(entity, lineId);
        await _requisitionRepo.UpdateAsync(entity);
    }

    public async Task SendToApproveAsync(Guid id)
    {
        IQueryable<PurchaseRequisition> query = await _requisitionRepo.GetQueryableAsync();
        PurchaseRequisition? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseRequisition), id);
        }

        await _requisitionManager.SendToApproveAsync(entity);
        await _requisitionRepo.UpdateAsync(entity);
    }

    public async Task ApproveAsync(Guid id)
    {
        PurchaseRequisition entity = await _requisitionRepo.GetAsync(id);
        await _requisitionManager.ApproveAsync(entity);
        await _requisitionRepo.UpdateAsync(entity);
    }

    public async Task RejectAsync(Guid id)
    {
        PurchaseRequisition entity = await _requisitionRepo.GetAsync(id);
        await _requisitionManager.RejectAsync(entity);
        await _requisitionRepo.UpdateAsync(entity);
    }

    public async Task ConvertToPurchaseOrderAsync(Guid id, ConvertToPurchaseOrderDto input)
    {
        IQueryable<PurchaseRequisition> query = await _requisitionRepo.GetQueryableAsync();
        PurchaseRequisition? requisition = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (requisition == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseRequisition), id);
        }

        List<(Guid RequisitionLineId, Guid SupplierId, Guid WarehouseId, decimal Quantity)> allocations = input.Allocations.Select(x => (x.RequisitionLineId, x.SupplierId, x.WarehouseId, x.Quantity)).ToList();

        List<PurchaseOrder> orders = await _orderManager.CreateOrdersFromRequisitionAsync(
            requisition,
            allocations,
            input.OrderDate,
            input.Note);

        foreach (PurchaseOrder order in orders)
        {
            await _orderRepo.InsertAsync(order);
        }

        await _requisitionRepo.UpdateAsync(requisition);
    }
}

