using SupplyCoreERP.Enums.Medicines;
using System;
using Volo.Abp.Application.Dtos;

namespace SupplyCoreERP.Medicines.Dtos
{
	public class GetMedicineListDto : PagedAndSortedResultRequestDto
	{
		public string? Filter { get; set; }
		public Guid? CategoryId { get; set; }
		public Guid? ManufacturerId { get; set; }
		public MedicineStatus? Status { get; set; }
		public bool? IsActive { get; set; }
	}
}
