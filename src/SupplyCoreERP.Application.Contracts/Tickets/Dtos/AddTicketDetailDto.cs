using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Tickets.Dtos
{
	public class AddTicketDetailDto
	{
		[Required]
		public Guid ProductId { get; set; }
		[Required]
		public Guid ProductBatchId { get; set; }
		[Required]
		public Guid BinId { get; set; }
		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal Quantity { get; set; }
	}
}
