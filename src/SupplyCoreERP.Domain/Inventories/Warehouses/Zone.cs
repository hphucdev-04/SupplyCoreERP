using SupplyCoreERP.Enums.Medicines; 
using SupplyCoreERP.Enums.Warehouses;
using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Warehouses
{
	public class Zone : FullAuditedAggregateRoot<Guid>
	{
		public Guid WarehouseId { get; private set; }

		public string Code { get; private set; } // VD: Z-COLD-01
		public string Name { get; private set; } // VD: Kho Lạnh Vắc-xin

		public ZoneType Type { get; private set; }

		public StorageCondition StorageCondition { get; private set; }

		public string Color { get; private set; } 

		public int PositionX { get; private set; }
		public int PositionY { get; private set; }
		public int Width { get; private set; }
		public int Length { get; private set; }
		public float Rotation { get; private set; }

		protected Zone() { }

		public Zone(Guid id, Guid warehouseId, string code, string name,
					ZoneType type, StorageCondition condition, string color,
					int x, int y, int w, int l, float rotation) : base(id)
		{
			WarehouseId = warehouseId;
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code)).ToUpper();
			Name = name;
			Type = type;
			StorageCondition = condition; // QUAN TRỌNG
			Color = color ?? "#CCCCCC";
			SetCoordinates(x, y, w, l, rotation);
		}

		public void SetCoordinates(int x, int y, int w, int l, float rotation)
		{
			PositionX = x; PositionY = y; Width = w; Length = l; Rotation = rotation;
		}

		public void UpdateInfo(string name, ZoneType type, StorageCondition condition, string? color)
		{
			Name = name;
			Type = type;
			StorageCondition = condition;
			Color = color ?? "#CCCCCC";
		}
	}
}