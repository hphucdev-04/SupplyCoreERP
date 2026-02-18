using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.BaseUnits.Dtos
{
	public class CreateUpdateBaseUnitDto
	{
		[Required]
		[StringLength(50)]
		public string Code { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }
	}
}
