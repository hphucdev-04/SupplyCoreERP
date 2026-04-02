using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Orders;
using SupplyCoreERP.Products;
using SupplyCoreERP.Suppliers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Purchasing.Orders
{
	public class PurchaseOrderManager : DomainService
	{
		private readonly IRepository<PurchaseOrder, Guid> _orderRepo;
		private readonly IRepository<Supplier, Guid> _supplierRepo;
		private readonly IRepository<Product, Guid> _productRepo;

		public PurchaseOrderManager(
			IRepository<PurchaseOrder, Guid> orderRepo,
			IRepository<Supplier, Guid> supplierRepo,
			IRepository<Product, Guid> productRepo)
		{
			_orderRepo = orderRepo;
			_supplierRepo = supplierRepo;
			_productRepo = productRepo;
		}

		public async Task<PurchaseOrder> CreateOrderAsync(Guid supplierId, DateTime orderDate, DateTime? expectedDeliveryDate, string? note)
		{
			var supplier = await _supplierRepo.GetAsync(supplierId);
			if (!supplier.IsActive)
				throw new UserFriendlyException($"Nhà cung cấp '{supplier.Name}' đang bị khóa!");

			string code = $"PO-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

			return new PurchaseOrder(GuidGenerator.Create(), code, supplierId, orderDate, expectedDeliveryDate, note);
		}

		public async Task UpdateOrderAsync(PurchaseOrder order, DateTime? expectedDeliveryDate, string? note)
		{
			// Ném dữ liệu cho Entity tự xử lý và tự check trạng thái
			order.UpdateMaster(expectedDeliveryDate, note);
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

			order.SendToApprove(); // Entity sẽ tự check xem có Detail nào không
		}

		public async Task ApproveAsync(PurchaseOrder order)
		{
			if (order.Status != PurchaseOrderStatus.PendingApproval)
				throw new UserFriendlyException("Đơn hàng chưa được gửi duyệt!");

			if (!order.Details.Any())
				throw new UserFriendlyException("Đơn hàng chưa có sản phẩm, không thể duyệt!");

			Guid[] productIds = order.Details.Select(x => x.ProductId).Distinct().ToArray();
			var products = await _productRepo.GetListAsync(x => productIds.Contains(x.Id));

			foreach (var item in order.Details)
			{
				var product = products.FirstOrDefault(p => p.Id == item.ProductId);
				if (product == null || !product.IsAvailableForInventory)
					throw new UserFriendlyException($"Sản phẩm '{product?.Name}' không còn đủ điều kiện giao dịch!");
			}

			order.Approve();
		}

		public async Task CancelAsync(PurchaseOrder order, string cancelReason)
		{
			order.Cancel(); // Entity tự lo chặn các trạng thái không được phép hủy

			string noteUpdate = $"[Đã hủy: {cancelReason}] " + (order.Note ?? "");
			order.UpdateMaster(order.ExpectedDeliveryDate, noteUpdate);
		}
	}
}