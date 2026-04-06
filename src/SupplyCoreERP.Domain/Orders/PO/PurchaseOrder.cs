using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Suppliers;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Orders.PO
{
	public class PurchaseOrder : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; }
		public Guid SupplierId { get; private set; }
		public virtual Supplier Supplier { get; private set; }
		public DateTime OrderDate { get; private set; }
		public DateTime? ExpectedDeliveryDate { get; private set; }
		public DateTime? DueDate { get; private set; }
		public PurchaseOrderStatus Status { get; private set; }
		public decimal SubTotal { get; private set; }
		public decimal TaxAmount { get; private set; }
		public decimal TotalAmount { get; private set; }
		public string? Note { get; private set; }
		public Guid WarehouseId { get; private set; }

		public virtual ICollection<PurchaseOrderDetail> Details { get; private set; }

		protected PurchaseOrder() { Details = new List<PurchaseOrderDetail>(); }

		public PurchaseOrder(Guid id, string code, Guid supplierId, Guid warehouseId, DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note) : base(id)
		{
			Code = code;
			SupplierId = supplierId;
			WarehouseId = warehouseId;
			OrderDate = orderDate;
			ExpectedDeliveryDate = expectedDeliveryDate;
			DueDate = dueDate;
			Note = note;
			Status = PurchaseOrderStatus.Draft;
			SubTotal = 0;
			TaxAmount = 0;
			TotalAmount = 0;
			Details = new List<PurchaseOrderDetail>();
		}

		public void UpdateMaster(Guid warehouseId, DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
		{
			if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
				throw new UserFriendlyException("Chỉ có thể sửa đơn hàng khi đang ở trạng thái Nháp hoặc Chờ duyệt.");

			WarehouseId = warehouseId;
			ExpectedDeliveryDate = expectedDeliveryDate;
			DueDate = dueDate;
			Note = note;
		}

		public PurchaseOrderDetail AddDetail(Guid id, Guid productId, Guid unitId, int conversionFactor, decimal quantity, decimal unitPrice, decimal taxRate)
		{
			if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
				throw new UserFriendlyException("Chỉ được thêm chi tiết khi đơn đang Nháp hoặc Chờ duyệt.");

			var detail = new PurchaseOrderDetail(id, Id, productId, unitId, conversionFactor, quantity, unitPrice, taxRate);
			Details.Add(detail);

			RecalculateTotal();
			return detail;
		}

		public void UpdateDetail(Guid detailId, decimal quantity, decimal unitPrice, decimal taxRate)
		{
			if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
				throw new UserFriendlyException("Không thể sửa chi tiết khi đơn đã duyệt.");

			var detail = Details.FirstOrDefault(x => x.Id == detailId);
			if (detail == null) throw new UserFriendlyException("Không tìm thấy dòng chi tiết.");

			detail.UpdateInfo(quantity, unitPrice, taxRate);
			RecalculateTotal();
		}

		public void RemoveDetail(Guid detailId)
		{
			if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
				throw new UserFriendlyException("Không thể xóa chi tiết khi đơn hàng đã duyệt.");

			var detail = Details.FirstOrDefault(x => x.Id == detailId);
			if (detail == null) throw new UserFriendlyException("Không tìm thấy dòng chi tiết.");

			Details.Remove(detail);
			RecalculateTotal();
		}

		private void RecalculateTotal()
		{
			SubTotal = Details.Sum(x => x.TotalPrice);
			TaxAmount = Details.Sum(x => x.TaxAmount);
			TotalAmount = SubTotal + TaxAmount;
		}

		public void SendToApprove()
		{
			if (!Details.Any()) throw new UserFriendlyException("Đơn hàng chưa có sản phẩm nào!");
			Status = PurchaseOrderStatus.PendingApproval;
		}

		public void Approve() => Status = PurchaseOrderStatus.Approved;
		public void StartReceiving() => Status = PurchaseOrderStatus.Receiving;
		public void Complete() => Status = PurchaseOrderStatus.Completed;

		public void Cancel()
		{
			if (Status == PurchaseOrderStatus.Completed) throw new UserFriendlyException("Đơn hàng đã hoàn tất, không thể hủy!");
			if (Status == PurchaseOrderStatus.Receiving) throw new UserFriendlyException("Đơn hàng đang nhập kho, yêu cầu Kho xóa phiếu trước!");
			Status = PurchaseOrderStatus.Canceled;
		}
	}
}