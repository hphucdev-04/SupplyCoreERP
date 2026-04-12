using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Products;
using SupplyCoreERP.Suppliers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Orders.PO
{
	public class PurchaseOrderManager : DomainService
	{
		private readonly IRepository<PurchaseOrder, Guid> _orderRepo;
		private readonly IRepository<Supplier, Guid> _supplierRepo;
		private readonly IRepository<Product, Guid> _productRepo;
		private readonly TicketManager _ticketManager;

		public PurchaseOrderManager(
			IRepository<PurchaseOrder, Guid> orderRepo,
			IRepository<Supplier, Guid> supplierRepo,
			IRepository<Product, Guid> productRepo,
			TicketManager ticketManager)
		{
			_orderRepo = orderRepo;
			_supplierRepo = supplierRepo;
			_productRepo = productRepo;
			_ticketManager = ticketManager;
		}

		public async Task<PurchaseOrder> CreateOrderAsync(
			Guid supplierId, Guid warehouseId, DateTime orderDate,
			DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note)
		{
			var supplier = await _supplierRepo.GetAsync(supplierId);
			if (!supplier.IsActive)
				throw new UserFriendlyException($"Nhà cung cấp '{supplier.Name}' đang bị khóa!");

			string code = $"PO-{DateTime.Now:yyyyMMdd}-{GuidGenerator.Create().ToString()[..4].ToUpper()}";

			DateTime? finalDueDate = inputDueDate
				?? (supplier.PaymentTermDays > 0 ? orderDate.AddDays(supplier.PaymentTermDays) : null);

			return new PurchaseOrder(GuidGenerator.Create(), code, supplierId, warehouseId,
									 orderDate, expectedDeliveryDate, finalDueDate, note);
		}

		public Task UpdateOrderAsync(PurchaseOrder order, Guid warehouseId,
			DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
		{
			order.UpdateMaster(warehouseId, expectedDeliveryDate, dueDate, note);
			return Task.CompletedTask;
		}

		public Task CheckBeforeDeleteAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể xóa đơn hàng đang ở trạng thái Nháp!");
			return Task.CompletedTask;
		}

		public async Task AddDetailAsync(PurchaseOrder order, Guid productId, Guid unitId,
			int conversionFactor, decimal quantity, decimal unitPrice, decimal taxRate)
		{
			var product = await _productRepo.GetAsync(productId);
			if (!product.IsAvailableForInventory)
				throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa đủ điều kiện giao dịch!");

			order.AddDetail(GuidGenerator.Create(), productId, unitId, conversionFactor, quantity, unitPrice, taxRate);
		}

		public Task UpdateDetailAsync(PurchaseOrder order, Guid detailId,
			decimal quantity, decimal unitPrice, decimal taxRate)
		{
			order.UpdateDetail(detailId, quantity, unitPrice, taxRate);
			return Task.CompletedTask;
		}

		public Task RemoveDetailAsync(PurchaseOrder order, Guid detailId)
		{
			order.RemoveDetail(detailId);
			return Task.CompletedTask;
		}

		public Task SendToApproveAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể gửi duyệt đơn đang ở trạng thái Nháp!");
			order.SendToApprove();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Validate nghiệp vụ và tạo phiếu nhập kho chờ.
		/// </summary>
		/// <returns>InventoryTicket mới tạo — chưa được lưu DB. AppService gọi InsertAsync.</returns>
		public async Task<InventoryTicket> ApproveAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.PendingApproval)
				throw new UserFriendlyException("Đơn hàng chưa được gửi duyệt!");

			var supplier = await _supplierRepo.GetAsync(order.SupplierId);

			if (supplier.DebtLimit > 0 && (supplier.CurrentDebt + order.TotalAmount) > supplier.DebtLimit)
				throw new UserFriendlyException(
					$"Từ chối duyệt! Đơn ({order.TotalAmount:N0}đ) sẽ vượt trần nợ với '{supplier.Name}'.");

			var today = DateTime.Now.Date;
			bool hasOverdue = await _orderRepo.AnyAsync(x =>
				x.SupplierId == order.SupplierId &&
				x.Status == PurchaseOrderStatus.Completed &&
				x.DueDate.HasValue && x.DueDate.Value.Date < today);

			if (hasOverdue)
				throw new UserFriendlyException($"'{supplier.Name}' đang có khoản nợ quá hạn. Xử lý nợ cũ trước!");

	
			var productIds = order.Details.Select(x => x.ProductId).Distinct().ToArray();
			var products = await _productRepo.GetListAsync(x => productIds.Contains(x.Id));

			foreach (var item in order.Details)
			{
				var product = products.FirstOrDefault(p => p.Id == item.ProductId);
				if (product == null || !product.IsAvailableForInventory)
					throw new UserFriendlyException($"Sản phẩm '{product?.Name}' không còn đủ điều kiện giao dịch!");
			}

			// Tạo phiếu nhập kho — KHÔNG InsertAsync tại đây
			var receiptTicket = await _ticketManager.CreateTicketAsync(
				TicketType.GoodsReceipt, order.WarehouseId, order.Id, order.Code,
				$"Phiếu chờ nhập kho cho Đơn mua hàng {order.Code}");

			order.Approve();

			return receiptTicket;
		}

		/// <summary>
		/// Hoàn tất đơn mua và cộng công nợ cho nhà cung cấp.
		/// </summary>
		/// <returns>Supplier đã AddDebt — chưa được lưu DB. AppService gọi UpdateAsync.</returns>
		public async Task<Supplier> CompleteAsync(PurchaseOrder order)
		{
			if (!await _ticketManager.HasStatusAsync(order.Id, ApprovalStatus.Approved))
				throw new UserFriendlyException("Phiếu nhập kho chưa được thực thi! Cần hoàn tất nhập kho trước.");

			if (order.Status != PurchaseOrderStatus.Receiving && order.Status != PurchaseOrderStatus.Approved)
				throw new UserFriendlyException("Chỉ có thể Hoàn tất khi đang Nhập kho hoặc Đã duyệt!");

			order.Complete();

			var supplier = await _supplierRepo.GetAsync(order.SupplierId);
			supplier.AddDebt(order.TotalAmount);

			return supplier; 
		}

		public Task CancelAsync(PurchaseOrder order, string cancelReason)
		{
			order.Cancel();
			order.UpdateMaster(order.WarehouseId, order.ExpectedDeliveryDate, order.DueDate,
							   $"[Đã hủy: {cancelReason}] " + (order.Note ?? ""));
			return Task.CompletedTask;
		}
	}
}