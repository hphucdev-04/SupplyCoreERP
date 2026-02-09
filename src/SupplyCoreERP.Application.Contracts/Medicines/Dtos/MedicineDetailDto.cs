using SupplyCoreERP.Enums.Medicines;
using System;
using System.Collections.Generic;

namespace SupplyCoreERP.Medicines.Dtos
{
	public class MedicineDetailDto : MedicineDto
	{
		public Guid CategoryId { get; set; }
		public Guid ManufacturerId { get; set; }
		public Guid BaseUnitId { get; set; }
		public Guid DosageFormId { get; set; }
		public Guid OriginCountryId { get; set; }

		public string RegistrationNumber { get; set; }
		public UsageRoute UsageRoute { get; set; }
		public StorageCondition StorageCondition { get; set; }
		public bool IsPrescriptionDrug { get; set; }

		public List<MedicineIngredientDto> Ingredients { get; set; }
		public List<MedicineUnitDto> Units { get; set; }
	}
}
