using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Tickets.Dtos;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Tickes.Dtos
{
	public class InventoryTicketDto : FullAuditedEntityDto<Guid>
	{
		public string TicketNumber { get; set; }
		public TicketType Type { get; set; }
		public ApprovalStatus Status { get; set; }

		public Guid WarehouseId { get; set; }
		public string? WarehouseName { get; set; }

		public Guid? ReferenceDocumentId { get; set; }
		public string? Note { get; set; }

		public List<InventoryTicketDetailDto> Details { get; set; } = new List<InventoryTicketDetailDto>();
	}
}