using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Permissions;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.PurchaseReturns.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.PurchaseReturns;

[Authorize(SupplyCoreERPPermissions.Order.PurchaseReturn.Default)]
public class PurchaseReturnAppService : SupplyCore, IPurchaseReturnAppService
{
    private readonly IRepository<PurchaseReturn, Guid> _purchaseReturnRepo;
    private readonly IPurchaseReturnManager _purchaseReturnManager;
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepo;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;

    public PurchaseReturnAppService(
        IRepository<PurchaseReturn, Guid> purchaseReturnRepo,
        IPurchaseReturnManager purchaseReturnManager,
        IRepository<PurchaseOrder, Guid> purchaseOrderRepo,
        IRepository<InventoryTicket, Guid> ticketRepo)
    {
        _purchaseReturnRepo = purchaseReturnRepo;
        _purchaseReturnManager = purchaseReturnManager;
        _purchaseOrderRepo = purchaseOrderRepo;
        _ticketRepo = ticketRepo;
    }

    public async Task<PagedResultDto<PurchaseReturnDto>> GetListAsync(GetPurchaseReturnListDto input)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Supplier)
            .Include(x => x.Warehouse);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter) || x.Supplier.Name.Contains(input.Filter))
            .WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(PurchaseReturn.CreationTime) + " DESC")
            .PageBy(input);

        List<PurchaseReturn> items = await AsyncExecuter.ToListAsync(query);

        List<PurchaseReturnDto> dtos = ObjectMapper.Map<List<PurchaseReturn>, List<PurchaseReturnDto>>(items);

        // Map PurchaseOrderCode
        if (dtos.Any())
        {
            var poIds = dtos.Select(d => d.PurchaseOrderId).Distinct().ToList();
            List<PurchaseOrder> pos = await _purchaseOrderRepo.GetListAsync(x => poIds.Contains(x.Id));
            var poDict = pos.ToDictionary(x => x.Id, x => x.Code);

            foreach (PurchaseReturnDto dto in dtos)
            {
                if (poDict.TryGetValue(dto.PurchaseOrderId, out string? poCode))
                {
                    dto.PurchaseOrderCode = poCode;
                }
            }
        }

        return new PagedResultDto<PurchaseReturnDto>(totalCount, dtos);
    }

    public async Task<PurchaseReturnDto> GetAsync(Guid id)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();

        PurchaseReturn? entity = await query
            .Include(x => x.Supplier)
            .Include(x => x.Warehouse)
            .Include(x => x.Lines).ThenInclude(l => l.Product)
            .Include(x => x.Lines).ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturn), id);
        }

        PurchaseReturnDto dto = ObjectMapper.Map<PurchaseReturn, PurchaseReturnDto>(entity);

        // Map PurchaseOrderCode
        PurchaseOrder? po = await _purchaseOrderRepo.FindAsync(entity.PurchaseOrderId);
        if (po != null)
        {
            dto.PurchaseOrderCode = po.Code;
        }

        // Traceability: PurchaseReturn -> Tickets
        List<InventoryTicket> tickets = await _ticketRepo.GetListAsync(x => x.ReferenceDocumentId == id);
        dto.RelatedTickets = tickets.Select(t => new PurchaseReturnRelatedTicketDto
        {
            Id = t.Id,
            TicketNumber = t.TicketNumber,
            Type = t.Type,
            Status = t.Status,
            CreationTime = t.CreationTime
        }).ToList();

        return dto;
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturn.Create)]
    public async Task<PurchaseReturnDto> CreateAsync(CreatePurchaseReturnDto input)
    {
        PurchaseReturn entity = await _purchaseReturnManager.CreateAsync(
            input.PurchaseOrderId,
            input.SupplierId,
            input.WarehouseId,
            input.ReturnDate,
            input.Note
        );

        await _purchaseReturnRepo.InsertAsync(entity);

        return ObjectMapper.Map<PurchaseReturn, PurchaseReturnDto>(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturn.Update)]
    public async Task<PurchaseReturnDto> UpdateAsync(Guid id, UpdatePurchaseReturnDto input)
    {
        PurchaseReturn entity = await _purchaseReturnRepo.GetAsync(id);

        await _purchaseReturnManager.UpdateAsync(
            entity,
            input.WarehouseId,
            input.ReturnDate,
            input.Note
        );

        await _purchaseReturnRepo.UpdateAsync(entity);

        return ObjectMapper.Map<PurchaseReturn, PurchaseReturnDto>(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturn.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        PurchaseReturn entity = await _purchaseReturnRepo.GetAsync(id);

        await _purchaseReturnManager.CheckBeforeDeleteAsync(entity);

        await _purchaseReturnRepo.DeleteAsync(entity);
    }

    public async Task AddLineAsync(Guid returnId, AddPurchaseReturnLineDto input)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == returnId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturn), returnId);
        }

        await _purchaseReturnManager.AddLineAsync(
            entity,
            input.PurchaseOrderLineId,
            input.ProductId,
            input.UnitId,
            input.ConversionFactor,
            input.Quantity,
            input.OriginalUnitPrice,
            input.DepreciationRate,
            input.TaxRate
        );

        await _purchaseReturnRepo.UpdateAsync(entity);
    }

    public async Task UpdateLineAsync(Guid returnId, Guid lineId, UpdatePurchaseReturnLineDto input)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == returnId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturn), returnId);
        }

        await _purchaseReturnManager.UpdateLineAsync(entity, lineId, input.Quantity, input.DepreciationRate);

        await _purchaseReturnRepo.UpdateAsync(entity);
    }

    public async Task RemoveLineAsync(Guid returnId, Guid lineId)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == returnId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturn), returnId);
        }

        await _purchaseReturnManager.RemoveLineAsync(entity, lineId);

        await _purchaseReturnRepo.UpdateAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturn.Approve)]
    public async Task SendToApproveAsync(Guid id)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(PurchaseReturn), id);

        await _purchaseReturnManager.SendToApproveAsync(entity);
        await _purchaseReturnRepo.UpdateAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturn.Approve)]
    public async Task ApproveAsync(Guid id)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(PurchaseReturn), id);

        await _purchaseReturnManager.ApproveAsync(entity);
        await _purchaseReturnRepo.UpdateAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturn.Reject)]
    public async Task RejectAsync(Guid id)
    {
        IQueryable<PurchaseReturn> query = await _purchaseReturnRepo.GetQueryableAsync();
        PurchaseReturn entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(PurchaseReturn), id);

        await _purchaseReturnManager.RejectAsync(entity);
        await _purchaseReturnRepo.UpdateAsync(entity);
    }
}
