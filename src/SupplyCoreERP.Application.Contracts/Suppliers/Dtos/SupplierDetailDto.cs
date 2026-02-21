using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Suppliers.Dtos
{
	public class SupplierDetailDto : SupplierDto
	{
		public string? TaxCode { get; set; }
		public string? RepresentativeName { get; set; }
		public string? Note { get; set; }

		public string? Address { get; set; }
		public Guid? CountryId { get; set; }
		public string? CountryName { get; set; }
		public Guid? CityId { get; set; }
		public Guid? AreaId { get; set; }
		public string? AreaName { get; set; }

		public decimal DebtLimit { get; set; }
		public int PaymentTermDays { get; set; }
	}
}
