using System;
using System.Threading.Tasks;
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

    Task CreateTicketDetailAsync(Guid ticketId, AddTicketDetailDto input);
    Task UpdateDetailQuantityAsync(Guid detailId, decimal actualQuantity);
    Task RemoveDetailAsync(Guid ticketId, Guid detailId);

    Task SendToApproveAsync(Guid id);
    Task ExecuteAsync(Guid id);
    Task RejectAsync(Guid id, string reason);
    Task AllocateFEFOAsync(Guid id, Guid productId, decimal requiredBaseQuantity);
}
