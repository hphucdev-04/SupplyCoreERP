using SupplyCoreERP.Enums.Medicines;
using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Medicines.Dtos
{
	public class MedicineDto : EntityDto<Guid>
	{
		public string Code { get; set; }
		public string Name { get; set; }

		public string CategoryName { get; set; }
		public string ManufacturerName { get; set; }
		public string BaseUnitName { get; set; }
		public string DosageFormName { get; set; }
		public string OriginCountryName { get; set; }

		public StorageCondition StorageCondition { get; set; }
		public MedicineStatus Status { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreationTime { get; set; }
	}
}
