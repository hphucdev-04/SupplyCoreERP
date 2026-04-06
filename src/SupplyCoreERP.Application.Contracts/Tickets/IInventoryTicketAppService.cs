using SupplyCoreERP.Tickets.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Tickets
{
	public interface IInventoryTicketAppService : IApplicationService
	{
		// Ticket
		Task<PagedResultDto<InventoryTicketDto>> GetListAsync(GetInventoryTicketListDto input);
		Task<InventoryTicketDto> GetAsync(Guid id);
		Task<InventoryTicketDto> CreateAsync(CreateInventoryTicketDto input);
		Task<InventoryTicketDto> UpdateAsync(Guid id, UpdateInventoryTicketDto input);
		Task DeleteAsync(Guid id);

		// TicketDetail
		Task<InventoryTicketDto> CreateTicketDetailAsync(Guid ticketId, AddTicketDetailDto input);
		Task<InventoryTicketDto> UpdateDetailQuantityAsync(Guid detailId, decimal actualQuantity);
		Task<InventoryTicketDto> RemoveDetailAsync(Guid ticketId, Guid detailId);

		// TicketFlow
		Task<InventoryTicketDto> SendToApproveAsync(Guid id);
		Task<InventoryTicketDto> ExecuteAsync(Guid id);
		Task<InventoryTicketDto> RejectAsync(Guid id, string reason);

		// FEFO
		Task<InventoryTicketDto> AllocateFEFOAsync(Guid id, Guid productId, decimal requiredBaseQuantity);
	}
}