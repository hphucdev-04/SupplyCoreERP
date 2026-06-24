using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.PurchaseOrders.Dtos;
using SupplyCoreERP.PurchaseReturns.Dtos;
using SupplyCoreERP.SalesRecalls.Dtos;
using SupplyCoreERP.SalesOrders.Dtos;
using SupplyCoreERP.Tickets.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Tickets;

public interface IInventoryTicketAppService : IApplicationService
{
    Task<PagedResultDto<InventoryTicketDto>> GetListAsync(GetInventoryTicketListDto input);
    Task<InventoryTicketDto> GetAsync(Guid id);
    Task<InventoryTicketDto> CreateAsync(CreateInventoryTicketDto input);
    Task<InventoryTicketDto> UpdateAsync(Guid id, UpdateInventoryTicketDto input);
    Task DeleteAsync(Guid id);

    // Line & Detail
    Task<List<PurchaseOrderLineDto>> GetLinesFromPurchaseOrderAsync(Guid poId);
    Task AddLineFromPurchaseOrderAsync(Guid id, Guid poLineId, decimal quantity);

    Task<List<SalesOrderLineDto>> GetLinesFromSalesOrderAsync(Guid soId);
    Task AddLineFromSalesOrderAsync(Guid id, Guid soLineId, decimal quantity);

    Task<List<PurchaseReturnLineDto>> GetLinesFromPurchaseReturnAsync(Guid returnId);
    Task AddLineFromPurchaseReturnAsync(Guid id, Guid prLineId, decimal quantity);

    Task<List<SalesRecallLineDto>> GetLinesFromSalesRecallAsync(Guid recallId);
    Task AddLineFromSalesRecallAsync(Guid id, Guid recallLineId, decimal quantity);

    Task DeleteLineAsync(Guid id);

    // Detail
    Task AddDetailAsync(Guid lineId, AddTicketDetailDto input);
    Task DeleteDetailAsync(Guid id);

    // Workflow
    Task SendToApproveAsync(Guid id);
    Task ExecuteAsync(Guid id);

    //FEFO
    Task ApplyFEFOAsync(Guid id);

    // Get tickets by for traceability
    Task<List<InventoryTicketDto>> GetRelatedTicketsByPurchaseOrderAsync(Guid poId);
    Task<List<InventoryTicketDto>> GetRelatedTicketsBySaleOrderAsync(Guid soId);
}

