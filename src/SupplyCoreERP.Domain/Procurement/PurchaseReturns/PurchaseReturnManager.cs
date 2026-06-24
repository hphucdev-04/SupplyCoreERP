using System;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseOrders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Procurement.PurchaseReturns;

public class PurchaseReturnManager : DomainService, IPurchaseReturnManager
{
    private readonly IRepository<PurchaseReturn, Guid> _purchaseReturnRepo;
    private readonly IRepository<PurchaseReturnLine, Guid> _lineRepo;
    private readonly IRepository<PurchaseOrderLine, Guid> _poLineRepo;
    private readonly ITicketManager _ticketManager;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IDocumentSequenceManager _documentManager;
    private readonly IRepository<Supplier, Guid> _supplierRepo;

    public PurchaseReturnManager(
        IRepository<PurchaseReturn, Guid> purchaseReturnRepo,
        IRepository<PurchaseReturnLine, Guid> lineRepo,
        IRepository<PurchaseOrderLine, Guid> poLineRepo,
        ITicketManager ticketManager,
        IRepository<InventoryTicket, Guid> ticketRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        IDocumentSequenceManager documentManager,
        IRepository<Supplier, Guid> supplierRepo)
    {
        _purchaseReturnRepo = purchaseReturnRepo;
        _lineRepo = lineRepo;
        _poLineRepo = poLineRepo;
        _ticketManager = ticketManager;
        _ticketRepo = ticketRepo;
        _ticketLineRepo = ticketLineRepo;
        _documentManager = documentManager;
        _supplierRepo = supplierRepo;
    }

    public async Task<PurchaseReturn> CreateAsync(
        Guid purchaseOrderId,
        Guid supplierId,
        Guid warehouseId,
        PurchaseReturnType returnType,
        DateTime returnDate,
        string? note)
    {
        Supplier supplier = await _supplierRepo.GetAsync(supplierId);
        if (!supplier.IsActive)
        {
            throw new BusinessException("SupplyCoreERP:InactiveSupplier", $"Nhà cung cấp '{supplier.Name}' đang bị khóa!");
        }

        // Tự sinh mã code từ DocumentSequenceManager với sequence code RO
        string code = await _documentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypePurchaseReturn);

        return new PurchaseReturn(
            GuidGenerator.Create(),
            code,
            purchaseOrderId,
            supplierId,
            warehouseId,
            returnType,
            returnDate,
            note
        );
    }

    public Task UpdateAsync(
        PurchaseReturn purchaseReturn,
        Guid warehouseId,
        PurchaseReturnType returnType,
        DateTime returnDate,
        string? note)
    {
        purchaseReturn.UpdateInfo(warehouseId, returnType, returnDate, note);
        return Task.CompletedTask;
    }

    public Task CheckBeforeDeleteAsync(PurchaseReturn purchaseReturn)
    {
        if (purchaseReturn.Status != PurchaseReturnStatus.Draft && purchaseReturn.Status != PurchaseReturnStatus.Rejected)
        {
            throw new BusinessException("SupplyCoreERP:InvalidStatus", "Chỉ có thể xóa chứng từ xuất trả khi đang ở trạng thái Nháp hoặc Từ chối!");
        }
        return Task.CompletedTask;
    }

    public async Task AddLineAsync(
        PurchaseReturn purchaseReturn,
        Guid purchaseOrderLineId,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        decimal quantity,
        decimal originalUnitPrice,
        decimal depreciationRate,
        decimal taxRate)
    {
        await ValidateReturnQuantityAsync(purchaseReturn.Id, purchaseOrderLineId, quantity, conversionFactor);

        purchaseReturn.AddLine(
            GuidGenerator.Create(),
            purchaseOrderLineId,
            productId,
            unitId,
            conversionFactor,
            quantity,
            originalUnitPrice,
            depreciationRate,
            taxRate
        );
    }

    public async Task UpdateLineAsync(
        PurchaseReturn purchaseReturn,
        Guid lineId,
        decimal quantity,
        decimal depreciationRate)
    {
        PurchaseReturnLine? line = purchaseReturn.Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng chứng từ xuất trả!");
        }

        await ValidateReturnQuantityAsync(purchaseReturn.Id, line.PurchaseOrderLineId, quantity, line.ConversionFactor);

        purchaseReturn.UpdateLine(lineId, quantity, depreciationRate);
    }

    public Task RemoveLineAsync(PurchaseReturn purchaseReturn, Guid lineId)
    {
        purchaseReturn.RemoveLine(lineId);
        return Task.CompletedTask;
    }

    public Task SendToApproveAsync(PurchaseReturn purchaseReturn)
    {
        purchaseReturn.SendToApprove();
        return Task.CompletedTask;
    }

    public async Task<InventoryTicket> ApproveAsync(PurchaseReturn purchaseReturn)
    {
        purchaseReturn.Approve();
        purchaseReturn.StartReturning(); // Chuyển sang trạng thái Returning (Đang xuất trả)

        // 1. Tự động sinh Phiếu xuất kho liên kết (TicketType = ReturnOutward)
        InventoryTicket ticket = await _ticketManager.CreateTicketAsync(
            TicketType.ReturnOutward,
            purchaseReturn.WarehouseId,
            purchaseReturn.Id,
            purchaseReturn.Code,
            $"Phiếu xuất kho trả hàng NCC cho chứng từ {purchaseReturn.Code}"
        );

        // Lưu ticket trước
        await _ticketRepo.InsertAsync(ticket);

        // 2. Tự động tạo các dòng phiếu kho tương ứng
        foreach (PurchaseReturnLine line in purchaseReturn.Lines)
        {
            InventoryTicketLine ticketLine = await _ticketManager.CreateTicketLineAsync(
                ticket,
                line.ProductId,
                line.Id, // Link ReferenceDocumentLineId to PurchaseReturnLine.Id
                line.Quantity,
                line.UnitId,
                line.ConversionFactor
            );
            await _ticketLineRepo.InsertAsync(ticketLine);
        }

        return ticket;
    }

    public Task RejectAsync(PurchaseReturn purchaseReturn)
    {
        purchaseReturn.Reject();
        return Task.CompletedTask;
    }

    public Task CompleteAsync(PurchaseReturn purchaseReturn)
    {
        purchaseReturn.Complete();
        return Task.CompletedTask;
    }

    private async Task ValidateReturnQuantityAsync(
        Guid purchaseReturnId,
        Guid purchaseOrderLineId,
        decimal requestQty,
        int conversionFactor)
    {
        // Lấy tổng số lượng đã xuất trả lũy tiến trước đó (chỉ trừ các phiếu bị Từ chối - Rejected)
        IQueryable<PurchaseReturnLine> lineQuery = await _lineRepo.GetQueryableAsync();
        IQueryable<PurchaseReturnLine> returnedQtyQuery = lineQuery.Where(x =>
            x.PurchaseOrderLineId == purchaseOrderLineId &&
            x.PurchaseReturnId != purchaseReturnId &&
            x.PurchaseReturn.Status != PurchaseReturnStatus.Rejected);

        decimal alreadyReturnedBase = await AsyncExecuter.SumAsync(
            returnedQtyQuery,
            x => x.Quantity * x.ConversionFactor
        );

        PurchaseOrderLine poLine = await _poLineRepo.GetAsync(purchaseOrderLineId);
        decimal requestBase = requestQty * conversionFactor;

        if (alreadyReturnedBase + requestBase > poLine.BaseQuantity)
        {
            throw new BusinessException(
                "SupplyCoreERP:ReturnQuantityExceedsLimit",
                $"Tổng số lượng xuất trả vượt quá số lượng đã nhận trên đơn PO gốc! " +
                $"Đã trả trước đó: {alreadyReturnedBase:N2} (đơn vị gốc), " +
                $"Yêu cầu trả thêm: {requestBase:N2} (đơn vị gốc), " +
                $"Định mức PO tối đa: {poLine.BaseQuantity:N2} (đơn vị gốc)."
            );
        }
    }
}
