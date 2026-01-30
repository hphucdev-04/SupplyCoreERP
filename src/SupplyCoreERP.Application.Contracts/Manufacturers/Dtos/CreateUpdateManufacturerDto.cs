using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Manufacturers.Dtos
{
	public class CreateUpdateManufacturerDto
	{
		[Required]
		[StringLength(255)] 
		public string Name { get; set; }

		[Required]
		public Guid ContinentId { get; set; }

		[Required]
		public Guid CountryId { get; set; }
	}
}
