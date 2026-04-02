using SupplyCoreERP.Tickets.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Tickets
{
	public interface IInventoryTicketAppService : IApplicationService
	{
		// ── Phiếu (master) ────────────────────────────────────
		Task<PagedResultDto<InventoryTicketDto>> GetListAsync(GetInventoryTicketListDto input);
		Task<InventoryTicketDto> GetAsync(Guid id);
		Task<InventoryTicketDto> CreateAsync(CreateInventoryTicketDto input);
		Task<InventoryTicketDto> UpdateAsync(Guid id, UpdateInventoryTicketDto input);
		Task DeleteAsync(Guid id);

		// ── Chi tiết phiếu ────────────────────────────────────
		Task<InventoryTicketDto> CreateTicketDetailAsync(Guid ticketId, AddTicketDetailDto input);
		Task UpdateDetailQuantityAsync(Guid detailId, decimal actualQuantity);
		Task RemoveDetailAsync(Guid ticketId, Guid detailId);

		// ── Quy trình duyệt ───────────────────────────────────
		Task SendToApproveAsync(Guid id);
		Task ExecuteAsync(Guid id);
		Task RejectAsync(Guid id, string reason);

		/// <summary>
		/// Cấp phát hàng tự động theo FEFO.
		/// <param name="requiredBaseQuantity">
		///     Số lượng cần xuất, ĐÃ quy về BaseUnit.
		///     Frontend tính: requiredQuantity × conversionFactor rồi truyền vào.
		/// </param>
		/// </summary>
		Task AllocateFEFOAsync(Guid id, Guid productId, decimal requiredBaseQuantity);
	}
}