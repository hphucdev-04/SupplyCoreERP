using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.PO.Dtos
{
	public class CreatePurchaseOrderDto
	{
		[Required] 
		public Guid SupplierId { get; set; }
		[Required] 
		public DateTime OrderDate { get; set; }
		public DateTime? ExpectedDeliveryDate { get; set; }
		public string? Note { get; set; }
	}
}
