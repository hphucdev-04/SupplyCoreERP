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
		private readonly IRepository<Warehouse, Guid> _warehouseRepo;

		public TicketManager(
			IRepository<InventoryTicket, Guid> ticketRepo,
			IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
			IRepository<InventoryBalance, Guid> balanceRepo,
			IRepository<InventoryTransaction, Guid> transactionRepo,
			IRepository<ProductBatch, Guid> batchRepo,
			IRepository<Bin, Guid> binRepo,
			IRepository<Warehouse, Guid> warehouseRepo)
		{
			_ticketRepo = ticketRepo;
			_ticketDetailRepo = ticketDetailRepo;
			_balanceRepo = balanceRepo;
			_transactionRepo = transactionRepo;
			_batchRepo = batchRepo;
			_binRepo = binRepo;
			_warehouseRepo = warehouseRepo;
		}

		// ==========================================
		// 1. TẠO & SỬA PHIẾU (MASTER)
		// ==========================================
		public async Task<InventoryTicket> CreateTicketAsync(TicketType type, Guid warehouseId, Guid? referenceDocumentId, string? note)
		{
			var warehouse = await _warehouseRepo.GetAsync(warehouseId);
			if (!warehouse.IsActive)
				throw new UserFriendlyException($"Kho '{warehouse.Name}' đang bị tạm khóa. Không thể tạo phiếu thao tác!");

			var draftCount = await _ticketRepo.CountAsync(x =>
				x.WarehouseId == warehouseId && x.Type == type && x.Status == ApprovalStatus.Draft);

			if (draftCount >= 10)
				throw new UserFriendlyException("Kho này đang có quá nhiều phiếu Nháp chưa được xử lý. Vui lòng duyệt hoặc xóa bớt!");

			string prefix = type.ToString().Substring(0, 3).ToUpper();
			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
			string randomSuffix = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
			string ticketNumber = $"{prefix}-{timestamp}-{randomSuffix}";

			return new InventoryTicket(GuidGenerator.Create(), ticketNumber, type, warehouseId, referenceDocumentId, note);
		}

		public void UpdateTicket(InventoryTicket ticket, string? note)
		{
			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể sửa Phiếu đã thực thi!");

			ticket.UpdateNote(note);
		}

		public async Task ValidateBeforeDeleteAsync(InventoryTicket ticket)
		{
			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể xóa Phiếu đã duyệt!");

			if (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward)
			{
				var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);
				foreach (var item in details)
				{
					var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId);
					if (balance != null)
					{
						balance.UnlockStock(item.Quantity);
						await _balanceRepo.UpdateAsync(balance);
					}
				}
			}
		}

		// ==========================================
		// 2. QUẢN LÝ CHI TIẾT PHIẾU (DETAIL)
		// ==========================================
		public async Task<InventoryTicketDetail> CreateTicketDetailAsync(InventoryTicket ticket, Guid productId, Guid productBatchId, Guid binId, decimal quantity)
		{
			if (ticket.Status == ApprovalStatus.Approved || ticket.Status == ApprovalStatus.Rejected)
				throw new UserFriendlyException("Không thể thêm chi tiết vào phiếu đã Duyệt hoặc Từ chối!");

			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng phải lớn hơn 0!");

			var bin = await _binRepo.GetAsync(binId);
			if (bin.WarehouseId != ticket.WarehouseId)
				throw new UserFriendlyException("Vị trí (Bin) không thuộc kho của phiếu này!");

			// Logic: Nếu cố tình thêm hàng vào phiếu xuất đang "Chờ duyệt", phải Lock hàng đó lại ngay
			if (ticket.Status == ApprovalStatus.Pending && (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward))
			{
				var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == binId && x.ProductBatchId == productBatchId);
				if (balance == null || balance.AvailableQuantity < quantity)
					throw new UserFriendlyException("Không đủ tồn kho khả dụng để thêm vào phiếu xuất!");

				balance.LockStock(quantity);
				await _balanceRepo.UpdateAsync(balance);
			}

			return new InventoryTicketDetail(GuidGenerator.Create(), ticket.Id, productId, productBatchId, binId, quantity);
		}

		public async Task UpdateDetailQuantityAsync(InventoryTicket ticket, InventoryTicketDetail detail, decimal actualQuantity)
		{
			if (actualQuantity < 0)
				throw new UserFriendlyException("Số lượng không được âm!");

			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể sửa chi tiết của Phiếu đã thực thi!");

			if (ticket.Status == ApprovalStatus.Pending && (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue))
			{
				var diff = detail.Quantity - actualQuantity;
				if (diff > 0)
				{
					var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == detail.BinId && x.ProductBatchId == detail.ProductBatchId);
					if (balance != null)
					{
						balance.UnlockStock(diff);
						await _balanceRepo.UpdateAsync(balance);
					}
				}
				else if (diff < 0)
				{
					throw new UserFriendlyException("Không thể tự ý tăng số lượng xuất lớn hơn chỉ định. Vui lòng thêm dòng mới!");
				}
			}

			detail.UpdateActualQuantity(actualQuantity);
		}

		public async Task RemoveTicketDetailAsync(InventoryTicket ticket, InventoryTicketDetail detail)
		{
			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể xóa chi tiết của Phiếu đã duyệt!");

			if (ticket.Status == ApprovalStatus.Pending &&
			   (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward))
			{
				var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == detail.BinId && x.ProductBatchId == detail.ProductBatchId);
				if (balance != null)
				{
					balance.UnlockStock(detail.Quantity);
					await _balanceRepo.UpdateAsync(balance);
				}
			}
		}

		// ==========================================
		// 3. QUY TRÌNH DUYỆT (APPROVE & REJECT)
		// ==========================================
		public async Task SendToApproveAsync(InventoryTicket ticket)
		{
			if (ticket.Status != ApprovalStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể gửi duyệt phiếu đang ở trạng thái Nháp (Draft)!");

			var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);
			if (!details.Any())
				throw new UserFriendlyException("Phiếu kho chưa có hàng hóa, không thể gửi duyệt!");

			// QUAN TRỌNG: Lock tồn kho nếu là phiếu xuất
			if (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward)
			{
				foreach (var item in details)
				{
					var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId);
					if (balance == null || balance.AvailableQuantity < item.Quantity)
					{
						throw new UserFriendlyException($"Không đủ tồn kho khả dụng cho hàng hóa ID {item.ProductId} tại vị trí đang chọn!");
					}
					balance.LockStock(item.Quantity);
					await _balanceRepo.UpdateAsync(balance);
				}
			}

			ticket.RequestApprove();
		}

		public async Task RejectTicketAsync(InventoryTicket ticket, string rejectReason)
		{
			if (ticket.Status != ApprovalStatus.Pending)
				throw new UserFriendlyException("Chỉ có thể từ chối phiếu đang chờ duyệt!");

			// Unlock hàng nếu là phiếu xuất
			if (ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue || ticket.Type == TicketType.ReturnOutward)
			{
				var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);
				foreach (var item in details)
				{
					var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId);
					if (balance != null)
					{
						balance.UnlockStock(item.Quantity);
						await _balanceRepo.UpdateAsync(balance);
					}
				}
			}

			ticket.Reject();
			ticket.UpdateNote($"[Từ chối: {rejectReason}] " + ticket.Note);
		}

		// ==========================================
		// 4. THỰC THI (EXECUTE)
		// ==========================================
		public async Task ExecuteTicketAsync(InventoryTicket ticket)
		{
			if (ticket.Status == ApprovalStatus.Approved) throw new UserFriendlyException("Phiếu này đã được duyệt và thực thi rồi!");
			if (ticket.Status == ApprovalStatus.Rejected) throw new UserFriendlyException("Phiếu này đã bị từ chối!");
			if (ticket.Status == ApprovalStatus.Draft) throw new UserFriendlyException("Phiếu đang ở bản nháp, vui lòng gửi duyệt (Pending) trước!");

			var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);
			InventoryTransactionType transType = MapTicketToTransaction(ticket.Type);

			foreach (var item in details)
			{
				var balance = await _balanceRepo.FirstOrDefaultAsync(x => x.WarehouseId == ticket.WarehouseId && x.BinId == item.BinId && x.ProductBatchId == item.ProductBatchId);
				bool isIncoming = ticket.Type == TicketType.GoodsReceipt || ticket.Type == TicketType.ReturnInward || ticket.Type == TicketType.RecallReceipt;

				if (isIncoming)
				{
					if (balance == null)
					{
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
		// 5. THUẬT TOÁN FEFO
		// ==========================================
		public async Task AllocateFEFOAsync(InventoryTicket ticket, Guid productId, decimal requiredQuantity)
		{
			decimal remaining = requiredQuantity;
			DateTime now = DateTime.Now;

			var balanceQuery = await _balanceRepo.GetQueryableAsync();
			var batchQuery = await _batchRepo.GetQueryableAsync();
			var binQuery = await _binRepo.GetQueryableAsync();

			var query = from bal in balanceQuery
						join bat in batchQuery on bal.ProductBatchId equals bat.Id
						join bin in binQuery on bal.BinId equals bin.Id
						where bal.WarehouseId == ticket.WarehouseId
						   && bal.ProductId == productId
						   && (bal.Quantity - bal.LockedQuantity) > 0
						   && bat.Status == BatchQAStatus.Approved
						   && !bin.IsBlocked
						   && bat.ExpiryDate > now
						orderby bat.ExpiryDate ascending, (bal.Quantity - bal.LockedQuantity) ascending
						select new { bal, bat, bin };

			var availableStocks = await AsyncExecuter.ToListAsync(query);

			foreach (var stock in availableStocks)
			{
				if (remaining <= 0) break;

				decimal availableQty = stock.bal.Quantity - stock.bal.LockedQuantity;
				decimal takeQty = Math.Min(availableQty, remaining);

				// Lock kho ngay lúc cấp phát FEFO
				stock.bal.LockStock(takeQty);
				await _balanceRepo.UpdateAsync(stock.bal);

				await _ticketDetailRepo.InsertAsync(new InventoryTicketDetail(
					GuidGenerator.Create(), ticket.Id, productId, stock.bat.Id, stock.bin.Id, takeQty));

				remaining -= takeQty;
			}

			if (remaining > 0)
				throw new UserFriendlyException($"Không đủ tồn kho ĐẠT CHUẨN để xuất! Còn thiếu {remaining:N0} đơn vị.");
		}
	}
}