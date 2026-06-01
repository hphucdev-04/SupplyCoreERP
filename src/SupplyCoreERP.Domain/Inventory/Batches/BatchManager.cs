using System;
using System.Threading.Tasks;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Balances;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Inventory.Batches;

public class BatchManager : DomainService
{
    // Dependencies
    private readonly IRepository<ProductBatch, Guid> _batchRepo;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly IDocumentSequenceManager _documentSequenceManager;

    // Constructor injection
    public BatchManager(
        IRepository<ProductBatch, Guid> batchRepo,
        IRepository<InventoryBalance, Guid> balanceRepo,
        IDocumentSequenceManager documentSequenceManager)
    {
        _batchRepo = batchRepo;
        _balanceRepo = balanceRepo;
        _documentSequenceManager = documentSequenceManager;
    }

    #region Batch 
    public async Task<ProductBatch> CreateAsync(
        Guid productId,
        string batchNumber,
        DateTime mfg,
        DateTime exp,
        Guid? supplierId,
        Guid? medicineRegistrationId = null)
    {
        string code = await _documentSequenceManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeBatch);

        if (await _batchRepo.AnyAsync(x => x.ProductId == productId && x.BatchNumber == batchNumber))
        {
            throw new BusinessException("SupplyCoreERP:BatchAlreadyExists", $"Số lô '{batchNumber}' đã tồn tại!");
        }

        return new ProductBatch(GuidGenerator.Create(), code, productId, batchNumber, mfg, exp, supplierId, medicineRegistrationId);
    }

    public void UpdateBatch(
        ProductBatch batch,
        DateTime mfg,
        DateTime exp,
        Guid? supplierId,
        Guid? medicineRegistrationId = null)
    {
        if (batch.Status == BatchQAStatus.Recalled || batch.Status == BatchQAStatus.Expired)
        {
            throw new BusinessException("SupplyCoreERP:BatchCannotBeUpdated", "Không thể sửa thông tin Lô thuốc đã bị thu hồi hoặc hết hạn!");
        }

        batch.UpdateInfo(mfg, exp, supplierId, medicineRegistrationId);
    }
    #endregion

    #region Validation
    public async Task ValidateDeleteAsync(Guid batchId)
    {
        if (await _balanceRepo.AnyAsync(x => x.ProductBatchId == batchId))
        {
            throw new BusinessException("SupplyCoreERP:BatchCannotBeDeleted", "Không thể xóa Lô đã phát sinh tồn kho!");
        }
    }
    #endregion
}






