using System;
using System.Threading.Tasks;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Balances;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventories.Batches;

public class BatchManager : DomainService
{
    private readonly IRepository<ProductBatch, Guid> _batchRepo;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly DocumentSequenceManager _documentSequenceManager;

    public BatchManager(
        IRepository<ProductBatch, Guid> batchRepo,
        IRepository<InventoryBalance, Guid> balanceRepo,
        DocumentSequenceManager documentSequenceManager)
    {
        _batchRepo = batchRepo;
        _balanceRepo = balanceRepo;
        _documentSequenceManager = documentSequenceManager;
    }

    public async Task<ProductBatch> CreateAsync(Guid productId, string batchNumber, DateTime mfg, DateTime exp, Guid? supplierId)
    {
        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeBatch);

        if (await _batchRepo.AnyAsync(x => x.ProductId == productId && x.BatchNumber == batchNumber))
        {
            throw new UserFriendlyException($"Số lô '{batchNumber}' đã tồn tại!");
        }

        return new ProductBatch(GuidGenerator.Create(), code, productId, batchNumber, mfg, exp, supplierId);
    }

    public void UpdateBatch(ProductBatch batch, DateTime mfg, DateTime exp, Guid? supplierId)
    {
        if (batch.Status == BatchQAStatus.Recalled || batch.Status == BatchQAStatus.Expired)
        {
            throw new UserFriendlyException("Không thể sửa thông tin Lô thuốc đã bị thu hồi hoặc hết hạn!");
        }

        batch.UpdateInfo(mfg, exp, supplierId);
    }

    public async Task ValidateDeleteAsync(Guid batchId)
    {
        if (await _balanceRepo.AnyAsync(x => x.ProductBatchId == batchId))
        {
            throw new UserFriendlyException("Không thể xóa Lô đã phát sinh tồn kho!");
        }
    }
}
