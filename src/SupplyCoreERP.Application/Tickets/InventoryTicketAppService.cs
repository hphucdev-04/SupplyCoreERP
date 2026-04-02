using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Tickets.Dtos;
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
		// 1. PHIẾU (MASTER)
		// ==========================================

		public async Task<PagedResultDto<InventoryTicketDto>> GetListAsync(GetInventoryTicketListDto input)
		{
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

			return new PagedResultDto<InventoryTicketDto>(
				totalCount,
				ObjectMapper.Map<List<InventoryTicket>, List<InventoryTicketDto>>(items));
		}

		public async Task<InventoryTicketDto> GetAsync(Guid id)
		{
			var query = await _ticketRepo.GetQueryableAsync();

			query = query
				.Include(x => x.Warehouse)
				.Include(x => x.Details).ThenInclude(d => d.Product).ThenInclude(p => p.BaseUnit)
				.Include(x => x.Details).ThenInclude(d => d.ProductBatch)
				.Include(x => x.Details).ThenInclude(d => d.Bin)
				.Include(x => x.Details).ThenInclude(d => d.Unit); // <-- Unit navigation mới

			var ticket = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
			if (ticket == null)
				throw new UserFriendlyException("Không tìm thấy Phiếu kho!");

			return ObjectMapper.Map<InventoryTicket, InventoryTicketDto>(ticket);
		}

		public async Task<InventoryTicketDto> CreateAsync(CreateInventoryTicketDto input)
		{
			var ticket = await _ticketManager.CreateTicketAsync(
				input.Type,
				input.WarehouseId,
				input.ReferenceDocumentId,
				input.ReferenceDocumentNumber,
				input.Note);

			await _ticketRepo.InsertAsync(ticket);
			return ObjectMapper.Map<InventoryTicket, InventoryTicketDto>(ticket);
		}

		public async Task<InventoryTicketDto> UpdateAsync(Guid id, UpdateInventoryTicketDto input)
		{
			var ticket = await _ticketRepo.GetAsync(id);
			_ticketManager.UpdateTicket(ticket, input.Note);
			await _ticketRepo.UpdateAsync(ticket);
			return await GetAsync(id);
		}

		public async Task DeleteAsync(Guid id)
		{
			var ticket = await _ticketRepo.GetAsync(id);
			await _ticketManager.ValidateBeforeDeleteAsync(ticket);
			await _ticketDetailRepo.DeleteAsync(x => x.TicketId == id);
			await _ticketRepo.DeleteAsync(ticket);
		}

		// ==========================================
		// 2. CHI TIẾT PHIẾU (DETAIL)
		// ==========================================

		public async Task<InventoryTicketDto> CreateTicketDetailAsync(Guid ticketId, AddTicketDetailDto input)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);

			var detail = await _ticketManager.CreateTicketDetailAsync(
				ticket,
				input.ProductId,
				input.ProductBatchId,
				input.BinId,
				input.UnitId,           
				input.ConversionFactor, 
				input.Quantity);

			await _ticketDetailRepo.InsertAsync(detail);
			return await GetAsync(ticketId);
		}

		public async Task UpdateDetailQuantityAsync(Guid detailId, decimal actualQuantity)
		{
			var detail = await _ticketDetailRepo.GetAsync(detailId);
			var ticket = await _ticketRepo.GetAsync(detail.TicketId);

			await _ticketManager.UpdateDetailQuantityAsync(ticket, detail, actualQuantity);

			await _ticketDetailRepo.UpdateAsync(detail);
		}

		public async Task RemoveDetailAsync(Guid ticketId, Guid detailId)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			var detail = await _ticketDetailRepo.GetAsync(detailId);

			await _ticketManager.RemoveTicketDetailAsync(ticket, detail);

			await _ticketDetailRepo.DeleteAsync(detail);
		}

		// ==========================================
		// 3. QUY TRÌNH DUYỆT & FEFO
		// ==========================================

		public async Task SendToApproveAsync(Guid id)
		{
			var ticket = await _ticketRepo.GetAsync(id);
			await _ticketManager.SendToApproveAsync(ticket);
			await _ticketRepo.UpdateAsync(ticket);
		}

		public async Task ExecuteAsync(Guid id)
		{
			var ticket = await _ticketRepo.GetAsync(id);
			await _ticketManager.ExecuteTicketAsync(ticket);
			await _ticketRepo.UpdateAsync(ticket);
		}

		public async Task RejectAsync(Guid id, string reason)
		{
			var ticket = await _ticketRepo.GetAsync(id);
			await _ticketManager.RejectTicketAsync(ticket, reason);
			await _ticketRepo.UpdateAsync(ticket);
		}

		/// <summary>
		/// FEFO nhận requiredBaseQuantity đã quy đổi về BaseUnit từ frontend.
		/// Domain sẽ tạo các detail với ConversionFactor=1 (vì đã là BaseUnit).
		/// </summary>
		public async Task AllocateFEFOAsync(Guid id, Guid productId, decimal requiredBaseQuantity)
		{
			var ticket = await _ticketRepo.GetAsync(id);
			await _ticketManager.AllocateFEFOAsync(ticket, productId, requiredBaseQuantity);
		}
	}
}