using SupplyCoreERP.Enums.Balances;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Balances.Dtos
{
	public class InventoryReservationDto : CreationAuditedEntityDto<Guid>
	{
		public Guid ReferenceDocumentId { get; set; }
		public string ReferenceDocumentNumber { get; set; }
		public decimal ReservedQuantity { get; set; }
		public ReservationStatus Status { get; set; }
	}
}
