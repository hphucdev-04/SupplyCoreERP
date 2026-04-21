using SupplyCoreERP.Customers;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Prices;
using SupplyCoreERP.Products;
using SupplyCoreERP.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Sales.Orders
{
	public class SalesOrderManager : DomainService
	{
		// Dependencies
		private readonly IRepository<SalesOrder, Guid> _orderRepo;
		private readonly IRepository<Customer, Guid> _customerRepo;
		private readonly IRepository<Product, Guid> _productRepo;
		private readonly IRepository<Warehouse, Guid> _warehouseRepo;
		private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
		private readonly PriceManager _priceManager;
		private readonly TicketManager _ticketManager;
        private readonly DocumentSequenceManager _documentManager;

        // DI
        public SalesOrderManager(
			IRepository<SalesOrder, Guid> orderRepo,
			IRepository<Customer, Guid> customerRepo,
			IRepository<Product, Guid> productRepo,
			IRepository<Warehouse, Guid> warehouseRepo,
			IRepository<InventoryBalance, Guid> balanceRepo,
			PriceManager priceManager,
			TicketManager ticketManager,
            DocumentSequenceManager documentManager
            )
		{
			_orderRepo = orderRepo;
			_customerRepo = customerRepo;
			_productRepo = productRepo;
			_warehouseRepo = warehouseRepo;
			_balanceRepo = balanceRepo;
			_priceManager = priceManager;
			_ticketManager = ticketManager;
			_documentManager = documentManager;
		}

        #region SaleOrder
        public async Task<SalesOrder> CreateOrderAsync(
			Guid customerId, Guid warehouseId, DateTime orderDate,
			DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note)
		{
            await ValidateAsync(customerId, warehouseId, orderDate, expectedDeliveryDate, inputDueDate);
            var customer = await _customerRepo.GetAsync(customerId);
			if (!customer.IsActive)
				throw new UserFriendlyException($"Khách hàng '{customer.Name}' đang bị khóa!");

			string code = await _documentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeCustomer);

			DateTime? finalDueDate = inputDueDate
				?? (customer.PaymentTermDays > 0 ? orderDate.AddDays(customer.PaymentTermDays) : null);

			return new SalesOrder(GuidGenerator.Create(), code, customerId, warehouseId,
								  orderDate, expectedDeliveryDate, finalDueDate, note);
		}

		public async Task UpdateOrderAsync(SalesOrder order, Guid warehouseId,
			DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
		{
            await ValidateAsync(order.CustomerId,  warehouseId, order.OrderDate, expectedDeliveryDate, dueDate);
            order.UpdateInfo(warehouseId, expectedDeliveryDate, dueDate, note);
			
		}

		public Task CheckBeforeDeleteAsync(SalesOrder order)
		{
			if (order.Status != SalesOrderStatus.Draft)
				throw new UserFriendlyException("Chỉ có thể xóa đơn bán đang ở trạng thái Nháp!");
			return Task.CompletedTask;
		}
        #endregion

        #region SaleOrder Details
        public async Task AddDetailAsync(SalesOrder order, Guid productId, Guid unitId,
			int conversionFactor, decimal quantity, decimal discountRate, decimal taxRate)
		{
			var product = await _productRepo.GetAsync(productId);
			if (!product.IsAvailableForInventory)
				throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa đủ điều kiện giao dịch!");

			var customer = await _customerRepo.GetAsync(order.CustomerId);
			decimal price = await _priceManager.GetOfficialPriceAsync(customer.PriceListId, productId, unitId, quantity);

			order.AddDetail(GuidGenerator.Create(), productId, unitId, conversionFactor, quantity, price, discountRate, taxRate);
		}

		public async Task UpdateDetailAsync(SalesOrder order, Guid detailId,
			decimal quantity, decimal discountRate, decimal taxRate)
		{
			var detail = order.Details.FirstOrDefault(x => x.Id == detailId)
				?? throw new UserFriendlyException("Không tìm thấy dòng chi tiết.");

			var customer = await _customerRepo.GetAsync(order.CustomerId);
			decimal newPrice = await _priceManager.GetOfficialPriceAsync(
				customer.PriceListId, detail.ProductId, detail.UnitId, quantity);

			order.UpdateDetail(detailId, quantity, newPrice, discountRate, taxRate);
		}

		public Task RemoveDetailAsync(SalesOrder order, Guid detailId)
		{
			order.RemoveDetail(detailId);
			return Task.CompletedTask;
		}
        #endregion

        #region Workflow
        public Task SendToApproveAsync(SalesOrder order)
		{
			order.SendToApprove();
			return Task.CompletedTask;
		}

		/// <summary>
		/// Validate tồn kho, tạo phiếu xuất kho và cấp phát FEFO tự động.
		/// </summary>
		/// <returns>
		/// Ticket mới tạo (chưa Insert) và danh sách FEFO details (chưa Insert).
		/// AppService chịu trách nhiệm InsertAsync ticket + InsertManyAsync details.
		/// </returns>
		public async Task<(InventoryTicket Ticket, IList<InventoryTicketDetail> FefoDetails)> ApproveAsync(SalesOrder order)
		{
			if (order.Status != SalesOrderStatus.PendingApproval)
				throw new UserFriendlyException("Đơn hàng chưa được gửi duyệt!");

			if (!order.Details.Any())
				throw new UserFriendlyException("Đơn hàng chưa có sản phẩm, không thể duyệt!");

			var customer = await _customerRepo.GetAsync(order.CustomerId);

			if (customer.DebtLimit > 0 && (customer.CurrentDebt + order.TotalAmount) > customer.DebtLimit)
				throw new UserFriendlyException($"Từ chối duyệt! Khách '{customer.Name}' sẽ vượt hạn mức nợ.");

			var today = DateTime.Now.Date;
			bool hasOverdue = await _orderRepo.AnyAsync(x =>
				x.CustomerId == order.CustomerId &&
				x.Status == SalesOrderStatus.Completed &&
				x.DueDate.HasValue && x.DueDate.Value.Date < today);

			if (hasOverdue)
				throw new UserFriendlyException("Khách hàng đang có đơn hàng cũ quá hạn. Vui lòng thu hồi nợ trước!");

			var productIds = order.Details.Select(x => x.ProductId).Distinct().ToList();
			var balances = await _balanceRepo.GetListAsync(
				x => productIds.Contains(x.ProductId) && x.WarehouseId == order.WarehouseId);
			var products = await _productRepo.GetListAsync(x => productIds.Contains(x.Id));

			foreach (var item in order.Details)
			{
				decimal totalAvailable = balances
					.Where(x => x.ProductId == item.ProductId)
					.Sum(x => x.AvailableQuantity);

				if (totalAvailable < item.BaseQuantity)
				{
					var product = products.FirstOrDefault(p => p.Id == item.ProductId);
					throw new UserFriendlyException(
						$"Sản phẩm '{product?.Name}' không đủ tồn kho! " +
						$"Cần: {item.BaseQuantity}, Khả dụng: {totalAvailable}.");
				}
			}

			// Tạo phiếu xuất — chưa Insert (ticket.Id đã có từ GuidGenerator)
			var issueTicket = await _ticketManager.CreateTicketAsync(
				TicketType.GoodsIssue, order.WarehouseId, order.Id, order.Code,
				$"Phiếu xuất tự động từ đơn bán hàng {order.Code}");

			// FEFO cấp phát — chưa Insert details
			var allFefoDetails = new List<InventoryTicketDetail>();
			foreach (var item in order.Details)
			{
				var details = await _ticketManager.AllocateFEFOAsync(issueTicket, item.ProductId, item.BaseQuantity);
				allFefoDetails.AddRange(details);
			}

			order.Approve();

			return (issueTicket, allFefoDetails);
		}

		/// <summary>
		/// Hoàn tất đơn bán và cộng công nợ cho khách hàng.
		/// </summary>
		/// <returns>Customer đã AddDebt — chưa được lưu DB. AppService gọi UpdateAsync.</returns>
		public async Task<Customer> CompleteAsync(SalesOrder order)
		{
			if (!await _ticketManager.HasStatusAsync(order.Id, ApprovalStatus.Approved))
				throw new UserFriendlyException("Phiếu xuất kho chưa được thực thi! Cần hoàn tất xuất kho trước.");

			if (order.Status != SalesOrderStatus.Delivering && order.Status != SalesOrderStatus.Approved)
				throw new UserFriendlyException("Chỉ có thể Hoàn tất khi đang Giao hoặc Đã duyệt!");

			order.Complete();

			var customer = await _customerRepo.GetAsync(order.CustomerId);
			customer.AddDebt(order.TotalAmount);

			return customer; 
		}

		public Task CancelAsync(SalesOrder order, string cancelReason)
		{
			order.Cancel();
			order.UpdateInfo(order.WarehouseId, order.ExpectedDeliveryDate, order.DueDate,
							   $"[Đã hủy: {cancelReason}] " + (order.Note ?? ""));
			return Task.CompletedTask;
		}
        #endregion

        #region Validate
        private async Task ValidateAsync(Guid customerId, Guid warehouseId,
           DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? dueDate)
        {
            if (!await _customerRepo.AnyAsync(x => x.Id == customerId))
                throw new UserFriendlyException("Khách hàng không tồn tại.");

            if (!await _warehouseRepo.AnyAsync(x => x.Id == warehouseId))
                throw new UserFriendlyException("Kho hàng không tồn tại.");

            if (orderDate.Date > DateTime.Now.Date)
                throw new UserFriendlyException("Ngày đặt hàng không được ở tương lai.");

            if (expectedDeliveryDate.HasValue && expectedDeliveryDate.Value.Date < orderDate.Date)
                throw new UserFriendlyException("Ngày giao hàng dự kiến không được trước ngày đặt hàng.");

            if (dueDate.HasValue && dueDate.Value.Date < orderDate.Date)
                throw new UserFriendlyException("Ngày đáo hạn không được trước ngày đặt hàng.");
        }
        #endregion
    }
}