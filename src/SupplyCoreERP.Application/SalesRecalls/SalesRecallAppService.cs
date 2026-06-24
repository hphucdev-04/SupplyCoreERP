using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Permissions;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Sales.SalesRecalls;
using SupplyCoreERP.SalesRecalls.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.SalesRecalls;

public class SalesRecallAppService : SupplyCore, ISalesRecallAppService
{
    private readonly IRepository<SalesRecall, Guid> _salesRecallRepo;
    private readonly ISalesRecallManager _salesRecallManager;
    private readonly IRepository<SalesOrder, Guid> _salesOrderRepo;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
    private readonly IRepository<ProductBatch, Guid> _productBatchRepo;

    public SalesRecallAppService(
        IRepository<SalesRecall, Guid> salesRecallRepo,
        ISalesRecallManager salesRecallManager,
        IRepository<SalesOrder, Guid> salesOrderRepo,
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
        IRepository<ProductBatch, Guid> productBatchRepo)
    {
        _salesRecallRepo = salesRecallRepo;
        _salesRecallManager = salesRecallManager;
        _salesOrderRepo = salesOrderRepo;
        _ticketRepo = ticketRepo;
        _ticketDetailRepo = ticketDetailRepo;
        _productBatchRepo = productBatchRepo;
    }

    public async Task<PagedResultDto<SalesRecallDto>> GetListAsync(GetSalesRecallListDto input)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();

        query = query
            .Include(x => x.Product)
            .Include(x => x.ProductBatch)
            .Include(x => x.Warehouse);

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Code.Contains(input.Filter) || x.Product.Name.Contains(input.Filter))
            .WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status);

        // Lọc theo CustomerId nếu có (tìm các quyết định thu hồi có chứa dòng của khách hàng này)
        if (input.CustomerId.HasValue)
        {
            query = query.Where(x => x.Lines.Any(l => l.CustomerId == input.CustomerId.Value));
        }

        int totalCount = await AsyncExecuter.CountAsync(query);

        query = query
            .OrderBy(input.Sorting ?? nameof(SalesRecall.CreationTime) + " DESC")
            .PageBy(input);

        List<SalesRecall> items = await AsyncExecuter.ToListAsync(query);

        List<SalesRecallDto> dtos = ObjectMapper.Map<List<SalesRecall>, List<SalesRecallDto>>(items);

        return new PagedResultDto<SalesRecallDto>(totalCount, dtos);
    }

    public async Task<SalesRecallDto> GetAsync(Guid id)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();

        SalesRecall? entity = await query
            .Include(x => x.Product).ThenInclude(p => p.BaseUnit)
            .Include(x => x.ProductBatch)
            .Include(x => x.Warehouse)
            .Include(x => x.Lines).ThenInclude(l => l.Customer)
            .Include(x => x.Lines).ThenInclude(l => l.SalesOrder).ThenInclude(o => o.Lines).ThenInclude(ol => ol.Unit)
            .Include(x => x.Lines).ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesRecall), id);
        }

        SalesRecallDto dto = ObjectMapper.Map<SalesRecall, SalesRecallDto>(entity);
        dto.BaseUnitName = entity.Product?.BaseUnit?.Name;

        foreach (var lineDto in dto.Lines)
        {
            var lineEntity = entity.Lines.FirstOrDefault(x => x.Id == lineDto.Id);
            if (lineEntity != null)
            {
                lineDto.RecalledQuantity = lineEntity.RecalledQuantity;
                lineDto.RecalledBaseQuantity = lineEntity.RecalledQuantity * lineEntity.ConversionFactor;

                if (lineEntity.SalesOrder != null)
                {
                    var soLine = lineEntity.SalesOrder.Lines.FirstOrDefault(l => l.ProductId == entity.ProductId);
                    if (soLine != null)
                    {
                        lineDto.SalesOrderQuantity = soLine.Quantity;
                        lineDto.SalesOrderUnitName = soLine.Unit?.Name;
                        lineDto.SalesOrderBaseQuantity = soLine.BaseQuantity;
                    }
                }
            }
        }

        // Traceability: SalesRecall -> Tickets
        List<InventoryTicket> tickets = await _ticketRepo.GetListAsync(x => x.ReferenceDocumentId == id);
        dto.RelatedTickets = tickets.Select(t => new SalesRecallRelatedTicketDto
        {
            Id = t.Id,
            TicketNumber = t.TicketNumber,
            Type = t.Type,
            Status = t.Status,
            CreationTime = t.CreationTime
        }).ToList();

        return dto;
    }

    [Authorize(SupplyCoreERPPermissions.Order.SalesRecall.Create)]
    public async Task<SalesRecallDto> CreateAsync(CreateSalesRecallDto input)
    {
        SalesRecall entity = await _salesRecallManager.CreateAsync(
            input.ProductId,
            input.ProductBatchId,
            input.WarehouseId,
            input.RecallDate,
            input.Level,
            input.RecallDecisionNumber,
            input.Note
        );

        await _salesRecallRepo.InsertAsync(entity);

        return ObjectMapper.Map<SalesRecall, SalesRecallDto>(entity);
    }

    public async Task<SalesRecallDto> UpdateAsync(Guid id, UpdateSalesRecallDto input)
    {
        SalesRecall entity = await _salesRecallRepo.GetAsync(id);

        await _salesRecallManager.UpdateAsync(
            entity,
            input.WarehouseId,
            input.RecallDate,
            input.Level,
            input.RecallDecisionNumber,
            input.Note
        );

        await _salesRecallRepo.UpdateAsync(entity);

        return ObjectMapper.Map<SalesRecall, SalesRecallDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        SalesRecall entity = await _salesRecallRepo.GetAsync(id);

        await _salesRecallManager.CheckBeforeDeleteAsync(entity);

        await _salesRecallRepo.DeleteAsync(entity);
    }

    public async Task AddLineAsync(Guid recallId, AddSalesRecallLineDto input)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == recallId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesRecall), recallId);
        }

        await _salesRecallManager.AddLineAsync(
            entity,
            input.CustomerId,
            input.SalesOrderId,
            input.UnitId,
            input.ConversionFactor,
            input.Quantity,
            input.OriginalUnitPrice,
            input.TaxRate
        );

        await _salesRecallRepo.UpdateAsync(entity);
    }

    public async Task UpdateLineAsync(Guid recallId, Guid lineId, UpdateSalesRecallLineDto input)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == recallId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesRecall), recallId);
        }

        await _salesRecallManager.UpdateLineAsync(entity, lineId, input.Quantity);

        await _salesRecallRepo.UpdateAsync(entity);
    }

    public async Task RemoveLineAsync(Guid recallId, Guid lineId)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall? entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == recallId);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SalesRecall), recallId);
        }

        await _salesRecallManager.RemoveLineAsync(entity, lineId);

        await _salesRecallRepo.UpdateAsync(entity);
    }

    public async Task SendToApproveAsync(Guid id)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(SalesRecall), id);

        await _salesRecallManager.SendToApproveAsync(entity);
        await _salesRecallRepo.UpdateAsync(entity);
    }

    public async Task ApproveAsync(Guid id)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(SalesRecall), id);

        await _salesRecallManager.ApproveAsync(entity);
        await _salesRecallRepo.UpdateAsync(entity);
    }

    public async Task RejectAsync(Guid id)
    {
        IQueryable<SalesRecall> query = await _salesRecallRepo.GetQueryableAsync();
        SalesRecall entity = await query.Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(typeof(SalesRecall), id);

        await _salesRecallManager.RejectAsync(entity);
        await _salesRecallRepo.UpdateAsync(entity);
    }

    public async Task<List<CustomerRecallTraceDto>> TraceCustomersByBatchAsync(Guid productBatchId)
    {
        // 1. Tìm các chi tiết phiếu xuất kho bán hàng (TicketType == TicketType.GoodsIssue) có ProductBatchId này
        // Phiếu xuất kho phải ở trạng thái đã duyệt/thực hiện hoàn tất (Status == ApprovalStatus.Approved)
        IQueryable<InventoryTicketDetail> detailQuery = await _ticketDetailRepo.GetQueryableAsync();
        List<InventoryTicketDetail> ticketDetails = await AsyncExecuter.ToListAsync(
            detailQuery
                .Include(x => x.TicketLine).ThenInclude(l => l.Ticket)
                .Include(x => x.Unit)
                .Include(x => x.ProductBatch)
                .Where(x =>
                    x.ProductBatchId == productBatchId &&
                    x.TicketLine.Ticket.Type == TicketType.GoodsIssue &&
                    x.TicketLine.Ticket.Status == ApprovalStatus.Approved)
        );

        List<CustomerRecallTraceDto> list = new();

        if (!ticketDetails.Any())
        {
            return list;
        }

        // 2. Lấy danh sách ReferenceDocumentId (chính là SalesOrderId)
        List<Guid> soIds = ticketDetails
            .Where(x => x.TicketLine.Ticket.ReferenceDocumentId.HasValue)
            .Select(x => x.TicketLine.Ticket.ReferenceDocumentId!.Value)
            .Distinct()
            .ToList();

        if (!soIds.Any())
        {
            return list;
        }

        // 3. Query thông tin SalesOrders kèm theo Customer và Lines
        IQueryable<SalesOrder> soQuery = await _salesOrderRepo.GetQueryableAsync();
        List<SalesOrder> salesOrders = await AsyncExecuter.ToListAsync(
            soQuery
                .Include(x => x.Customer)
                .Include(x => x.Lines).ThenInclude(l => l.Unit)
                .Where(x => soIds.Contains(x.Id))
        );

        Dictionary<Guid, SalesOrder> soDict = salesOrders.ToDictionary(x => x.Id, x => x);

        // 4. Map thông tin ra DTO
        foreach (InventoryTicketDetail? detail in ticketDetails)
        {
            if (detail.TicketLine.Ticket.ReferenceDocumentId.HasValue)
            {
                Guid soId = detail.TicketLine.Ticket.ReferenceDocumentId.Value;
                if (soDict.TryGetValue(soId, out SalesOrder? so))
                {
                    var soLine = so.Lines.FirstOrDefault(x => x.ProductId == detail.ProductId);
                    decimal unitPrice = 0;
                    decimal taxRate = 0;
                    decimal traceQuantity = 0;
                    Guid traceUnitId = Guid.Empty;
                    string traceUnitName = string.Empty;
                    int conversionFactor = 1;

                    if (soLine != null)
                    {
                        // Đơn giá bán lịch sử sau chiết khấu
                        unitPrice = soLine.UnitPrice * (1 - soLine.DiscountRate / 100);
                        taxRate = soLine.TaxRate;
                        conversionFactor = soLine.ConversionFactor;
                        traceQuantity = detail.BaseQuantity / soLine.ConversionFactor;
                        traceUnitId = soLine.UnitId;
                        traceUnitName = soLine.Unit?.Name ?? string.Empty;
                    }
                    else
                    {
                        traceQuantity = detail.Quantity;
                        traceUnitId = detail.UnitId;
                        traceUnitName = detail.Unit?.Name ?? string.Empty;
                        conversionFactor = detail.ConversionFactor;
                    }

                    list.Add(new CustomerRecallTraceDto
                    {
                        CustomerId = so.CustomerId,
                        CustomerCode = so.Customer.Code,
                        CustomerName = so.Customer.Name,
                        SalesOrderId = so.Id,
                        SalesOrderCode = so.Code,
                        SalesOrderDate = so.OrderDate,
                        ProductBatchId = detail.ProductBatchId,
                        BatchNumber = detail.ProductBatch?.BatchNumber ?? string.Empty,
                        Quantity = traceQuantity,
                        UnitName = traceUnitName,
                        UnitId = traceUnitId,
                        UnitPrice = unitPrice,
                        TaxRate = taxRate,
                        ConversionFactor = conversionFactor
                    });
                }
            }
        }

        return list;
    }
}
