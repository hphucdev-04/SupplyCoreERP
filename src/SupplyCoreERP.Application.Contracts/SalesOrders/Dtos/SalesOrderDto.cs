using SupplyCoreERP.Enums.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.SalesOrders.Dtos
{
	public class SalesOrderDto : FullAuditedEntityDto<Guid>
	{
		public string Code { get; set; }

		public Guid CustomerId { get; set; }
		public string? CustomerName { get; set; }

		public Guid WarehouseId { get; set; }
		public string? WarehouseName { get; set; }

		public DateTime OrderDate { get; set; }
		public DateTime? ExpectedDeliveryDate { get; set; }
		public DateTime? DueDate { get; set; }
		public SalesOrderStatus Status { get; set; }

		public decimal SubTotal { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public string? Note { get; set; }

		public List<SalesOrderDetailDto> Details { get; set; } = new List<SalesOrderDetailDto>();
	}
}
