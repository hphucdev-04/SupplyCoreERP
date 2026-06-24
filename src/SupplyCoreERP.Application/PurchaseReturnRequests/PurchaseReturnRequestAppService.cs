using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Permissions;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturnRequests;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.PurchaseReturnRequests.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.PurchaseReturnRequests;

public class PurchaseReturnRequestAppService : SupplyCore, IPurchaseReturnRequestAppService
{
    private readonly IRepository<PurchaseReturnRequest, Guid> _requestRepo;
    private readonly PurchaseReturnRequestManager _requestManager;
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepo;
    private readonly IRepository<PurchaseReturn, Guid> _purchaseReturnRepo;
    private readonly IRepository<Supplier, Guid> _supplierRepo;

    public PurchaseReturnRequestAppService(
        IRepository<PurchaseReturnRequest, Guid> requestRepo,
        PurchaseReturnRequestManager requestManager,
        IRepository<PurchaseOrder, Guid> purchaseOrderRepo,
        IRepository<PurchaseReturn, Guid> purchaseReturnRepo,
        IRepository<Supplier, Guid> supplierRepo)
    {
        _requestRepo = requestRepo;
        _requestManager = requestManager;
        _purchaseOrderRepo = purchaseOrderRepo;
        _purchaseReturnRepo = purchaseReturnRepo;
        _supplierRepo = supplierRepo;
    }

    public async Task<PagedResultDto<PurchaseReturnRequestDto>> GetListAsync(GetPurchaseReturnRequestListDto input)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Warehouse);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter))
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(PurchaseReturnRequest.CreationTime) + " DESC")
            .PageBy(input);

        List<PurchaseReturnRequest> items = await AsyncExecuter.ToListAsync(query);

        List<PurchaseReturnRequestDto> dtos = ObjectMapper.Map<List<PurchaseReturnRequest>, List<PurchaseReturnRequestDto>>(items);

        return new PagedResultDto<PurchaseReturnRequestDto>(totalCount, dtos);
    }

    public async Task<PurchaseReturnRequestDto> GetAsync(Guid id)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();

        PurchaseReturnRequest? entity = await query
            .Include(x => x.Warehouse)
            .Include(x => x.Lines).ThenInclude(l => l.Product)
            .Include(x => x.Lines).ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), id);
        }

        PurchaseReturnRequestDto dto = ObjectMapper.Map<PurchaseReturnRequest, PurchaseReturnRequestDto>(entity);

        // Map PurchaseOrderCode và Supplier cho các Lines
        if (dto.Lines.Any())
        {
            List<Guid> poIds = dto.Lines.Select(l => l.PurchaseOrderId).Distinct().ToList();
            List<PurchaseOrder> pos = await _purchaseOrderRepo.GetListAsync(x => poIds.Contains(x.Id));

            List<Guid> supplierIds = pos.Select(p => p.SupplierId).Distinct().ToList();
            List<Supplier> suppliers = await _supplierRepo.GetListAsync(s => supplierIds.Contains(s.Id));

            Dictionary<Guid, PurchaseOrder> poDict = pos.ToDictionary(x => x.Id);
            Dictionary<Guid, Supplier> supplierDict = suppliers.ToDictionary(x => x.Id);

            foreach (PurchaseReturnRequestLineDto line in dto.Lines)
            {
                if (poDict.TryGetValue(line.PurchaseOrderId, out PurchaseOrder? po))
                {
                    line.PurchaseOrderCode = po.Code;
                    line.SupplierId = po.SupplierId;
                    if (supplierDict.TryGetValue(po.SupplierId, out Supplier? supplier))
                    {
                        line.SupplierName = supplier.Name;
                        line.SupplierCode = supplier.Code;
                    }
                }
            }
        }

        // Load các tickets con sinh ra từ yêu cầu trả hàng này (RelatedTickets)
        List<PurchaseReturn> relatedReturns = await _purchaseReturnRepo.GetListAsync(x => x.PurchaseReturnRequestId == id);
        dto.RelatedTickets = relatedReturns.Select(t => new PurchaseReturnRequestRelatedTicketDto
        {
            Id = t.Id,
            TicketNumber = t.Code,
            Type = 1, // 1 tượng trưng cho phiếu PurchaseReturn con sinh ra
            Status = (int)t.Status,
            CreationTime = t.CreationTime
        }).ToList();

        return dto;
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Create)]
    public async Task<PurchaseReturnRequestDto> CreateAsync(CreatePurchaseReturnRequestDto input)
    {
        PurchaseReturnRequest entity = await _requestManager.CreateAsync(
            input.WarehouseId,
            input.RequestDate,
            input.Note
        );

        await _requestRepo.InsertAsync(entity);

        return ObjectMapper.Map<PurchaseReturnRequest, PurchaseReturnRequestDto>(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Update)]
    public async Task<PurchaseReturnRequestDto> UpdateAsync(Guid id, UpdatePurchaseReturnRequestDto input)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();
        PurchaseReturnRequest? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), id);
        }

        entity.UpdateInfo(
            input.WarehouseId,
            input.RequestDate,
            input.Note
        );

        await _requestRepo.UpdateAsync(entity);

        return ObjectMapper.Map<PurchaseReturnRequest, PurchaseReturnRequestDto>(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        PurchaseReturnRequest entity = await _requestRepo.GetAsync(id);

        if (entity.Status != PurchaseReturnRequestStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ có thể xóa yêu cầu trả hàng ở trạng thái Nháp!");
        }

        await _requestRepo.DeleteAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Update)]
    public async Task AddLineAsync(Guid requestId, AddPurchaseReturnRequestLineDto input)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();
        PurchaseReturnRequest? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == requestId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), requestId);
        }

        await _requestManager.AddLineAsync(
            entity,
            input.ProductId,
            input.UnitId,
            input.ConversionFactor,
            input.PurchaseOrderId,
            input.PurchaseOrderLineId,
            input.Quantity,
            input.OriginalUnitPrice,
            input.DepreciationRate,
            input.TaxRate,
            input.ReturnType
        );

        await _requestRepo.UpdateAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Update)]
    public async Task UpdateLineAsync(Guid requestId, Guid lineId, UpdatePurchaseReturnRequestLineDto input)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();
        PurchaseReturnRequest? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == requestId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), requestId);
        }

        await _requestManager.UpdateLineAsync(
            entity,
            lineId,
            input.Quantity,
            input.DepreciationRate,
            input.ReturnType
        );

        await _requestRepo.UpdateAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Update)]
    public async Task RemoveLineAsync(Guid requestId, Guid lineId)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();
        PurchaseReturnRequest? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == requestId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), requestId);
        }

        entity.RemoveLine(lineId);

        await _requestRepo.UpdateAsync(entity);
    }

    public async Task SendToApproveAsync(Guid id)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();
        PurchaseReturnRequest? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), id);
        }

        entity.SendToApprove();

        await _requestRepo.UpdateAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Approve)]
    public async Task ApproveAndSplitAsync(Guid id)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();
        PurchaseReturnRequest? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), id);
        }

        await _requestManager.ApproveAndSplitAsync(entity);

        await _requestRepo.UpdateAsync(entity);
    }

    [Authorize(SupplyCoreERPPermissions.Order.PurchaseReturnRequest.Reject)]
    public async Task RejectAsync(Guid id)
    {
        IQueryable<PurchaseReturnRequest> query = await _requestRepo.GetQueryableAsync();
        PurchaseReturnRequest? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(PurchaseReturnRequest), id);
        }

        entity.Reject();

        await _requestRepo.UpdateAsync(entity);
    }
}
