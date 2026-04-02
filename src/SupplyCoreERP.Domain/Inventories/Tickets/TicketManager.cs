using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Transactions;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Products;
using SupplyCoreERP.Warehouses;
using System;
using System.Collections.Generic;
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
		private readonly IRepository<Product, Guid> _productRepo;
		private readonly WarehouseManager _warehouseManager;

		public TicketManager(
			IRepository<InventoryTicket, Guid> ticketRepo,
			IRepository<InventoryTicketDetail, Guid> ticketDetailRepo,
			IRepository<InventoryBalance, Guid> balanceRepo,
			IRepository<InventoryTransaction, Guid> transactionRepo,
			IRepository<ProductBatch, Guid> batchRepo,
			IRepository<Bin, Guid> binRepo,
			IRepository<Warehouse, Guid> warehouseRepo,
			IRepository<Product, Guid> productRepo,
			WarehouseManager warehouseManager)
		{
			_ticketRepo = ticketRepo;
			_ticketDetailRepo = ticketDetailRepo;
			_balanceRepo = balanceRepo;
			_transactionRepo = transactionRepo;
			_batchRepo = batchRepo;
			_binRepo = binRepo;
			_warehouseRepo = warehouseRepo;
			_productRepo = productRepo;
			_warehouseManager = warehouseManager;
		}
		#region Helpers
		private bool IsIssueTicket(TicketType type) =>
			type == TicketType.GoodsIssue ||
			type == TicketType.DisposalIssue ||
			type == TicketType.ReturnOutward;

		private bool IsIncomingTicket(TicketType type) =>
			type == TicketType.GoodsReceipt ||
			type == TicketType.ReturnInward ||
			type == TicketType.RecallReceipt;

		private async Task ValidateBinForIncomingAsync(Guid binId, Guid productId, Guid productBatchId)
		{
			var binQuery = await _binRepo.WithDetailsAsync(b => b.Zone);
			var bin = await AsyncExecuter.FirstOrDefaultAsync(binQuery.Where(b => b.Id == binId));

			if (bin == null)
				throw new UserFriendlyException("Không tìm thấy vị trí (Bin)!");

			var product = await _productRepo.GetAsync(productId);

			_warehouseManager.ValidateStorageCompatibility(bin, product.RequiredStorageCondition);

			await ValidateBinSKUCapacityAsync(bin, productId, productBatchId);
		}

		/// <summary>
		/// Validate một lô duy nhất (dùng cho các luồng tạo/cập nhật detail đơn lẻ).
		/// Phiếu nhập (RecallReceipt) không cần check QA vì đây là nhập hàng thu hồi về.
		/// </summary>
		private async Task ValidateBatchForIssueAsync(Guid productBatchId)
		{
			var batch = await _batchRepo.GetAsync(productBatchId);
			ValidateBatchForIssue(batch);
		}

		/// <summary>
		/// Validate in-memory — dùng sau khi đã batch-load batches.
		/// </summary>
		private void ValidateBatchForIssue(ProductBatch batch)
		{
			if (batch.Status != BatchQAStatus.Approved)
				throw new UserFriendlyException(
					$"Lô hàng '{batch.BatchNumber}' chưa được QA duyệt hoặc đã bị thu hồi/hết hạn. " +
					$"Trạng thái hiện tại: {batch.Status}. Không thể thực hiện xuất kho!");

			if (batch.ExpiryDate <= DateTime.Now)
				throw new UserFriendlyException(
					$"Lô hàng '{batch.BatchNumber}' đã hết hạn sử dụng ({batch.ExpiryDate:dd/MM/yyyy}). Không thể xuất kho!");
		}

		/// <summary>
		/// Validate sản phẩm có đủ điều kiện nhập/xuất kho không.
		/// </summary>
		private async Task ValidateProductForInventoryAsync(Guid productId)
		{
			var product = await _productRepo.GetAsync(productId);

			if (!product.IsAvailableForInventory)
				throw new UserFriendlyException(
					$"Sản phẩm '{product.Name}' ({product.Code}) chưa được duyệt hoặc đang bị tạm ngưng. " +
					"Không thể thực hiện nhập/xuất kho!");
		}

		/// <summary>
		/// Kiểm tra bin còn chỗ cho SKU+Lô này không.
		/// Chỉ áp dụng cho phiếu NHẬP — phiếu xuất không tạo slot mới.
		/// </summary>
		private async Task ValidateBinSKUCapacityAsync(Bin bin, Guid productId, Guid productBatchId)
		{
			int usedSKUCount = await _balanceRepo.CountAsync(
				b => b.BinId == bin.Id && b.Quantity > 0);

			bool isNewSKU = !await _balanceRepo.AnyAsync(
				b => b.BinId == bin.Id
				  && b.ProductId == productId
				  && b.ProductBatchId == productBatchId);

			bin.ValidateSKUCapacity(usedSKUCount, isNewSKU);
		}

		/// <summary>
		/// Batch-load tất cả InventoryBalance liên quan đến một danh sách details.
		/// Trả về Dictionary keyed bởi (BinId, ProductBatchId) để lookup O(1).
		/// Lưu ý: query dùng IN clause trên hai tập riêng (binIds, batchIds),
		/// kết quả có thể rộng hơn tập cần thiết nhưng luôn nhỏ và được lọc in-memory.
		/// </summary>
		private async Task<Dictionary<(Guid BinId, Guid BatchId), InventoryBalance>> LoadBalanceMapAsync(
			IList<InventoryTicketDetail> details,
			Guid? warehouseId = null)
		{
			var binIds = details.Select(d => d.BinId).Distinct().ToList();
			var batchIds = details.Select(d => d.ProductBatchId).Distinct().ToList();

			var balances = warehouseId.HasValue
				? await _balanceRepo.GetListAsync(x =>
					x.WarehouseId == warehouseId.Value &&
					binIds.Contains(x.BinId) &&
					batchIds.Contains(x.ProductBatchId))
				: await _balanceRepo.GetListAsync(x =>
					binIds.Contains(x.BinId) &&
					batchIds.Contains(x.ProductBatchId));

			return balances.ToDictionary(b => (b.BinId, b.ProductBatchId));
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
		#endregion


		#region Ticket
		public async Task<InventoryTicket> CreateTicketAsync(
			TicketType type,
			Guid warehouseId,
			Guid? referenceDocumentId,
			string? referenceDocumentNumber,
			string? note)
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

			return new InventoryTicket(GuidGenerator.Create(), ticketNumber, type, warehouseId,
				referenceDocumentId, referenceDocumentNumber, note);
		}

		public void UpdateTicket(InventoryTicket ticket, string? note)
		{
			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể sửa Phiếu đã thực thi!");

			ticket.UpdateNote(note);
		}

		/// <summary>
		/// FIX N+1: thay vì foreach → GetBalance → UpdateAsync,
		/// nay batch-load tất cả balances → xử lý in-memory → UpdateManyAsync.
		/// </summary>
		public async Task ValidateBeforeDeleteAsync(InventoryTicket ticket)
		{
			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể xóa Phiếu đã duyệt!");

			if (IsIssueTicket(ticket.Type))
			{
				var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);

				// FIX: 1 query thay vì N queries
				var balanceMap = await LoadBalanceMapAsync(details);

				foreach (var item in details)
				{
					if (balanceMap.TryGetValue((item.BinId, item.ProductBatchId), out var balance))
						balance.UnlockStock(item.BaseQuantity);
				}

				// FIX: 1 batch write thay vì N writes
				var modified = balanceMap.Values.ToList();
				if (modified.Any())
					await _balanceRepo.UpdateManyAsync(modified);
			}
		}
		#endregion

		#region Ticket Detail
		/// <param name="unitId">Đơn vị người dùng chọn (Viên, Vỉ, Hộp...)</param>
		/// <param name="conversionFactor">
		///     Tỉ lệ quy đổi về BaseUnit, snapshot tại thời điểm tạo.
		///     Truyền 1 nếu unitId chính là BaseUnit của sản phẩm.
		/// </param>
		public async Task<InventoryTicketDetail> CreateTicketDetailAsync(
			InventoryTicket ticket,
			Guid productId,
			Guid productBatchId,
			Guid binId,
			Guid unitId,
			int conversionFactor,
			decimal quantity)
		{
			if (ticket.Status == ApprovalStatus.Approved || ticket.Status == ApprovalStatus.Rejected)
				throw new UserFriendlyException("Không thể thêm chi tiết vào phiếu đã Duyệt hoặc Từ chối!");

			if (quantity <= 0)
				throw new UserFriendlyException("Số lượng phải lớn hơn 0!");

			if (conversionFactor <= 0)
				throw new UserFriendlyException("Tỉ lệ quy đổi không hợp lệ!");

			await ValidateProductForInventoryAsync(productId);

			var bin = await _binRepo.GetAsync(binId);
			if (bin.WarehouseId != ticket.WarehouseId)
				throw new UserFriendlyException("Vị trí (Bin) không thuộc kho của phiếu này!");

			if (IsIncomingTicket(ticket.Type))
				await ValidateBinForIncomingAsync(binId, productId, productBatchId);

			if (IsIssueTicket(ticket.Type))
				await ValidateBatchForIssueAsync(productBatchId);

			decimal baseQty = quantity * conversionFactor;

			// Nếu phiếu xuất đang "Chờ duyệt" → lock ngay
			if (ticket.Status == ApprovalStatus.Pending && IsIssueTicket(ticket.Type))
			{
				var balance = await _balanceRepo.FirstOrDefaultAsync(x =>
					x.BinId == binId && x.ProductBatchId == productBatchId);

				if (balance == null || balance.AvailableQuantity < baseQty)
					throw new UserFriendlyException("Không đủ tồn kho khả dụng để thêm vào phiếu xuất!");

				balance.LockStock(baseQty);
				await _balanceRepo.UpdateAsync(balance);
			}

			return new InventoryTicketDetail(
				GuidGenerator.Create(), ticket.Id, productId, productBatchId,
				binId, unitId, conversionFactor, quantity);
		}

		public async Task UpdateDetailQuantityAsync(
			InventoryTicket ticket,
			InventoryTicketDetail detail,
			decimal actualQuantity)
		{
			if (actualQuantity < 0)
				throw new UserFriendlyException("Số lượng không được âm!");

			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Không thể sửa chi tiết của Phiếu đã thực thi!");

			if (ticket.Status == ApprovalStatus.Pending &&
				(ticket.Type == TicketType.GoodsIssue || ticket.Type == TicketType.DisposalIssue))
			{
				decimal newBaseQty = actualQuantity * detail.ConversionFactor;
				decimal diff = detail.BaseQuantity - newBaseQty;

				if (diff > 0)
				{
					// Giảm số lượng → unlock phần dôi
					var balance = await _balanceRepo.FirstOrDefaultAsync(x =>
						x.BinId == detail.BinId && x.ProductBatchId == detail.ProductBatchId);
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

			if (ticket.Status == ApprovalStatus.Pending && IsIssueTicket(ticket.Type))
			{
				var balance = await _balanceRepo.FirstOrDefaultAsync(x =>
					x.BinId == detail.BinId && x.ProductBatchId == detail.ProductBatchId);
				if (balance != null)
				{
					balance.UnlockStock(detail.BaseQuantity);
					await _balanceRepo.UpdateAsync(balance);
				}
			}
		}
		#endregion

		#region Ticket Workflow
		/// <summary>
		/// FIX N+1:
		///   TRƯỚC: foreach detail → ValidateBatch (1 query/item) → GetBalance (1 query/item) → UpdateAsync (1 write/item)
		///   SAU  : GetBatches (1 query) + GetBalances (1 query) → validate/lock in-memory → UpdateManyAsync (1 write)
		/// </summary>
		public async Task SendToApproveAsync(InventoryTicket ticket)
		{
			if (ticket.Status != ApprovalStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể gửi duyệt phiếu đang ở trạng thái Nháp (Draft)!");

			var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);
			if (!details.Any())
				throw new UserFriendlyException("Phiếu kho chưa có hàng hóa, không thể gửi duyệt!");

			if (IsIssueTicket(ticket.Type))
			{
				// FIX: batch-load tất cả batches liên quan (1 query)
				var batchIds = details.Select(d => d.ProductBatchId).Distinct().ToList();
				var batches = await _batchRepo.GetListAsync(x => batchIds.Contains(x.Id));
				var batchMap = batches.ToDictionary(b => b.Id);

				// Validate tất cả lô in-memory (0 thêm queries)
				foreach (var item in details)
					ValidateBatchForIssue(batchMap[item.ProductBatchId]);

				// FIX: batch-load tất cả balances liên quan (1 query)
				var balanceMap = await LoadBalanceMapAsync(details);

				foreach (var item in details)
				{
					if (!balanceMap.TryGetValue((item.BinId, item.ProductBatchId), out var balance)
						|| balance.AvailableQuantity < item.BaseQuantity)
						throw new UserFriendlyException(
							$"Không đủ tồn kho khả dụng cho sản phẩm ID {item.ProductId} tại vị trí đang chọn!");

					balance.LockStock(item.BaseQuantity);
				}

				// FIX: 1 batch write thay vì N writes
				var locked = balanceMap.Values.ToList();
				if (locked.Any())
					await _balanceRepo.UpdateManyAsync(locked);
			}

			ticket.RequestApprove();
		}

		/// <summary>
		/// FIX N+1:
		///   TRƯỚC: foreach detail → GetBalance (1 query/item) → UpdateAsync (1 write/item)
		///   SAU  : GetBalances (1 query) → unlock in-memory → UpdateManyAsync (1 write)
		/// </summary>
		public async Task RejectTicketAsync(InventoryTicket ticket, string rejectReason)
		{
			if (ticket.Status != ApprovalStatus.Pending)
				throw new UserFriendlyException("Chỉ có thể từ chối phiếu đang chờ duyệt!");

			if (IsIssueTicket(ticket.Type))
			{
				var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);

				// FIX: 1 query thay vì N queries
				var balanceMap = await LoadBalanceMapAsync(details);

				foreach (var item in details)
				{
					if (balanceMap.TryGetValue((item.BinId, item.ProductBatchId), out var balance))
						balance.UnlockStock(item.BaseQuantity);
				}

				// FIX: 1 batch write thay vì N writes
				var modified = balanceMap.Values.ToList();
				if (modified.Any())
					await _balanceRepo.UpdateManyAsync(modified);
			}

			ticket.Reject();
			ticket.UpdateNote($"[Từ chối: {rejectReason}] " + ticket.Note);
		}

		/// <summary>
		/// FIX N+1:
		///   TRƯỚC: foreach detail → ValidateBatch (1 query) → GetBalance (1 query)
		///          → UpdateAsync/InsertAsync (1 write) → InsertTransaction (1 write)
		///   SAU  : GetBatches (1 query) + GetBalances (1 query) → xử lý in-memory
		///          → InsertManyAsync balances + UpdateManyAsync balances + InsertManyAsync transactions
		/// </summary>
		public async Task ExecuteTicketAsync(InventoryTicket ticket)
		{
			if (ticket.Status == ApprovalStatus.Approved)
				throw new UserFriendlyException("Phiếu này đã được duyệt và thực thi rồi!");
			if (ticket.Status == ApprovalStatus.Rejected)
				throw new UserFriendlyException("Phiếu này đã bị từ chối!");
			if (ticket.Status == ApprovalStatus.Draft)
				throw new UserFriendlyException("Phiếu đang ở bản nháp, vui lòng gửi duyệt (Pending) trước!");

			var details = await _ticketDetailRepo.GetListAsync(x => x.TicketId == ticket.Id);
			InventoryTransactionType transType = MapTicketToTransaction(ticket.Type);

			var batchIds = details.Select(d => d.ProductBatchId).Distinct().ToList();
			var binIds = details.Select(d => d.BinId).Distinct().ToList();

			// FIX: batch-load batches (1 query) thay vì N queries trong vòng lặp
			Dictionary<Guid, ProductBatch> batchMap = null;
			if (IsIssueTicket(ticket.Type))
			{
				var batches = await _batchRepo.GetListAsync(x => batchIds.Contains(x.Id));
				batchMap = batches.ToDictionary(b => b.Id);

				// Final gate: validate tất cả lô in-memory (0 thêm queries)
				foreach (var batch in batches)
					ValidateBatchForIssue(batch);
			}

			// FIX: batch-load balances (1 query) thay vì N queries trong vòng lặp
			var balanceMap = await LoadBalanceMapAsync(details, ticket.WarehouseId);

			var newBalances = new List<InventoryBalance>();
			var transactions = new List<InventoryTransaction>();

			foreach (var item in details)
			{
				balanceMap.TryGetValue((item.BinId, item.ProductBatchId), out var balance);

				if (IsIncomingTicket(ticket.Type))
				{
					if (balance == null)
					{
						// Lần đầu nhập SKU+Lô vào bin → kiểm tra sức chứa và storage compatibility
						// (phòng trường hợp có phiếu khác đã nhập vào bin giữa lúc tạo phiếu và lúc execute)
						await ValidateBinForIncomingAsync(item.BinId, item.ProductId, item.ProductBatchId);

						balance = new InventoryBalance(
							GuidGenerator.Create(),
							ticket.WarehouseId, item.BinId,
							item.ProductId, item.ProductBatchId,
							item.BaseQuantity);

						newBalances.Add(balance);
						// Cập nhật map để phòng detail trùng bin+batch trong cùng phiếu
						balanceMap[(item.BinId, item.ProductBatchId)] = balance;
					}
					else
					{
						// Balance đã tồn tại → chỉ cộng thêm, không tốn slot mới
						balance.AddStock(item.BaseQuantity);
					}
				}
				else
				{
					if (balance == null)
						throw new UserFriendlyException(
							$"Không có tồn kho cho sản phẩm: {item.ProductId} tại vị trí này!");

					balance.UnlockStock(item.BaseQuantity);
					balance.RemoveStock(item.BaseQuantity);
				}

				// Transaction ghi BaseQuantity để số liệu nhất quán với InventoryBalance
				transactions.Add(new InventoryTransaction(
					GuidGenerator.Create(),
					ticket.WarehouseId, item.BinId, item.ProductId, item.ProductBatchId,
					transType,
					item.BaseQuantity,
					balance.Quantity,
					ticket.Id, ticket.TicketNumber, ticket.Note));
			}

			// FIX: batch write thay vì N writes riêng lẻ
			if (newBalances.Any())
				await _balanceRepo.InsertManyAsync(newBalances);

			// Chỉ update các balance đã tồn tại (không update newBalances vừa insert)
			var updatedBalances = balanceMap.Values
				.Where(b => !newBalances.Contains(b))
				.ToList();
			if (updatedBalances.Any())
				await _balanceRepo.UpdateManyAsync(updatedBalances);

			if (transactions.Any())
				await _transactionRepo.InsertManyAsync(transactions);

			ticket.Execute();
		}
		#endregion

		#region FEFO
		/// <summary>
		/// Tự động cấp phát hàng theo thuật toán FEFO (First Expired First Out).
		/// FEFO luôn làm việc với BaseUnit — InventoryBalance.Quantity là BaseUnit.
		/// FIX N+1: query join vẫn giữ nguyên (đã tốt).
		/// Phần writes được gom thành UpdateManyAsync + InsertManyAsync thay vì N lần riêng lẻ.
		/// </summary>
		/// <param name="requiredBaseQuantity">
		///     Số lượng cần xuất, ĐÃ quy về BaseUnit.
		///     Caller chịu trách nhiệm quy đổi trước khi gọi hàm này.
		/// </param>
		public async Task AllocateFEFOAsync(InventoryTicket ticket, Guid productId, decimal requiredBaseQuantity)
		{
			var product = await _productRepo.GetAsync(productId);

			if (!product.IsAvailableForInventory)
				throw new UserFriendlyException(
					$"Sản phẩm '{product.Name}' chưa được duyệt hoặc đang bị tạm ngưng. Không thể cấp phát FEFO!");

			decimal remaining = requiredBaseQuantity;
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
						orderby bat.ExpiryDate ascending,
								(bal.Quantity - bal.LockedQuantity) ascending
						select new { bal, bat, bin };

			var availableStocks = await AsyncExecuter.ToListAsync(query);

			var balancesToUpdate = new List<InventoryBalance>();
			var detailsToInsert = new List<InventoryTicketDetail>();

			foreach (var stock in availableStocks)
			{
				if (remaining <= 0) break;

				decimal availableQty = stock.bal.Quantity - stock.bal.LockedQuantity;
				decimal takeQty = Math.Min(availableQty, remaining);

				if (ticket.Status == ApprovalStatus.Pending)
				{
					stock.bal.LockStock(takeQty);
					balancesToUpdate.Add(stock.bal);
				}

				// FEFO tạo detail với ConversionFactor=1 vì takeQty đã là BaseUnit
				detailsToInsert.Add(new InventoryTicketDetail(
					GuidGenerator.Create(),
					ticket.Id,
					productId,
					stock.bat.Id,
					stock.bin.Id,
					unitId: product.BaseUnitId,
					conversionFactor: 1,
					qty: takeQty));

				remaining -= takeQty;
			}

			if (remaining > 0)
				throw new UserFriendlyException(
					$"Không đủ tồn kho ĐẠT CHUẨN để xuất! Còn thiếu {remaining:N0} {product.BaseUnit?.Name ?? "đơn vị"}.");

			if (balancesToUpdate.Any())
				await _balanceRepo.UpdateManyAsync(balancesToUpdate);

			if (detailsToInsert.Any())
				await _ticketDetailRepo.InsertManyAsync(detailsToInsert);
		}
		#endregion
	}
}