using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturns;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Procurement.PurchaseReturnRequests;

public class PurchaseReturnRequestManager : DomainService, IPurchaseReturnRequestManager
{
    private readonly IRepository<PurchaseReturnRequest, Guid> _requestRepo;
    private readonly IRepository<PurchaseReturnRequestLine, Guid> _requestLineRepo;
    private readonly IRepository<Supplier, Guid> _supplierRepo;
    private readonly IRepository<PurchaseOrderLine, Guid> _poLineRepo;
    private readonly IRepository<PurchaseReturnLine, Guid> _returnLineRepo;
    private readonly IPurchaseReturnManager _returnManager;
    private readonly IRepository<PurchaseReturn, Guid> _returnRepo;
    private readonly IDocumentSequenceManager _documentManager;

    public PurchaseReturnRequestManager(
        IRepository<PurchaseReturnRequest, Guid> requestRepo,
        IRepository<PurchaseReturnRequestLine, Guid> requestLineRepo,
        IRepository<Supplier, Guid> supplierRepo,
        IRepository<PurchaseOrderLine, Guid> poLineRepo,
        IRepository<PurchaseReturnLine, Guid> returnLineRepo,
        IPurchaseReturnManager returnManager,
        IRepository<PurchaseReturn, Guid> returnRepo,
        IDocumentSequenceManager documentManager)
    {
        _requestRepo = requestRepo;
        _requestLineRepo = requestLineRepo;
        _supplierRepo = supplierRepo;
        _poLineRepo = poLineRepo;
        _returnLineRepo = returnLineRepo;
        _returnManager = returnManager;
        _returnRepo = returnRepo;
        _documentManager = documentManager;
    }

    public async Task<PurchaseReturnRequest> CreateAsync(
        Guid supplierId,
        Guid warehouseId,
        PurchaseReturnType returnType,
        DateTime requestDate,
        string? note)
    {
        Supplier supplier = await _supplierRepo.GetAsync(supplierId);
        if (!supplier.IsActive)
        {
            throw new BusinessException("SupplyCoreERP:InactiveSupplier", $"Nhà cung cấp '{supplier.Name}' đang bị khóa!");
        }

        // Tự động sinh mã code từ DocumentSequenceManager với sequence code PRQ (hoặc cấu hình tương tự)
        // Chúng ta sẽ đăng ký PRQ làm document type. Nếu chưa có sequence, ta có thể sinh thủ công hoặc dùng document type PRQ.
        string code = await _documentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypePurchaseReturnRequest);

        return new PurchaseReturnRequest(
            GuidGenerator.Create(),
            code,
            supplierId,
            warehouseId,
            returnType,
            requestDate,
            note
        );
    }

    public async Task AddLineAsync(
        PurchaseReturnRequest request,
        Guid productId,
        Guid unitId,
        int conversionFactor,
        Guid purchaseOrderId,
        Guid purchaseOrderLineId,
        decimal quantity,
        decimal originalUnitPrice,
        decimal depreciationRate,
        decimal taxRate)
    {
        await ValidateReturnQuantityAsync(request.Id, purchaseOrderLineId, quantity, conversionFactor);

        request.AddLine(
            GuidGenerator.Create(),
            productId,
            unitId,
            conversionFactor,
            purchaseOrderId,
            purchaseOrderLineId,
            quantity,
            originalUnitPrice,
            depreciationRate,
            taxRate
        );
    }

    public async Task UpdateLineAsync(
        PurchaseReturnRequest request,
        Guid lineId,
        decimal quantity,
        decimal depreciationRate)
    {
        PurchaseReturnRequestLine? line = request.Lines.FirstOrDefault(x => x.Id == lineId);
        if (line == null)
        {
            throw new BusinessException("SupplyCoreERP:LineNotFound", "Không tìm thấy dòng chi tiết yêu cầu!");
        }

        await ValidateReturnQuantityAsync(request.Id, line.PurchaseOrderLineId, quantity, line.ConversionFactor);

        request.UpdateLine(lineId, quantity, depreciationRate);
    }

    public async Task ApproveAndSplitAsync(PurchaseReturnRequest request)
    {
        // 1. Chuyển trạng thái yêu cầu mẹ sang Approved
        request.Approve();

        // 2. Thực hiện thuật toán Grouping & Splitting theo PurchaseOrderId
        IEnumerable<IGrouping<Guid, PurchaseReturnRequestLine>> groups = request.Lines.GroupBy(x => x.PurchaseOrderId);

        foreach (IGrouping<Guid, PurchaseReturnRequestLine> group in groups)
        {
            Guid purchaseOrderId = group.Key;

            // Tạo phiếu PurchaseReturn con liên kết 1-1 với PO này
            PurchaseReturn purchaseReturn = await _returnManager.CreateAsync(
                purchaseOrderId,
                request.SupplierId,
                request.WarehouseId,
                request.RequestDate,
                $"Được tự động phân tách từ Yêu cầu trả hàng {request.Code}"
            );

            // Thiết lập liên kết ngược với yêu cầu trả hàng mẹ
            purchaseReturn.SetRequestRelation(request.Id);

            // Lưu phiếu con
            await _returnRepo.InsertAsync(purchaseReturn);

            // Thêm các dòng chi tiết vào phiếu con
            foreach (PurchaseReturnRequestLine? reqLine in group)
            {
                await _returnManager.AddLineAsync(
                    purchaseReturn,
                    reqLine.PurchaseOrderLineId,
                    reqLine.ProductId,
                    reqLine.UnitId,
                    reqLine.ConversionFactor,
                    reqLine.Quantity,
                    reqLine.OriginalUnitPrice,
                    reqLine.DepreciationRate,
                    reqLine.TaxRate
                );
            }
        }

        // 3. Đánh dấu yêu cầu mẹ là đã xử lý
        request.MarkAsProcessed();
    }

    private async Task ValidateReturnQuantityAsync(
        Guid requestId,
        Guid purchaseOrderLineId,
        decimal requestQty,
        int conversionFactor)
    {
        // 1. Tính tổng số lượng đã xuất trả thực tế trong các phiếu PurchaseReturn con (không bị Rejected)
        IQueryable<PurchaseReturnLine> returnLineQuery = await _returnLineRepo.GetQueryableAsync();
        IQueryable<PurchaseReturnLine> returnedQtyQuery = returnLineQuery.Where(x =>
            x.PurchaseOrderLineId == purchaseOrderLineId &&
            x.PurchaseReturn.Status != PurchaseReturnStatus.Rejected);

        decimal alreadyReturnedBase = await AsyncExecuter.SumAsync(
            returnedQtyQuery,
            x => x.Quantity * x.ConversionFactor
        );

        // 2. Tính tổng số lượng đang nằm trong các phiếu Yêu cầu PRQ nháp/chờ duyệt khác
        IQueryable<PurchaseReturnRequestLine> reqLineQuery = await _requestLineRepo.GetQueryableAsync();
        IQueryable<PurchaseReturnRequestLine> pendingReqQuery = reqLineQuery.Where(x =>
            x.PurchaseOrderLineId == purchaseOrderLineId &&
            x.PurchaseReturnRequestId != requestId &&
            x.PurchaseReturnRequest.Status != PurchaseReturnRequestStatus.Rejected &&
            x.PurchaseReturnRequest.Status != PurchaseReturnRequestStatus.Processed);

        decimal pendingBase = await AsyncExecuter.SumAsync(
            pendingReqQuery,
            x => x.Quantity * x.ConversionFactor
        );

        // 3. Tính số lượng yêu cầu đợt này
        decimal requestBase = requestQty * conversionFactor;

        PurchaseOrderLine poLine = await _poLineRepo.GetAsync(purchaseOrderLineId);

        if (alreadyReturnedBase + pendingBase + requestBase > poLine.BaseQuantity)
        {
            throw new BusinessException(
                "SupplyCoreERP:ReturnQuantityExceedsLimit",
                $"Tổng số lượng yêu cầu trả vượt quá số lượng đã nhận trên đơn PO gốc! " +
                $"Đã trả trước đó: {alreadyReturnedBase:N2} (đơn vị gốc), " +
                $"Đang chờ duyệt trong các yêu cầu khác: {pendingBase:N2} (đơn vị gốc), " +
                $"Yêu cầu lần này: {requestBase:N2} (đơn vị gốc), " +
                $"Định mức PO tối đa: {poLine.BaseQuantity:N2} (đơn vị gốc)."
            );
        }
    }
}
