using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Warehouses.Dtos
{
	public class CreateUpdateBinDto
	{
		[Required]
		public Guid WarehouseId { get; set; }

		[Required]
		public Guid ZoneId { get; set; } 

		public int PositionX { get; set; }
		public int PositionY { get; set; }

		[Range(1, int.MaxValue)]
		public int Width { get; set; }

		[Range(1, int.MaxValue)]
		public int Length { get; set; }

		public float Rotation { get; set; }

		public int MaxSKU { get; set; }
		public bool IsBlocked { get; set; }
	}
}