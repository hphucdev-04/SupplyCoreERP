using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Warehouses;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventories.Warehouses
{
	public class WarehouseManager : DomainService
	{
		private readonly IRepository<Warehouse, Guid> _warehouseRepo;
		private readonly IRepository<Zone, Guid> _zoneRepo;
		private readonly IRepository<Bin, Guid> _binRepo;
		private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
		private readonly DocumentSequenceManager _documentSequenceManager;

		public WarehouseManager(
			IRepository<Warehouse, Guid> warehouseRepo,
			IRepository<Zone, Guid> zoneRepo,
			IRepository<Bin, Guid> binRepo,
			IRepository<InventoryBalance, Guid> balanceRepo,
			DocumentSequenceManager documentSequenceManager
			)
		{
			_warehouseRepo = warehouseRepo;
			_zoneRepo = zoneRepo;
			_binRepo = binRepo;
			_balanceRepo = balanceRepo;
			_documentSequenceManager = documentSequenceManager;
		}

		#region Warehouse
		public async Task<Warehouse> CreateAsync(string name, string? address, Guid? countryId, Guid? cityId, Guid? areaId, int width, int length)
		{
			var code  = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeWarehouse);

			if (await _warehouseRepo.AnyAsync(x => x.Code == code))
				throw new UserFriendlyException($"Mã kho '{code}' đã tồn tại!");

			return new Warehouse(GuidGenerator.Create(), code, name, address, countryId, cityId, areaId, width, length);
		}

		public async Task UpdateAsync(Warehouse warehouse, string name, string? address, Guid? countryId, Guid? cityId, Guid? areaId, int width, int length)
		{

			warehouse.UpdateInfo(name, address, countryId, cityId, areaId);
			warehouse.UpdateMapSize(width, length);
		}

		public async Task DeleteAsync(Guid warehouseId)
		{
			if (await _balanceRepo.AnyAsync(x => x.WarehouseId == warehouseId && x.Quantity > 0))
				throw new UserFriendlyException("Không thể xóa Kho đang chứa hàng tồn!");

			await _binRepo.DeleteAsync(x => x.WarehouseId == warehouseId);
			await _zoneRepo.DeleteAsync(x => x.WarehouseId == warehouseId);
			await _warehouseRepo.DeleteAsync(warehouseId);
		}
		#endregion

		#region Zone
		public async Task<Zone> CreateZoneAsync(Guid warehouseId,string name, ZoneType type, StorageCondition condition, string color, int x, int y, int w, int l, float rotation)
		{
			var code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeZone);

			if (await _zoneRepo.AnyAsync(z => z.WarehouseId == warehouseId && z.Code == code))
				throw new UserFriendlyException($"Mã khu vực '{code}' đã tồn tại trong kho này!");

			return new Zone(GuidGenerator.Create(), warehouseId, code, name, type, condition, color, x, y, w, l, rotation);
		}

		public async Task UpdateZoneAsync(Zone zone, string name, ZoneType type, StorageCondition condition, string? color, int x, int y, int w, int l, float rotation)
		{

			zone.UpdateInfo(name, type, condition, color);
			zone.SetCoordinates(x, y, w, l, rotation);
		}

		public async Task DeleteZoneAsync(Guid zoneId)
		{
			if (await _binRepo.AnyAsync(b => b.ZoneId == zoneId))
				throw new UserFriendlyException("Không thể xóa Khu vực đang chứa các vị trí (Bin). Vui lòng xóa Bin trước!");

			await _zoneRepo.DeleteAsync(zoneId);
		}
		#endregion

		#region Bin
		public async Task<Bin> CreateBinAsync(
			Guid warehouseId, Guid zoneId,
			int x, int y, int w, int l, float rotation, int maxSKU)
		{
			var zone = await _zoneRepo.GetAsync(zoneId);
			if (zone.WarehouseId != warehouseId)
				throw new UserFriendlyException("Dữ liệu không hợp lệ: Zone không thuộc Warehouse này!");

			var code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeBin);

			if (await _binRepo.AnyAsync(b => b.WarehouseId == warehouseId && b.Code == code))
				throw new UserFriendlyException($"Mã vị trí '{code}' đã tồn tại trong kho này!");

			return new Bin(GuidGenerator.Create(), warehouseId, zoneId, code, x, y, w, l, rotation, maxSKU);
		}

		public async Task UpdateBinAsync(
			Bin bin, Guid zoneId,
			int x, int y, int w, int l, float rotation, int maxSKU, bool isBlocked)
		{
			if (bin.ZoneId != zoneId)
			{
				var zone = await _zoneRepo.GetAsync(zoneId);
				if (zone.WarehouseId != bin.WarehouseId)
					throw new UserFriendlyException("Dữ liệu không hợp lệ: Zone không thuộc Warehouse này!");
			}

			// Nếu giảm MaxSKU xuống thấp hơn số SKU đang có → cảnh báo
			if (maxSKU > 0)
			{
				int usedSKU = await _balanceRepo.CountAsync(b => b.BinId == bin.Id && b.Quantity > 0);
				if (maxSKU < usedSKU)
					throw new UserFriendlyException(
						$"Không thể đặt giới hạn {maxSKU} SKU vì vị trí '{bin.Code}' đang chứa {usedSKU} loại hàng!");
			}

			bin.UpdateInfo(zoneId, maxSKU);
			bin.SetCoordinates(x, y, w, l, rotation);
			bin.ToggleBlock(isBlocked);
		}

		public async Task DeleteBinAsync(Guid binId)
		{
			if (await _balanceRepo.AnyAsync(bal => bal.BinId == binId && bal.Quantity > 0))
				throw new UserFriendlyException("Không thể xóa Vị trí đang chứa hàng tồn kho!");

			await _binRepo.DeleteAsync(binId);
		}
		#endregion

		#region Logic Validation
		public void ValidateStorageCompatibility(Bin bin, StorageCondition? productCondition)
		{
			if (bin.Zone == null)
				throw new UserFriendlyException("Lỗi hệ thống: Không tải được thông tin Zone!");

			if (productCondition.HasValue && bin.Zone.StorageCondition != productCondition.Value)
				throw new UserFriendlyException(
					$"SAI QUY ĐỊNH BẢO QUẢN!\n" +
					$"Sản phẩm yêu cầu môi trường: '{productCondition}'.\n" +
					$"Nhưng vị trí '{bin.Code}' thuộc Zone '{bin.Zone.Name}' " +
					$"là môi trường: '{bin.Zone.StorageCondition}'."
				);

			if (bin.Zone.Type == ZoneType.Inbound || bin.Zone.Type == ZoneType.Outbound
				|| bin.Zone.Type == ZoneType.ForkliftParking)
				throw new UserFriendlyException(
					$"Không được phép lưu kho tại khu vực vận hành '{bin.Zone.Type}'!");
		}
		#endregion
	}
}