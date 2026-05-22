using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Orders.PR;
using SupplyCoreERP.Products;
using SupplyCoreERP.Suppliers;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Orders.PO;

public class PurchaseOrderManager : DomainService
{
    // Dependencies
    private readonly IRepository<PurchaseOrder, Guid> _orderRepo;
    private readonly IRepository<Supplier, Guid> _supplierRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly IRepository<SupplierProduct, Guid> _supplierProductRepo;
    private readonly IRepository<SupplierProductCondition, Guid> _conditionRepo;
    private readonly TicketManager _ticketManager;
    private readonly DocumentSequenceManager _documentManager;
    private readonly UnitConversionManager _unitConversionManager;

    // DI
    public PurchaseOrderManager(
        IRepository<PurchaseOrder, Guid> orderRepo,
        IRepository<Supplier, Guid> supplierRepo,
        IRepository<Product, Guid> productRepo,
        IRepository<Warehouse, Guid> warehouseRepo,
        IRepository<SupplierProduct, Guid> supplierProductRepo,
        IRepository<SupplierProductCondition, Guid> conditionRepo,
        TicketManager ticketManager,
        DocumentSequenceManager documentManager,
        UnitConversionManager unitConversionManager
    )
    {
        _orderRepo = orderRepo;
        _supplierRepo = supplierRepo;
        _productRepo = productRepo;
        _warehouseRepo = warehouseRepo;
        _supplierProductRepo = supplierProductRepo;
        _conditionRepo = conditionRepo;
        _ticketManager = ticketManager;
        _documentManager = documentManager;
        _unitConversionManager = unitConversionManager;
    }

    #region PurchaseOrder
    public async Task<PurchaseOrder> CreateOrderAsync(
     Guid supplierId, Guid warehouseId, DateTime orderDate,
     DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note,
     Guid? purchaseRequisitionId = null)
    {
        await ValidateAsync(supplierId, warehouseId, orderDate, expectedDeliveryDate, inputDueDate);

        Supplier supplier = await _supplierRepo.GetAsync(supplierId);
        if (!supplier.IsActive)
        {
            throw new UserFriendlyException($"Nhà cung cấp '{supplier.Name}' đang bị khóa!");
        }

        string code = await _documentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypePurchaseOrder);

        DateTime? finalDueDate = inputDueDate
            ?? (supplier.PaymentTermDays > 0 ? orderDate.AddDays(supplier.PaymentTermDays) : null);

        return new PurchaseOrder(GuidGenerator.Create(), code, supplierId, warehouseId,
                                 orderDate, expectedDeliveryDate, finalDueDate, note, purchaseRequisitionId);
    }

    /// <summary>
    /// Tạo danh sách các Đơn mua hàng (PO) từ Yêu cầu mua hàng (PR) dựa trên phân bổ chi tiết.
    /// </summary>
    public async Task<List<PurchaseOrder>> CreateOrdersFromRequisitionAsync(
        PurchaseRequisition requisition,
        List<(Guid RequisitionLineId, Guid SupplierId, Guid WarehouseId, decimal Quantity)> allocations,
        DateTime orderDate,
        string? note)
    {
        if (requisition.Status != PurchaseRequisitionStatus.Approved &&
            requisition.Status != PurchaseRequisitionStatus.PartialOrdered)
        {
            throw new UserFriendlyException("Yêu cầu mua hàng chưa được duyệt hoặc đã hoàn tất.");
        }

        if (!allocations.Any())
        {
            throw new UserFriendlyException("Vui lòng cung cấp thông tin phân bổ sản phẩm.");
        }

        var createdOrders = new List<PurchaseOrder>();

        // Nhóm theo (SupplierId, WarehouseId) để tạo PO
        var groups = allocations.GroupBy(x => new { x.SupplierId, x.WarehouseId });

        foreach (var group in groups)
        {
            // Tự động lấy ngày cần hàng từ PR
            DateTime? expectedDeliveryDate = requisition.RequiredDate;

            // Tạo PO thông qua CreateOrderAsync để tự động tính DueDate từ NCC
            PurchaseOrder order = await CreateOrderAsync(
                group.Key.SupplierId,
                group.Key.WarehouseId,
                orderDate,
                expectedDeliveryDate,
                null,
                $"[Từ PR: {requisition.Code}] " + (note ?? ""),
                requisition.Id); // Gắn PR Id để truy vết

            Supplier supplier = await _supplierRepo.GetAsync(group.Key.SupplierId);

            foreach ((Guid RequisitionLineId, Guid SupplierId, Guid WarehouseId, decimal Quantity) allocation in group)
            {
                PurchaseRequisitionLine? prLine = requisition.Lines.FirstOrDefault(x => x.Id == allocation.RequisitionLineId);
                if (prLine == null)
                {
                    continue;
                }

                // Nạp thông tin sản phẩm để lấy tên (phục vụ hiển thị lỗi)
                Product product = await _productRepo.GetAsync(prLine.ProductId);

                SupplierProduct? supplierProduct = await _supplierProductRepo.FirstOrDefaultAsync(sp =>
                    sp.SupplierId == group.Key.SupplierId &&
                    sp.ProductId == prLine.ProductId &&
                    sp.IsActive);

                if (supplierProduct == null)
                {
                    throw new UserFriendlyException($"Sản phẩm '{product.Name}' không được cung cấp bởi '{supplier.Name}'.");
                }

                // Lấy toàn bộ các mức giá phân cấp của sản phẩm khớp đơn vị tính
                IQueryable<SupplierProductCondition> conditionsQuery = (await _conditionRepo.GetQueryableAsync())
                    .Where(c => c.SupplierProductId == supplierProduct.Id && c.UnitId == prLine.UnitId);
                List<SupplierProductCondition> conditions = await _conditionRepo.AsyncExecuter.ToListAsync(conditionsQuery);

                if (conditions == null || !conditions.Any())
                {
                    throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa được cấu hình bảng giá cho đơn vị tính yêu cầu với nhà cung cấp '{supplier.Name}'.");
                }

                // Sắp xếp theo MOQ tăng dần
                var sortedConditions = conditions.OrderBy(c => c.MinOrderQuantity).ToList();

                SupplierProductCondition condition;

                // Lọc các mức giá thỏa mãn MOQ dựa trên số lượng đặt thực tế (allocation.Quantity)
                var eligibleConditions = sortedConditions.Where(c => allocation.Quantity >= c.MinOrderQuantity).ToList();

                if (eligibleConditions.Any())
                {
                    condition = eligibleConditions.Last(); // Lấy mức giá ưu đãi nhất thỏa mãn MOQ
                }
                else
                {
                    condition = sortedConditions.First(); // Phương án B: Áp mức giá của MOQ thấp nhất
                }

                // Kiểm tra số lượng hợp lệ (không vượt quá số lượng còn lại trong yêu cầu)
                decimal remainingQty = prLine.Quantity - prLine.OrderedQuantity;
                if (allocation.Quantity > remainingQty + 0.0001m) // Chấp nhận sai số nhỏ do làm tròn
                {
                    throw new UserFriendlyException($"Sản phẩm '{product.Name}' yêu cầu đặt {allocation.Quantity} nhưng thực tế chỉ còn thiếu {remainingQty}.");
                }

                if (allocation.Quantity <= 0)
                {
                    continue;
                }

                // Thêm line vào PO với số lượng do người dùng nhập
                order.AddLine(
                    GuidGenerator.Create(),
                    prLine.ProductId,
                    prLine.UnitId,
                    condition.ConversionFactor,
                    allocation.Quantity,
                    condition.StandardPrice,
                    0 // TaxRate mặc định là 0
                );

                // Cập nhật số lượng đã đặt trên PR Line
                prLine.AddOrderedQuantity(allocation.Quantity);
            }

            if (order.Lines.Any())
            {
                createdOrders.Add(order);
            }
        }

        if (!createdOrders.Any())
        {
            throw new UserFriendlyException("Không có sản phẩm nào hợp lệ để tạo đơn hàng.");
        }

        // Cập nhật trạng thái PR
        requisition.UpdateOrderingStatus();

        return createdOrders;
    }

    public async Task UpdateOrderAsync(PurchaseOrder order, Guid warehouseId,
        DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
    {
        await ValidateAsync(order.SupplierId, warehouseId, order.OrderDate, expectedDeliveryDate, dueDate);
        order.UpdateInfo(warehouseId, expectedDeliveryDate, dueDate, note);
    }

    public Task CheckBeforeDeleteAsync(PurchaseOrder order)
    {
        if (order.Status != PurchaseOrderStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ có thể xóa đơn hàng đang ở trạng thái Nháp!");
        }

        return Task.CompletedTask;
    }
    #endregion

    #region PurchaseOrder Lines
    public async Task AddLineAsync(PurchaseOrder order, Guid productId, Guid unitId,
        int conversionFactor, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        Product product = await _productRepo.GetAsync(productId);
        if (!product.IsAvailableForInventory)
        {
            throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa đủ điều kiện giao dịch!");
        }

        // Ràng buộc sản phẩm và đơn vị tính theo nhà cung cấp
        bool isProvidedBySupplier = await _conditionRepo.AnyAsync(c =>
            c.UnitId == unitId &&
            c.SupplierProduct.SupplierId == order.SupplierId &&
            c.SupplierProduct.ProductId == productId &&
            c.SupplierProduct.IsActive);

        if (!isProvidedBySupplier)
        {
            Supplier supplier = await _supplierRepo.GetAsync(order.SupplierId);
            throw new UserFriendlyException($"Sản phẩm '{product.Name}' chưa được cấu hình bảng giá cho đơn vị tính được chọn với nhà cung cấp '{supplier.Name}'!");
        }

        order.AddLine(GuidGenerator.Create(), productId, unitId, conversionFactor, quantity, unitPrice, taxRate);
    }

    public Task UpdateLineAsync(PurchaseOrder order, Guid lineId,
        decimal quantity, decimal unitPrice, decimal taxRate)
    {
        order.UpdateLine(lineId, quantity, unitPrice, taxRate);
        return Task.CompletedTask;
    }

    public Task RemoveLineAsync(PurchaseOrder order, Guid lineId)
    {
        order.RemoveLine(lineId);
        return Task.CompletedTask;
    }
    #endregion

    #region Work flow
    public Task SendToApproveAsync(PurchaseOrder order)
    {
        if (order.Status != PurchaseOrderStatus.Draft)
        {
            throw new UserFriendlyException("Chỉ có thể gửi duyệt đơn đang ở trạng thái Nháp!");
        }

        order.SendToApprove();
        return Task.CompletedTask;
    }


    public async Task<InventoryTicket> ApproveAsync(PurchaseOrder order)
    {
        if (order.Status != PurchaseOrderStatus.PendingApproval)
        {
            throw new UserFriendlyException("Đơn hàng chưa được gửi duyệt!");
        }

        Supplier supplier = await _supplierRepo.GetAsync(order.SupplierId);

        if (supplier.DebtLimit > 0 && (supplier.CurrentDebt + order.TotalAmount) > supplier.DebtLimit)
        {
            throw new UserFriendlyException(
                $"Từ chối duyệt! Đơn ({order.TotalAmount:N0}đ) sẽ vượt trần nợ với '{supplier.Name}'.");
        }

        DateTime today = DateTime.Now.Date;
        bool hasOverdue = await _orderRepo.AnyAsync(x =>
            x.SupplierId == order.SupplierId &&
            x.Status == PurchaseOrderStatus.Completed &&
            x.DueDate.HasValue && x.DueDate.Value.Date < today);

        if (hasOverdue)
        {
            throw new UserFriendlyException($"'{supplier.Name}' đang có khoản nợ quá hạn. Xử lý nợ cũ trước!");
        }

        Guid[] productIds = order.Lines.Select(x => x.ProductId).Distinct().ToArray();
        List<Product> products = await _productRepo.GetListAsync(x => productIds.Contains(x.Id));

        foreach (PurchaseOrderLine item in order.Lines)
        {
            Product? product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null || !product.IsAvailableForInventory)
            {
                throw new UserFriendlyException($"Sản phẩm '{product?.Name}' không còn đủ điều kiện giao dịch!");
            }
        }

        // Tạo phiếu nhập kho — KHÔNG InsertAsync tại đây
        InventoryTicket receiptTicket = await _ticketManager.CreateTicketAsync(
            TicketType.GoodsReceipt, order.WarehouseId, order.Id, order.Code,
            $"Phiếu chờ nhập kho cho Đơn mua hàng {order.Code}");

        order.Approve();

        return receiptTicket;
    }

    /// <summary>
    /// Hoàn tất đơn mua và cộng công nợ cho nhà cung cấp.
    /// </summary>
    /// <returns>Supplier đã AddDebt — chưa được lưu DB. AppService gọi UpdateAsync.</returns>
    public async Task<Supplier> CompleteAsync(PurchaseOrder order)
    {
        if (order.Status == PurchaseOrderStatus.Completed)
        {
            throw new UserFriendlyException("Đơn hàng đã được hoàn tất trước đó.");
        }

        // 1. Kiểm tra trạng thái phiếu kho liên quan
        if (!await _ticketManager.HasStatusAsync(order.Id, ApprovalStatus.Approved))
        {
            throw new UserFriendlyException("Chưa có phiếu nhập kho nào được thực thi! Cần thực hiện nhập kho trước khi hoàn tất đơn.");
        }

        // 2. BẮT BUỘC: Kiểm tra tất cả các dòng đã nhận đủ hàng
        foreach (PurchaseOrderLine line in order.Lines)
        {
            // So sánh theo BaseQuantity để tránh sai số làm tròn ở đơn vị PO
            Product product = await _productRepo.GetAsync(line.ProductId);
            decimal receivedBaseQuantity = _unitConversionManager.ConvertToBaseQuantity(product, line.UnitId, line.ReceivedQuantity);
            if (receivedBaseQuantity < line.BaseQuantity - 0.0001m)
            {
                throw new UserFriendlyException(
                    $"Sản phẩm '{line.Product?.Name}' mới nhận được {line.ReceivedQuantity} {line.Unit?.Name}, " +
                    $"còn thiếu {line.Quantity - line.ReceivedQuantity}. Không thể hoàn tất đơn hàng khi chưa nhận đủ!");
            }
        }

        if (order.Status != PurchaseOrderStatus.Receiving && order.Status != PurchaseOrderStatus.Approved)
        {
            throw new UserFriendlyException("Chỉ có thể Hoàn tất khi đang ở trạng thái Nhập kho hoặc Đã duyệt!");
        }

        order.Complete();

        Supplier supplier = await _supplierRepo.GetAsync(order.SupplierId);
        supplier.AddDebt(order.TotalAmount);

        return supplier;
    }
    #endregion

    #region Validate
    private async Task ValidateAsync(Guid supplierId, Guid warehouseId,
        DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? dueDate)
    {
        if (!await _supplierRepo.AnyAsync(x => x.Id == supplierId))
        {
            throw new UserFriendlyException("Nhà cung cấp không tồn tại.");
        }

        if (!await _warehouseRepo.AnyAsync(x => x.Id == warehouseId))
        {
            throw new UserFriendlyException("Kho hàng không tồn tại.");
        }

        if (orderDate.Date > DateTime.Now.Date)
        {
            throw new UserFriendlyException("Ngày đặt hàng không được ở tương lai.");
        }

        if (expectedDeliveryDate.HasValue && expectedDeliveryDate.Value.Date < orderDate.Date)
        {
            throw new UserFriendlyException("Ngày giao hàng dự kiến không được trước ngày đặt hàng.");
        }

        if (dueDate.HasValue && dueDate.Value.Date < orderDate.Date)
        {
            throw new UserFriendlyException("Ngày đáo hạn không được trước ngày đặt hàng.");
        }
    }
    #endregion
}
