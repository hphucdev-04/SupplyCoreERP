using SupplyCoreERP.Enums.Partner;
using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Customers.Dtos
{
	public class CustomerDto : FullAuditedEntityDto<Guid>
	{
		public string Code { get; set; }
		public string Name { get; set; }
		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public Gender? Gender { get; set; }
		public CustomerType Type { get; set; }
		public string? TaxCode { get; set; }
		public bool IsActive { get; set; }

		public string? Address { get; set; }
		public Guid? CountryId { get; set; }
		public string? CountryName { get; set; }
		public Guid? CityId { get; set; }
		public string? CityName { get; set; }
		public Guid? AreaId { get; set; }
		public string? AreaName { get; set; }

		public decimal DebtLimit { get; set; }
		public int PaymentTermDays { get; set; }
		public decimal CurrentDebt { get; set; }
	}
}
