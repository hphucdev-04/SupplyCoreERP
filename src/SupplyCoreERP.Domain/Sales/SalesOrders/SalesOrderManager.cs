using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Common.DocumentSequences;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Partner.Customers;
using SupplyCoreERP.Sales.PriceLists;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Sales.Orders;

public class SalesOrderManager : DomainService, ISalesOrderManager
{
    // Dependencies
    private readonly IRepository<SalesOrder, Guid> _orderRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<Product, Guid> _productRepo;
    private readonly IRepository<Warehouse, Guid> _warehouseRepo;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly PriceManager _priceManager;
    private readonly ITicketManager _ticketManager;
    private readonly IDocumentSequenceManager _documentManager;
    private readonly UnitConversionManager _unitConversionManager;

    // Constructor injection
    public SalesOrderManager(
        IRepository<SalesOrder, Guid> orderRepo,
        IRepository<Customer, Guid> customerRepo,
        IRepository<Product, Guid> productRepo,
        IRepository<Warehouse, Guid> warehouseRepo,
        IRepository<InventoryBalance, Guid> balanceRepo,
        PriceManager priceManager,
        ITicketManager ticketManager,
        IDocumentSequenceManager documentManager,
        UnitConversionManager unitConversionManager
        )
    {
        _orderRepo = orderRepo;
        _customerRepo = customerRepo;
        _productRepo = productRepo;
        _warehouseRepo = warehouseRepo;
        _balanceRepo = balanceRepo;
        _priceManager = priceManager;
        _ticketManager = ticketManager;
        _documentManager = documentManager;
        _unitConversionManager = unitConversionManager;
    }

    #region SaleOrder
    public async Task<SalesOrder> CreateOrderAsync(
        Guid customerId, Guid warehouseId, DateTime orderDate,
        DateTime? expectedDeliveryDate, DateTime? inputDueDate, string? note)
    {
        await ValidateAsync(customerId, warehouseId, orderDate, expectedDeliveryDate, inputDueDate);
        Customer customer = await _customerRepo.GetAsync(customerId);
        if (!customer.IsActive)
        {
            throw new BusinessException("SupplyCoreERP:CustomerLocked", $"Khách hàng '{customer.Name}' đang bị khóa!");
        }

        string code = await _documentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypeSalesOrder);

        DateTime? finalDueDate = inputDueDate
            ?? (customer.PaymentTermDays > 0 ? orderDate.AddDays(customer.PaymentTermDays) : null);

        return new SalesOrder(GuidGenerator.Create(), code, customerId, warehouseId,
                              orderDate, expectedDeliveryDate, finalDueDate, note);
    }

    public async Task UpdateOrderAsync(SalesOrder order, Guid warehouseId,
        DateTime? expectedDeliveryDate, DateTime? dueDate, string? note)
    {
        await ValidateAsync(order.CustomerId, warehouseId, order.OrderDate, expectedDeliveryDate, dueDate);
        order.UpdateInfo(warehouseId, expectedDeliveryDate, dueDate, note);

    }

    public Task CheckBeforeDeleteAsync(SalesOrder order)
    {
        if (order.Status != SalesOrderStatus.Draft)
        {
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Chỉ có thể xóa đơn bán đang ở trạng thái Nháp!");
        }

        return Task.CompletedTask;
    }
    #endregion

    #region SaleOrder Lines
    public async Task AddLineAsync(SalesOrder order, Guid productId, Guid unitId,
        int conversionFactor, decimal quantity, decimal? unitPrice, decimal discountRate, decimal taxRate)
    {
        IQueryable<Product> productQuery = await _productRepo.WithDetailsAsync(p => p.Units);
        Product product = await AsyncExecuter.FirstOrDefaultAsync(productQuery, p => p.Id == productId);
        if (product == null)
        {
            throw new EntityNotFoundException(typeof(Product), productId);
        }

        if (!product.IsAvailableForInventory)
        {
            throw new BusinessException("SupplyCoreERP:ProductNotAvailable", $"Sản phẩm '{product.Name}' chưa đủ điều kiện giao dịch!");
        }

        // Kiểm tra chéo ConversionFactor với GetConversionFactor của hệ thống quy đổi đơn vị tuyến tính
        int expectedAbsoluteFactor = _unitConversionManager.GetConversionFactor(product, unitId);
        if (conversionFactor != expectedAbsoluteFactor)
        {
            throw new BusinessException(
                "SupplyCoreERP:InvalidConversionFactor",
                $"Hệ số quy đổi không hợp lệ so với cấu hình sản phẩm '{product.Name}'. Nhận từ Client: {conversionFactor}, Kỳ vọng hệ số tuyệt đối: {expectedAbsoluteFactor}."
            );
        }

        // Kiểm tra tồn kho khả dụng tổng quát (không quan tâm lô hàng/QA ở bước này)
        decimal requiredBaseQty = _unitConversionManager.ConvertToBaseQuantity(product, unitId, quantity);

        IQueryable<InventoryBalance> balanceQuery = await _balanceRepo.GetQueryableAsync();
        decimal totalAvailable = await AsyncExecuter.SumAsync(
            balanceQuery.Where(x => x.WarehouseId == order.WarehouseId && x.ProductId == productId),
            x => x.Quantity - x.LockedQuantity);

        if (totalAvailable < requiredBaseQty)
        {
            throw new BusinessException("SupplyCoreERP:InsufficientInventory", (
                $"Không đủ tồn kho khả dụng cho '{product.Name}'! " +
                $"Yêu cầu: {requiredBaseQty}, Hiện có: {totalAvailable}."));
        }

        Customer customer = await _customerRepo.GetAsync(order.CustomerId);
        decimal price = unitPrice ?? await _priceManager.GetOfficialPriceAsync(customer.PriceListId, productId, unitId, quantity);

        order.AddLine(GuidGenerator.Create(), productId, unitId, conversionFactor, quantity, price, discountRate, taxRate);
    }

    public async Task UpdateLineAsync(SalesOrder order, Guid lineId,
        decimal quantity, decimal? unitPrice, decimal discountRate, decimal taxRate)
    {
        SalesOrderLine line = order.Lines.FirstOrDefault(x => x.Id == lineId)
            ?? throw new BusinessException("SupplyCoreERP:SalesOrderLineNotFound", "Không tìm thấy dòng chi tiết.");

        Customer customer = await _customerRepo.GetAsync(order.CustomerId);
        decimal newPrice = unitPrice ?? await _priceManager.GetOfficialPriceAsync(
            customer.PriceListId, line.ProductId, line.UnitId, quantity);

        order.UpdateLine(lineId, quantity, newPrice, discountRate, taxRate);
    }

    public Task RemoveLineAsync(SalesOrder order, Guid lineId)
    {
        order.RemoveLine(lineId);
        return Task.CompletedTask;
    }
    #endregion

    #region Workflow
    public Task SendToApproveAsync(SalesOrder order)
    {
        order.SendToApprove();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Duyệt đơn hàng về mặt kinh doanh và tạo phiếu xuất kho Nháp.
    /// </summary>
    /// <returns>Ticket mới tạo (chưa Insert).</returns>
    public async Task<InventoryTicket> ApproveAsync(SalesOrder order)
    {
        if (order.Status != SalesOrderStatus.PendingApproval)
        {
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Đơn hàng chưa được gửi duyệt!");
        }

        if (!order.Lines.Any())
        {
            throw new BusinessException("SupplyCoreERP:NoOrderLines", "Đơn hàng chưa có sản phẩm, không thể duyệt!");
        }

        Customer customer = await _customerRepo.GetAsync(order.CustomerId);

        if (customer.DebtLimit > 0 && (customer.CurrentDebt + order.TotalAmount) > customer.DebtLimit)
        {
            throw new BusinessException("SupplyCoreERP:CustomerDebtLimitExceeded", $"Từ chối duyệt! Khách '{customer.Name}' sẽ vượt hạn mức nợ.");
        }

        DateTime today = DateTime.Now.Date;
        bool hasOverdue = await _orderRepo.AnyAsync(x =>
            x.CustomerId == order.CustomerId &&
            x.Status == SalesOrderStatus.Completed &&
            x.DueDate.HasValue && x.DueDate.Value.Date < today);

        if (hasOverdue)
        {
            throw new BusinessException("SupplyCoreERP:CustomerHasOverdueOrders", "Khách hàng đang có đơn hàng cũ quá hạn. Vui lòng thu hồi nợ trước!");
        }

        List<Guid> productIds = order.Lines.Select(x => x.ProductId).Distinct().ToList();

        // Kiểm tra tồn kho tổng quát (đảm bảo hứa bán được)
        IQueryable<InventoryBalance> balanceQuery = await _balanceRepo.GetQueryableAsync();
        decimal totalAvailable = await AsyncExecuter.SumAsync(
            balanceQuery.Where(x => productIds.Contains(x.ProductId) && x.WarehouseId == order.WarehouseId),
            x => x.Quantity - x.LockedQuantity);

        decimal totalRequired = order.Lines.Sum(x => x.BaseQuantity);

        if (totalAvailable < totalRequired)
        {
            throw new BusinessException("SupplyCoreERP:InsufficientInventory",
                $"Tổng tồn kho khả dụng ({totalAvailable}) không đủ để đáp ứng đơn hàng ({totalRequired}).");
        }

        // Tạo phiếu xuất — chưa Insert (ticket.Id đã có từ GuidGenerator)
        InventoryTicket issueTicket = await _ticketManager.CreateTicketAsync(
            TicketType.GoodsIssue, order.WarehouseId, order.Id, order.Code,
            $"Phiếu xuất tự động từ đơn bán hàng {order.Code}");

        order.Approve();

        return issueTicket;
    }

    /// <summary>
    /// Hoàn tất đơn bán và cộng công nợ cho khách hàng.
    /// </summary>
    /// <returns>Customer đã AddDebt — chưa được lưu DB. AppService gọi UpdateAsync.</returns>
    public async Task<Customer> CompleteAsync(SalesOrder order)
    {
        if (!await _ticketManager.HasStatusAsync(order.Id, ApprovalStatus.Approved))
        {
            throw new BusinessException("SupplyCoreERP:TicketNotApproved", "Phiếu xuất kho chưa được thực thi! Cần hoàn tất xuất kho trước.");
        }

        if (order.Status != SalesOrderStatus.Delivering && order.Status != SalesOrderStatus.Approved)
        {
            throw new BusinessException("SupplyCoreERP:InvalidOrderStatus", "Chỉ có thể Hoàn tất khi đang Giao hoặc Đã duyệt!");
        }

        order.Complete();

        Customer customer = await _customerRepo.GetAsync(order.CustomerId);
        customer.AddDebt(order.TotalAmount);

        return customer;
    }
    #endregion

    #region Validate
    private async Task ValidateAsync(Guid customerId, Guid warehouseId,
       DateTime orderDate, DateTime? expectedDeliveryDate, DateTime? dueDate)
    {
        if (!await _customerRepo.AnyAsync(x => x.Id == customerId))
        {
            throw new BusinessException("SupplyCoreERP:CustomerNotFound", "Khách hàng không tồn tại.");
        }

        if (!await _warehouseRepo.AnyAsync(x => x.Id == warehouseId))
        {
            throw new BusinessException("SupplyCoreERP:WarehouseNotFound", "Kho hàng không tồn tại.");
        }

        if (orderDate.Date > DateTime.Now.Date)
        {
            throw new BusinessException("SupplyCoreERP:InvalidOrderDate", "Ngày đặt hàng không được ở tương lai.");
        }

        if (expectedDeliveryDate.HasValue && expectedDeliveryDate.Value.Date < orderDate.Date)
        {
            throw new BusinessException("SupplyCoreERP:InvalidExpectedDeliveryDate", "Ngày giao hàng dự kiến không được trước ngày đặt hàng.");
        }

        if (dueDate.HasValue && dueDate.Value.Date < orderDate.Date)
        {
            throw new BusinessException("SupplyCoreERP:InvalidDueDate", "Ngày đáo hạn không được trước ngày đặt hàng.");
        }
    }
    #endregion
}
