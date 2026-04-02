using SupplyCoreERP.Enums.Orders;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.PO.Dtos
{
	public class PurchaseOrderDto : FullAuditedEntityDto<Guid>
	{
		public string PoNumber { get; set; }
		public Guid SupplierId { get; set; }
		public string SupplierName { get; set; } 
		public DateTime OrderDate { get; set; }
		public DateTime? ExpectedDeliveryDate { get; set; }
		public PurchaseOrderStatus Status { get; set; }
		public decimal SubTotal { get; set; }
		public decimal TaxAmount { get; set; }
		public decimal TotalAmount { get; set; }
		public string? Note { get; set; }

		public List<PurchaseOrderDetailDto> Details { get; set; }
	}
}
