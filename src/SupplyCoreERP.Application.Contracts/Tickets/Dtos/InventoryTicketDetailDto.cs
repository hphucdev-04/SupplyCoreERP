using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickets.Dtos
{
	public class InventoryTicketDetailDto : FullAuditedEntityDto<Guid>
	{
		public Guid TicketId { get; set; }

		public Guid ProductId { get; set; }
		public string? ProductName { get; set; }

		public Guid ProductBatchId { get; set; }
		public string? BatchNumber { get; set; }

		public Guid BinId { get; set; }
		public string? BinCode { get; set; }

		public decimal Quantity { get; set; }
	}
}