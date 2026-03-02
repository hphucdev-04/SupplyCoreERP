using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventories.Batches
{
	public class BatchManager : DomainService
	{
		private readonly IRepository<ProductBatch, Guid> _batchRepo;
		private readonly IRepository<InventoryBalance, Guid> _balanceRepo;

		public BatchManager(IRepository<ProductBatch, Guid> batchRepo, IRepository<InventoryBalance, Guid> balanceRepo)
		{
			_batchRepo = batchRepo; _balanceRepo = balanceRepo;
		}

		public async Task<ProductBatch> CreateAsync(Guid productId, string batchNumber, DateTime mfg, DateTime exp, Guid? supplierId)
		{
			if (await _batchRepo.AnyAsync(x => x.ProductId == productId && x.BatchNumber == batchNumber))
				throw new UserFriendlyException($"Số lô '{batchNumber}' đã tồn tại!");
			return new ProductBatch(GuidGenerator.Create(), productId, batchNumber, mfg, exp, supplierId);
		}

		public async Task UpdateAsync(Guid id, DateTime mfg, DateTime exp, Guid? supplierId)
		{
			var batch = await _batchRepo.GetAsync(id);
			if (batch.Status == BatchQAStatus.Recalled || batch.Status == BatchQAStatus.Expired)
				throw new UserFriendlyException("Không thể sửa thông tin Lô thuốc đã bị thu hồi hoặc hết hạn!");

			batch.UpdateInfo(mfg, exp, supplierId);
			await _batchRepo.UpdateAsync(batch);
		}

		public async Task DeleteAsync(Guid batchId)
		{
			if (await _balanceRepo.AnyAsync(x => x.ProductBatchId == batchId))
				throw new UserFriendlyException("Không thể xóa Lô đã phát sinh tồn kho!");
			await _batchRepo.DeleteAsync(batchId);
		}
	}
}