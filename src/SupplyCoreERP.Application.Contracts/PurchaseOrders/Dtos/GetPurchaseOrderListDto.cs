using SupplyCoreERP.Enums.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PurchaseOrders.Dtos
{
	public class GetPurchaseOrderListDto : PagedAndSortedResultRequestDto
	{
		public string? Filter { get; set; }
		public Guid? SupplierId { get; set; }
		public Guid? WarehouseId { get; set; }
		public PurchaseOrderStatus? Status { get; set; }
	}
}
