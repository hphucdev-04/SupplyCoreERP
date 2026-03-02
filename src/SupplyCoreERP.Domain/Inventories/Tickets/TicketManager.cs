using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Warehouses;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventories.Tickets
{
	public class TicketManager : DomainService
	{
		private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
		private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepo;
		private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
		private readonly IRepository<InventoryTransaction, Guid> _transactionRepo;
		private readonly IRepository<ProductBatch, Guid> _batchRepo;
		private readonly IRepository<Bin, Guid> _binRepo;

		public TicketManager(
			IRepository<InventoryTicket, Guid> ticketRepo, 
			IRepository<InventoryTicketDetail, Guid> ticketDetailRepo, 
			IRepository<InventoryBalance, Guid> balanceRepo, 
			IRepository<InventoryTransaction, Guid> transactionRepo, 
			IRepository<ProductBatch, Guid> batchRepo,
			IRepository<Bin, Guid> binRepo)
		{
			_ticketRepo = ticketRepo; 
			_ticketDetailRepo = ticketDetailRepo; 
			_balanceRepo = balanceRepo;
			_transactionRepo = transactionRepo; 
			_batchRepo = batchRepo; 
			_binRepo = binRepo;
		}

		// ==========================================
		// QUẢN LÝ PHIẾU (CẬP NHẬT & XÓA)
		// ==========================================
		public async Task UpdateTicketAsync(Guid ticketId, string? note)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			if (ticket.Status == ApprovalStatus.Approved) throw new UserFriendlyException("Không thể sửa Phiếu đã thực thi!");
			ticket.UpdateNote(note);
			await _ticketRepo.UpdateAsync(ticket);
		}

		public async Task UpdateDetailQuantityAsync(Guid detailId, decimal actualQuantity)
		{
			if (actualQuantity < 0) throw new UserFriendlyException("Số lượng không được âm!");
			var detail = await _ticketDetailRepo.GetAsync(detailId);
			var ticket = await _ticketRepo.GetAsync(detail.TicketId);

			if (ticket.Status == ApprovalStatus.Approved) throw new UserFriendlyException("Không thể sửa chi tiết của Phiếu đã thực thi!");

			if (ticket.Status == ApprovalStatus.Pending && (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue))
			{
				var diff = detail.Quantity - actualQuantity;
				if (diff > 0)
				{
					var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == detail.BinId && x.ProductBatchId == detail.ProductBatchId);
					if (balance != null) { balance.UnlockStock(diff); await _balanceRepo.UpdateAsync(balance); }
				}
				else if (diff < 0) throw new UserFriendlyException("Không thể tự ý tăng số lượng xuất lớn hơn chỉ định FEFO!");
			}

			detail.UpdateActualQuantity(actualQuantity);
			await _ticketDetailRepo.UpdateAsync(detail);
		}

		public async Task DeleteTicketAsync(Guid ticketId)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			if (ticket.Status == ApprovalStatus.Approved) throw new UserFriendlyException("Không thể xóa Phiếu đã duyệt!");

			if (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward)
			{
				var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticketId);
				foreach (var item in details)
				{
					var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId);
					if (balance != null) { balance.UnlockStock(item.Quantity); await _balanceRepo.UpdateAsync(balance); }
				}
			}
			await _ticketDetailRepo.DeleteAsync(x => x.TicketId == ticketId);
			await _ticketRepo.DeleteAsync(ticketId);
		}


		public async Task RemoveTicketDetailAsync(Guid ticketId, Guid detailId)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể xóa chi tiết của Phiếu đã duyệt!");

			var detail = await _ticketDetailRepo.GetAsync(detailId);

			// LOGIC QUAN TRỌNG:
			// Nếu là Phiếu Xuất/Hủy/Trả và đang Chờ duyệt -> Nghĩa là hàng đã bị Lock.
			// Khi xóa chi tiết -> Phải Unlock trả lại kho.
			if (ticket.Status == ApprovalStatus.Pending &&
			   (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward))
			{
				var balance = await _balanceRepo.FirstOrDefaultAsync(
					x => x.BinId == detail.BinId && x.ProductBatchId == detail.ProductBatchId);

				if (balance != null)
				{
					balance.UnlockStock(detail.Quantity);
					await _balanceRepo.UpdateAsync(balance);
				}
			}

			await _ticketDetailRepo.DeleteAsync(detail);
		}

		// ==========================================
		// DUYỆT VÀ TỪ CHỐI PHIẾU
		// ==========================================
		public async Task RejectTicketAsync(Guid ticketId, string rejectReason)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			if (ticket.Status != ApprovalStatus.Pending) throw new UserFriendlyException("Chỉ có thể từ chối phiếu đang chờ duyệt!");

			if (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward)
			{
				var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticketId);
				foreach (var item in details)
				{
					var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId);
					if (balance != null) { balance.UnlockStock(item.Quantity); await _balanceRepo.UpdateAsync(balance); }
				}
			}

			ticket.Reject();
			ticket.UpdateNote($"[Từ chối: {rejectReason}] " + ticket.Note);
			await _ticketRepo.UpdateAsync(ticket);
		}

		public async Task ExecuteTicketAsync(Guid ticketId)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			// ... (Validate Status giữ nguyên)

			var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticketId);
			InventoryTransactionType transType = MapTicketToTransaction(ticket.Type);

			foreach (var item in details)
			{
				// Sửa: Dùng BinId
				var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.WarehouseId == ticket.WarehouseId && x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId);
				bool isIncoming = ticket.Type == TicketType.GoodsReceipt || ticket.Type == TicketType.ReturnInward || ticket.Type == TicketType.RecallReceipt;

				if (isIncoming)
				{
					if (balance == null)
					{
						// Sửa: BinId
						balance = new InventoryBalance(GuidGenerator.Create(), ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId, item.Quantity);
						await _balanceRepo.InsertAsync(balance);
					}
					else
					{
						balance.AddStock(item.Quantity);
						await _balanceRepo.UpdateAsync(balance);
					}
				}
				else
				{
					if (balance == null) throw new UserFriendlyException($"Không có tồn kho cho sản phẩm: {item.ProductId} tại vị trí này!");
					balance.UnlockStock(item.Quantity);
					balance.RemoveStock(item.Quantity);
					await _balanceRepo.UpdateAsync(balance);
				}

				await _transactionRepo.InsertAsync(new InventoryTransaction(GuidGenerator.Create(), ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId, transType, item.Quantity, balance.Quantity, ticket.Id, ticket.Note));
			}

			ticket.Execute();
			await _ticketRepo.UpdateAsync(ticket);
		}

		private InventoryTransactionType MapTicketToTransaction(TicketType type)
		{
			return type switch
			{
				TicketType.GoodsReceipt => InventoryTransactionType.PurchaseReceipt,
				TicketType.GoodsIssue => InventoryTransactionType.SaleDelivery,
				TicketType.ReturnInward => InventoryTransactionType.ReturnInward,
				TicketType.ReturnOutward => InventoryTransactionType.ReturnOutward,
				TicketType.RecallReceipt => InventoryTransactionType.RecallReceipt,
				TicketType.DisposalIssue => InventoryTransactionType.Disposal,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		// ==========================================
		// THUẬT TOÁN FEFO (TỐI ƯU SQL QUERY IQueryable)
		// ==========================================
		public async Task AllocateFEFOAsync(Guid ticketId, Guid productId, decimal requiredQuantity)
		{
			var ticket = await _ticketRepo.GetAsync(ticketId);
			decimal remaining = requiredQuantity;
			DateTime now = DateTime.Now; // Lấy giờ ra biến để query ổn định

			var balanceQuery = await _balanceRepo.GetQueryableAsync();
			var batchQuery = await _batchRepo.GetQueryableAsync();
			var binQuery = await _binRepo.GetQueryableAsync();

			// Query FEFO: Lấy các dòng tồn kho thỏa mãn
			var query = from bal in balanceQuery
						join bat in batchQuery on bal.ProductBatchId equals bat.Id
						join bin in binQuery on bal.BinId equals bin.Id
						where bal.WarehouseId == ticket.WarehouseId
						   && bal.ProductId == productId
						   && (bal.Quantity - bal.LockedQuantity) > 0
						   && bat.Status == BatchQAStatus.Approved
						   && !bin.IsBlocked // Check Bin không bị khóa
						   && bat.ExpiryDate > now
						orderby bat.ExpiryDate ascending, (bal.Quantity - bal.LockedQuantity) ascending
						select new { bal, bat, bin }; // Select entity gốc để EF track

			// Thực thi query Async
			var availableStocks = await AsyncExecuter.ToListAsync(query);

			foreach (var stock in availableStocks)
			{
				if (remaining <= 0) break;

				decimal availableQty = stock.bal.Quantity - stock.bal.LockedQuantity;
				decimal takeQty = Math.Min(availableQty, remaining);

				stock.bal.LockStock(takeQty);
				await _balanceRepo.UpdateAsync(stock.bal);

				// Tạo Detail với BinId
				await _ticketDetailRepo.InsertAsync(new InventoryTicketDetail(
					GuidGenerator.Create(), ticketId, productId, stock.bat.Id, stock.bin.Id, takeQty));

				remaining -= takeQty;
			}

			if (remaining > 0) throw new UserFriendlyException($"Không đủ tồn kho ĐẠT CHUẨN để xuất! Còn thiếu {remaining:N0} đơn vị.");
		}
	}
}