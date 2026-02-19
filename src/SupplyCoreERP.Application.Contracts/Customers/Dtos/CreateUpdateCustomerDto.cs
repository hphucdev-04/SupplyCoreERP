using SupplyCoreERP.Enums.Partner;
using System;
using System.ComponentModel.DataAnnotations;


namespace SupplyCoreERP.Customers.Dtos
{
	public class CreateUpdateCustomerDto
	{
		[Required]
		[MaxLength(50)]
		public string Code { get; set; }

		[Required]
		[MaxLength(255)]
		public string Name { get; set; }

		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public Gender? Gender { get; set; }
		public CustomerType Type { get; set; }
		public string? TaxCode { get; set; }

		public string? Address { get; set; }
		public Guid? CountryId { get; set; }
		public Guid? CityId { get; set; }
		public Guid? AreaId { get; set; }

		public decimal DebtLimit { get; set; }
		public int PaymentTermDays { get; set; }

	}
}
