using SupplyCoreERP.ActiveIngredients;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace SupplyCoreERP.Medicines
{
	public class MedicineIngredient : Entity<Guid>
	{
		public Guid MedicineId { get; private set; }
		public Guid ActiveIngredientId { get; private set; }
		public virtual ActiveIngredient ActiveIngredient { get; private set; }
	
		private MedicineIngredient() { }
		public MedicineIngredient(Guid medicineId, Guid ingredientId)
		{
			MedicineId = medicineId;
			ActiveIngredientId = ingredientId;
		}
	}
}
