using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.DosageForms.Dtos
{
	public class CreateUpdateDosageFormDto
	{
		[Required]
		[StringLength(50)]
		public string Code { get; set; }

		[Required]
		[StringLength(255)]
		public string Name { get; set; }
	}
}
