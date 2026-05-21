using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Products;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Orders.PR;

public class PurchaseRequisitionManager : DomainService
{
    private readonly IRepository<PurchaseRequisition, Guid> _requisitionRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly DocumentSequenceManager _documentManager;

    public PurchaseRequisitionManager(
        IRepository<PurchaseRequisition, Guid> requisitionRepo,
        IRepository<Product, Guid> productRepo,
        IRepository<Warehouse, Guid> warehouseRepo,
        DocumentSequenceManager documentManager)
    {
        _requisitionRepo = requisitionRepo;
        _productRepo = productRepo;
        _warehouseRepo = warehouseRepo;
        _documentManager = documentManager;
    }

    public async Task<PurchaseRequisition> CreateAsync(
        Guid warehouseId,
        DateTime requestedDate,
        DateTime? requiredDate,
        string? note)
    {
        await ValidateWarehouseAsync(warehouseId);
        ValidateDates(requestedDate, requiredDate);

        string code = await _documentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypePurchaseRequisition);

        return new PurchaseRequisition(
            GuidGenerator.Create(),
            code,
            warehouseId,
            requestedDate,
            requiredDate,
            note);
    }

    public async Task UpdateAsync(PurchaseRequisition requisition, Guid warehouseId, DateTime? requiredDate, string? note)
    {
        await ValidateWarehouseAsync(warehouseId);
        ValidateDates(requisition.RequestedDate, requiredDate);
        requisition.UpdateInfo(warehouseId, requiredDate, note);
    }

    private async Task ValidateWarehouseAsync(Guid warehouseId)
    {
        Warehouse warehouse = await _warehouseRepo.GetAsync(warehouseId);
        if (!warehouse.IsActive)
        {
            throw new UserFriendlyException($"Kho hàng '{warehouse.Name}' đang bị khóa.");
        }
    }

    public async Task AddLineAsync(
        PurchaseRequisition requisition,
        Guid productId,
        Guid unitId,
        decimal quantity,
        string? note)
    {
        Product product = await _productRepo.GetAsync(productId);
        if (!product.IsAvailableForInventory)
        {
            throw new UserFriendlyException($"Sản phẩm '{product.Name}' không sẵn sàng để giao dịch.");
        }

        requisition.AddLine(GuidGenerator.Create(), productId, unitId, quantity, note);
    }

    public Task UpdateLineAsync(PurchaseRequisition requisition, Guid lineId, decimal quantity, string? note)
    {
        requisition.UpdateLine(lineId, quantity, note);
        return Task.CompletedTask;
    }

    public Task RemoveLineAsync(PurchaseRequisition requisition, Guid lineId)
    {
        requisition.RemoveLine(lineId);
        return Task.CompletedTask;
    }

    public Task SendToApproveAsync(PurchaseRequisition requisition)
    {
        requisition.SendToApprove();
        return Task.CompletedTask;
    }

    public Task ApproveAsync(PurchaseRequisition requisition)
    {
        requisition.Approve();
        return Task.CompletedTask;
    }

    public Task RejectAsync(PurchaseRequisition requisition)
    {
        requisition.Reject();
        return Task.CompletedTask;
    }

    private void ValidateDates(DateTime requestedDate, DateTime? requiredDate)
    {
        if (requestedDate.Date > DateTime.Now.Date)
        {
            throw new UserFriendlyException("Ngày yêu cầu không được ở tương lai.");
        }

        if (requiredDate.HasValue && requiredDate.Value.Date < requestedDate.Date)
        {
            throw new UserFriendlyException("Ngày cần hàng không được trước ngày yêu cầu.");
        }
    }
}
