using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Warehouses
{
	public class Bin : FullAuditedAggregateRoot<Guid>
	{
		public Guid WarehouseId { get; private set; }

		// Bắt buộc thuộc Zone để biết Bin này Lạnh hay Thường
		public Guid ZoneId { get; private set; }
		public virtual Zone Zone { get; protected set; }

		public string Code { get; private set; } // VD: A-01-02

		// --- TỌA ĐỘ & KÍCH THƯỚC (RESIZE ĐƯỢC) ---
		public int PositionX { get; private set; }
		public int PositionY { get; private set; }
		public int Width { get; private set; }
		public int Length { get; private set; }
		public float Rotation { get; private set; }

		public decimal MaxWeight { get; private set; }
		public bool IsBlocked { get; private set; }

		protected Bin() { }

		public Bin(Guid id, Guid warehouseId, Guid zoneId, string code,
				   int x, int y, int w, int l, float rotation, decimal maxWeight = 0) : base(id)
		{
			WarehouseId = warehouseId;
			ZoneId = zoneId;
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code)).ToUpper();
			MaxWeight = maxWeight;
			SetCoordinates(x, y, w, l, rotation);
		}

		public void SetCoordinates(int x, int y, int w, int l, float rotation)
		{
			PositionX = x; PositionY = y; Width = w; Length = l; Rotation = rotation;
		}

		public void UpdateInfo(Guid zoneId, decimal maxWeight, string code)
		{
			ZoneId = zoneId;
			MaxWeight = maxWeight;
			Code = code;
		}

		public void ToggleBlock(bool isBlocked) => IsBlocked = isBlocked;
	}
}