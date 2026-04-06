using SupplyCoreERP.Enums.Orders;
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

		public async Task<PurchaseOrder> CreateOrderAsync(Guid supplierId, Guid warehouseId, DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note)
		{
			var supplier = await _supplierRepo.GetAsync(supplierId);
			if (!supplier.IsActive)
				throw new UserFriendlyException($"Nhà cung cấp '{supplier.Name}' đang bị khóa!");

			string code = $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

			DateTime? finalDueDate = inputDueDate;
			if (!finalDueDate.HasValue && supplier.PaymentTermDays > 0)
			{
				finalDueDate = orderDate.AddDays(supplier.PaymentTermDays);
			}

			return new PurchaseOrder(GuidGenerator.Create(), code, supplierId, warehouseId, orderDate, expectedDeliveryDate, finalDueDate, note);
		}

		public async Task UpdateOrderAsync(PurchaseOrder order, Guid warehouseId, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
		{
			order.UpdateMaster(warehouseId, expectedDeliveryDate, dueDate, note);
		}

		public async Task CheckBeforeDeleteAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể xóa đơn hàng đang ở trạng thái Nháp!");
		}

		public async Task AddDetailAsync(PurchaseOrder order, Guid productId, Guid unitId, int conversionFactor, decimal quantity, decimal unitPrice, decimal taxRate)
		{
			var product = await _productRepo.GetAsync(productId);
			if (!product.IsAvailableForInventory)
				throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa đủ điều kiện giao dịch!");

			order.AddDetail(GuidGenerator.Create(), productId, unitId, conversionFactor, quantity, unitPrice, taxRate);
		}

		public async Task UpdateDetailAsync(PurchaseOrder order, Guid detailId, decimal quantity, decimal unitPrice, decimal taxRate)
		{
			order.UpdateDetail(detailId, quantity, unitPrice, taxRate);
		}

		public async Task RemoveDetailAsync(PurchaseOrder order, Guid detailId)
		{
			order.RemoveDetail(detailId);
		}

		public async Task SendToApproveAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể gửi duyệt đơn đang ở trạng thái Nháp!");

			order.SendToApprove();
		}

		public async Task ApproveAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.PendingApproval)
				throw new UserFriendlyException("Đơn hàng chưa được gửi duyệt!");

			var supplier = await _supplierRepo.GetAsync(order.SupplierId);

			if (supplier.DebtLimit > 0 && (supplier.CurrentDebt + order.TotalAmount) > supplier.DebtLimit)
			{
				throw new UserFriendlyException($"Từ chối duyệt! Đơn hàng này ({order.TotalAmount:N0}đ) sẽ làm vượt quá trần nợ cho phép với Nhà cung cấp '{supplier.Name}'.");
			}

			var today = DateTime.Now.Date;
			bool hasOverdueOrders = await _orderRepo.AnyAsync(x =>
				x.SupplierId == order.SupplierId &&
				x.Status == PurchaseOrderStatus.Completed &&
				x.DueDate.HasValue && x.DueDate.Value.Date < today);

			if (hasOverdueOrders)
				throw new UserFriendlyException($"Nhà cung cấp '{supplier.Name}' đang có khoản nợ quá hạn chưa thanh toán. Cần xử lý nợ cũ trước!");

			Guid[] productIds = order.Details.Select(x => x.ProductId).Distinct().ToArray();
			var products = await _productRepo.GetListAsync(x => productIds.Contains(x.Id));

			foreach (var item in order.Details)
			{
				var product = products.FirstOrDefault(p => p.Id == item.ProductId);
				if (product == null || !product.IsAvailableForInventory)
					throw new UserFriendlyException($"Sản phẩm '{product?.Name}' không còn đủ điều kiện giao dịch!");
			}

			// TẠO PHIẾU NHẬP KHO CHỜ 
			string note = $"Phiếu chờ nhập kho cho Đơn mua hàng {order.Code}";
			var receiptTicket = await _ticketManager.CreateTicketAsync(
				SupplyCoreERP.Enums.Warehouses.TicketType.GoodsReceipt,
				order.WarehouseId,
				order.Id,
				order.Code,
				note
			);

			order.Approve();
		}

		public async Task CompleteAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.Receiving && order.Status != PurchaseOrderStatus.Approved)
				throw new UserFriendlyException("Chỉ có thể Hoàn tất đơn hàng khi đang Nhập kho hoặc Đã duyệt!");

			order.Complete();

			var supplier = await _supplierRepo.GetAsync(order.SupplierId);
			supplier.AddDebt(order.TotalAmount);
		}

		public async Task CancelAsync(PurchaseOrder order, string cancelReason)
		{
			order.Cancel();
			order.UpdateMaster(order.WarehouseId, order.ExpectedDeliveryDate, order.DueDate, $"[Đã hủy: {cancelReason}] " + (order.Note ?? ""));
		}
	}
}