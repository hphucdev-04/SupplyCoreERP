using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Medicines.Dtos
{
	public class CreateUpdateMedicineIngredientDto
	{
		[Required]
		public Guid ActiveIngredientId { get; set; }
	}
}
