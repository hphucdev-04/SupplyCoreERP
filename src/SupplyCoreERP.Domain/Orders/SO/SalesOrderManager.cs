using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Prices;
using SupplyCoreERP.Products;
using SupplyCoreERP.Customers;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Tickets;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Sales.Orders
{
	public class SalesOrderManager : DomainService
	{
		private readonly IRepository<SalesOrder, Guid> _orderRepo;
		private readonly IRepository<Customer, Guid> _customerRepo;
		private readonly IRepository<Product, Guid> _productRepo;
		private readonly PriceManager _priceManager;
		private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
		private readonly TicketManager _ticketManager;

		public SalesOrderManager(
			IRepository<SalesOrder, Guid> orderRepo,
			IRepository<Customer, Guid> customerRepo,
			IRepository<Product, Guid> productRepo,
			PriceManager priceManager,
			IRepository<InventoryBalance, Guid> balanceRepo,
			TicketManager ticketManager)
		{
			_orderRepo = orderRepo;
			_customerRepo = customerRepo;
			_productRepo = productRepo;
			_priceManager = priceManager;
			_balanceRepo = balanceRepo;
			_ticketManager = ticketManager;
		}

		public async Task<SalesOrder> CreateOrderAsync(Guid customerId, Guid warehouseId, DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note)
		{
			var customer = await _customerRepo.GetAsync(customerId);
			if (!customer.IsActive)
				throw new UserFriendlyException($"Khách hàng '{customer.Name}' đang bị khóa, không thể lên đơn!");

			string code = $"SO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

			DateTime? finalDueDate = inputDueDate;
			if (!finalDueDate.HasValue && customer.PaymentTermDays > 0)
			{
				finalDueDate = orderDate.AddDays(customer.PaymentTermDays);
			}

			return new SalesOrder(GuidGenerator.Create(), code, customerId, warehouseId, orderDate, expectedDeliveryDate, finalDueDate, note);
		}

		public async Task UpdateOrderAsync(SalesOrder order, Guid warehouseId, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
		{
			order.UpdateMaster(warehouseId, expectedDeliveryDate, dueDate, note);
		}

		public async Task CheckBeforeDeleteAsync(SalesOrder order)
		{
			if (order.Status != SalesOrderStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể xóa đơn bán đang ở trạng thái Nháp!");
		}

		public async Task AddDetailAsync(SalesOrder order, Guid productId, Guid unitId, int conversionFactor, decimal quantity, decimal discountRate, decimal taxRate)
		{
			var product = await _productRepo.GetAsync(productId);
			if (!product.IsAvailableForInventory)
				throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa đủ điều kiện giao dịch!");

			var customer = await _customerRepo.GetAsync(order.CustomerId);

			decimal officialUnitPrice = await _priceManager.GetOfficialPriceAsync(customer.PriceListId, productId, unitId, quantity);

			order.AddDetail(GuidGenerator.Create(), productId, unitId, conversionFactor, quantity, officialUnitPrice, discountRate, taxRate);
		}

		public async Task UpdateDetailAsync(SalesOrder order, Guid detailId, decimal quantity, decimal discountRate, decimal taxRate)
		{
			var detail = order.Details.FirstOrDefault(x => x.Id == detailId);
			if (detail == null) throw new UserFriendlyException("Không tìm thấy dòng chi tiết");

			var customer = await _customerRepo.GetAsync(order.CustomerId);
			decimal newOfficialUnitPrice = await _priceManager.GetOfficialPriceAsync(customer.PriceListId, detail.ProductId, detail.UnitId, quantity);

			order.UpdateDetail(detailId, quantity, newOfficialUnitPrice, discountRate, taxRate);
		}

		public async Task RemoveDetailAsync(SalesOrder order, Guid detailId) => order.RemoveDetail(detailId);

		public async Task SendToApproveAsync(SalesOrder order) => order.SendToApprove();

		public async Task ApproveAsync(SalesOrder order)
		{
			if (order.Status != SalesOrderStatus.PendingApproval)
				throw new UserFriendlyException("Đơn hàng chưa được gửi duyệt!");

			if (!order.Details.Any())
				throw new UserFriendlyException("Đơn hàng chưa có sản phẩm, không thể duyệt!");

			var customer = await _customerRepo.GetAsync(order.CustomerId);

			if (customer.DebtLimit > 0 && (customer.CurrentDebt + order.TotalAmount) > customer.DebtLimit)
				throw new UserFriendlyException($"Từ chối duyệt! Khách '{customer.Name}' sẽ bị vượt hạn mức nợ.");

			var today = DateTime.Now.Date;
			bool hasOverdueOrders = await _orderRepo.AnyAsync(x =>
				x.CustomerId == order.CustomerId &&
				x.Status == SalesOrderStatus.Completed &&
				x.DueDate.HasValue && x.DueDate.Value.Date < today);

			if (hasOverdueOrders)
				throw new UserFriendlyException("Khách hàng đang có đơn hàng cũ quá hạn. Vui lòng thu hồi nợ trước!");

			var productIds = order.Details.Select(x => x.ProductId).Distinct().ToList();
			var balances = await _balanceRepo.GetListAsync(x => productIds.Contains(x.ProductId) && x.WarehouseId == order.WarehouseId);

			foreach (var item in order.Details)
			{
				decimal totalAvailable = balances.Where(x => x.ProductId == item.ProductId).Sum(x => x.AvailableQuantity);

				if (totalAvailable < item.BaseQuantity)
				{
					var product = await _productRepo.GetAsync(item.ProductId);
					throw new UserFriendlyException($"Sản phẩm '{product.Name}' không đủ tồn kho tại kho này! Cần: {item.BaseQuantity}, Khả dụng: {totalAvailable}.");
				}
			}

			// TẠO PHIẾU XUẤT KHO TỰ ĐỘNG
			var issueTicket = await _ticketManager.CreateTicketAsync(
				SupplyCoreERP.Enums.Warehouses.TicketType.GoodsIssue,
				order.WarehouseId,
				order.Id,
				order.Code,
				$"Phiếu xuất tự động từ đơn bán hàng {order.Code}"
			);

			// CHẠY FEFO CẤP PHÁT HÀNG TỰ ĐỘNG
			foreach (var item in order.Details)
			{
				await _ticketManager.AllocateFEFOAsync(issueTicket, item.ProductId, item.BaseQuantity);
			}

			order.Approve();
		}

		public async Task CompleteAsync(SalesOrder order)
		{
			if (order.Status != SalesOrderStatus.Delivering && order.Status != SalesOrderStatus.Approved)
				throw new UserFriendlyException("Chỉ có thể Hoàn tất đơn hàng khi đang Giao hoặc Đã duyệt!");

			order.Complete();

			var customer = await _customerRepo.GetAsync(order.CustomerId);
			customer.AddDebt(order.TotalAmount);
		}

		public async Task CancelAsync(SalesOrder order, string cancelReason)
		{
			order.Cancel();
			order.UpdateMaster(order.WarehouseId, order.ExpectedDeliveryDate, order.DueDate, $"[Đã hủy: {cancelReason}] " + (order.Note ?? ""));
		}
	}
}