using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.PO.Dtos
{
	public class UpdatePurchaseOrderDetailDto
	{
		[Required, Range(0.01, double.MaxValue)] 
		public decimal Quantity { get; set; }
		[Required, Range(0, double.MaxValue)] 
		public decimal UnitPrice { get; set; }
		[Required, Range(0, 100)] 
		public decimal TaxRate { get; set; }
	}
}
