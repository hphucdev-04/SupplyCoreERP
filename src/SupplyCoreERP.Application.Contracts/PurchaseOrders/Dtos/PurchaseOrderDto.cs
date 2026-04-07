using SupplyCoreERP.Enums.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseOrders.Dtos
{
	public class PurchaseOrderDto : FullAuditedEntityDto<Guid>
	{
		public string Code { get; set; }

		public Guid SupplierId { get; set; }
		public string? SupplierName { get; set; }

		public Guid WarehouseId { get; set; }
		public string? WarehouseName { get; set; }

		public DateTime OrderDate { get; set; }
		public DateTime? ExpectedDeliveryDate { get; set; }
		public DateTime? DueDate { get; set; }
		public PurchaseOrderStatus Status { get; set; }

		public decimal SubTotal { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public string? Note { get; set; }

		public List<PurchaseOrderDetailDto> Details { get; set; } = new List<PurchaseOrderDetailDto>();
	}
}
