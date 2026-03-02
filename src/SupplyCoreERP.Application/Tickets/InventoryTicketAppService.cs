using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Tickes.Dtos;
using SupplyCoreERP.Tickets.Dtos; // SỬA LỖI CHÍNH TẢ Ở ĐÂY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Tickets
{
	public class InventoryTicketAppService : ApplicationService, IInventoryTicketAppService
	{
		private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
		private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
		private readonly TicketManager _ticketManager;

		public InventoryTicketAppService(
			IRepository<InventoryTicket, Guid> ticketRepo,
			IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
			TicketManager ticketManager)
		{
			_ticketRepo = ticketRepo;
			_ticketDetailRepo = ticketDetailRepo;
			_ticketManager = ticketManager;
		}

		// ==========================================
		// 1. QUẢN LÝ PHIẾU (MASTER)
		// ==========================================
		public async Task<PagedResultDto<InventoryTicketDto>> GetListAsync(GetInventoryTicketListDto input)
		{
			// Include Warehouse để hiển thị tên kho
			var query = await _ticketRepo.WithDetailsAsync(x => x.Warehouse);

			query = query
				.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.TicketNumber.Contains(input.Filter))
				.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
				.WhereIf(input.Status.HasValue, x => x.Status == input.Status)
				.WhereIf(input.WarehouseId.HasValue, x => x.WarehouseId == input.WarehouseId);

			var totalCount = await AsyncExecuter.CountAsync(query);
			var items = await AsyncExecuter.ToListAsync(
				query.OrderBy(string.IsNullOrWhiteSpace(input.Sorting) ? "CreationTime DESC" : input.Sorting)
					 .Skip(input.SkipCount)
					 .Take(input.MaxResultCount)
			);

			return new PagedResultDto<InventoryTicketDto>(totalCount, ObjectMapper.Map<List<InventoryTicket>, List<InventoryTicketDto>>(items));
		}

		public async Task<InventoryTicketDto> GetAsync(Guid id)
		{
			// SỬA: Include Bin thay vì StorageLocation
			var query = await _ticketRepo.WithDetailsAsync(
				x => x.Warehouse,
				x => x.Details,
				x => x.Details.Select(d => d.Product),
				x => x.Details.Select(d => d.ProductBatch),
				x => x.Details.Select(d => d.Bin)
			);

			var ticket = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
			if (ticket == null) throw new UserFriendlyException("Không tìm thấy Phiếu kho!");

			return ObjectMapper.Map<InventoryTicket, InventoryTicketDto>(ticket);
		}

		public async Task<InventoryTicketDto> CreateAsync(CreateInventoryTicketDto input)
		{
			string ticketNumber = $"{input.Type.ToString().Substring(0, 3).ToUpper()}-{DateTime.Now:yyyyMMddHHmmss}";

			var ticket = new InventoryTicket(
				GuidGenerator.Create(), ticketNumber, input.Type, input.WarehouseId, input.ReferenceDocumentId, input.Note);

			await _ticketRepo.InsertAsync(ticket);
			return ObjectMapper.Map<InventoryTicket, InventoryTicketDto>(ticket);
		}

		public async Task<InventoryTicketDto> UpdateAsync(Guid id, UpdateInventoryTicketDto input)
		{
			await _ticketManager.UpdateTicketAsync(id, input.Note);
			return await GetAsync(id);
		}

		public async Task DeleteAsync(Guid id) => await _ticketManager.DeleteTicketAsync(id);

		// ==========================================
		// 2. QUẢN LÝ CHI TIẾT PHIẾU (DETAIL)
		// ==========================================
		public async Task<InventoryTicketDto> AddDetailAsync(Guid ticketId, AddTicketDetailDto input)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			if (ticket.Status != ApprovalStatus.Draft && ticket.Status != ApprovalStatus.Pending)
				throw new UserFriendlyException("Chỉ có thể thêm chi tiết vào Phiếu Nháp hoặc Chờ duyệt!");

			// SỬA: Đảm bảo mapping BinId đúng
			var detail = new InventoryTicketDetail(
				GuidGenerator.Create(), ticketId, input.ProductId, input.ProductBatchId, input.BinId, input.Quantity);

			await _ticketDetailRepo.InsertAsync(detail);
			return await GetAsync(ticketId);
		}

		public async Task UpdateDetailQuantityAsync(Guid detailId, decimal actualQuantity)
		{
			await _ticketManager.UpdateDetailQuantityAsync(detailId, actualQuantity);
		}

		public async Task RemoveDetailAsync(Guid ticketId, Guid detailId)
		{
			// SỬA: Gọi Manager để xử lý logic Unlock tồn kho (nếu cần)
			// Không được xóa trực tiếp Repo ở đây vì sẽ gây lệch tồn kho
			await _ticketManager.RemoveTicketDetailAsync(ticketId, detailId);
		}

		// ==========================================
		// 3. QUY TRÌNH DUYỆT & FEFO
		// ==========================================
		public async Task SendToApproveAsync(Guid id)
		{
			var ticket = await _ticketRepo.GetAsync(id);
			ticket.RequestApprove();
			await _ticketRepo.UpdateAsync(ticket);
		}

		public async Task ExecuteAsync(Guid id)
		{
			await _ticketManager.ExecuteTicketAsync(id);
		}

		public async Task RejectAsync(Guid id, string reason)
		{
			await _ticketManager.RejectTicketAsync(id, reason);
		}

		public async Task AllocateFEFOAsync(Guid id, Guid productId, decimal requiredQuantity)
		{
			await _ticketManager.AllocateFEFOAsync(id, productId, requiredQuantity);
		}
	}
}