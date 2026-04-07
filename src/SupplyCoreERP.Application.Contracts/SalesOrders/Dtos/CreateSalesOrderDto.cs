using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.SalesOrders.Dtos
{
	public class CreateSalesOrderDto
	{
		[Required] 
		public Guid CustomerId { get; set; }
		[Required] 
		public Guid WarehouseId { get; set; }
		[Required] 
		public DateTime OrderDate { get; set; }
		public DateTime? ExpectedDeliveryDate { get; set; }
		public DateTime? DueDate { get; set; }
		[MaxLength(1000)] 
		public string? Note { get; set; }
	}
}
