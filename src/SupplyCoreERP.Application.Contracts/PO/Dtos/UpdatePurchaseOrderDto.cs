using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.PO.Dtos
{
	public class UpdatePurchaseOrderDto
	{
		public string? Note { get; set; }
		public DateTime? ExpectedDeliveryDate { get; set; }
	}
}
