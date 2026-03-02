using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Warehouses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Warehouses.Dtos
{
	public class CreateUpdateZoneDto
	{
		[Required]
		public Guid WarehouseId { get; set; }

		[Required]
		[MaxLength(50)]
		public string Code { get; set; }

		[Required]
		[MaxLength(255)]
		public string Name { get; set; }

		[Required]
		public ZoneType Type { get; set; }

		[Required]
		public StorageCondition StorageCondition { get; set; }

		public string? Color { get; set; } // Hex code

		public int PositionX { get; set; }
		public int PositionY { get; set; }

		[Range(1, int.MaxValue)]
		public int Width { get; set; }

		[Range(1, int.MaxValue)]
		public int Length { get; set; }

		public float Rotation { get; set; }
	}
}
