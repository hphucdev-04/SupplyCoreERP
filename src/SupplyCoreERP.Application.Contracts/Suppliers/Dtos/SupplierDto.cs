using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Suppliers.Dtos
{
	public class SupplierDto : FullAuditedEntityDto<Guid>
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public string? CityName { get; set; }
		public decimal CurrentDebt { get; set; }
		public bool IsActive { get; set; }
	}
}
