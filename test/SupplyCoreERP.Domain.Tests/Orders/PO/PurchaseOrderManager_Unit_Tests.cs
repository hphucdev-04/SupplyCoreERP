using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SupplyCoreERP.DocumentSequences;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Inventories.Transactions;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Medicines;
using SupplyCoreERP.Orders.PR;
using SupplyCoreERP.Products;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Suppliers;
using SupplyCoreERP.Warehouses;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Xunit;

namespace SupplyCoreERP.Orders.PO;

public class PurchaseOrderManager_Unit_Tests
{
    private readonly IRepository<PurchaseOrder, Guid> _mockOrderRepo;
    private readonly IRepository<Supplier, Guid> _mockSupplierRepo;
    private readonly IRepository<Product, Guid> _mockProductRepo;
    private readonly IRepository<Warehouse, Guid> _mockWarehouseRepo;
    private readonly IRepository<SupplierProduct, Guid> _mockSupplierProductRepo;
    private readonly IRepository<SupplierProductCondition, Guid> _mockConditionRepo;
    private readonly TicketManager _mockTicketManager;
    private readonly DocumentSequenceManager _mockDocumentManager;
    private readonly UnitConversionManager _mockUnitConversionManager;

    private readonly PurchaseOrderManager _purchaseOrderManager;

    private readonly Guid _supplierId;
    private readonly Guid _warehouseId;
    private readonly Guid _productId;
    private readonly Guid _unitId;
    private readonly Supplier _supplier;
    private readonly Warehouse _warehouse;
    private readonly Medicine _product;

    public PurchaseOrderManager_Unit_Tests()
    {
        // Mock Repositories
        _mockOrderRepo = Substitute.For<IRepository<PurchaseOrder, Guid>>();
        _mockSupplierRepo = Substitute.For<IRepository<Supplier, Guid>>();
        _mockProductRepo = Substitute.For<IRepository<Product, Guid>>();
        _mockWarehouseRepo = Substitute.For<IRepository<Warehouse, Guid>>();
        _mockSupplierProductRepo = Substitute.For<IRepository<SupplierProduct, Guid>>();
        _mockConditionRepo = Substitute.For<IRepository<SupplierProductCondition, Guid>>();

        // Mock DocumentSequenceManager
        _mockDocumentManager = Substitute.For<DocumentSequenceManager>(
            Substitute.For<IRepository<DocumentSequence, Guid>>()
        );

        // Mock UnitConversionManager
        _mockUnitConversionManager = Substitute.For<UnitConversionManager>();

        // Mock TicketManager
        _mockTicketManager = Substitute.For<TicketManager>(
            Substitute.For<IRepository<InventoryTicket, Guid>>(),
            Substitute.For<IRepository<InventoryTicketLine, Guid>>(),
            Substitute.For<IRepository<InventoryTicketDetail, Guid>>(),
            Substitute.For<IRepository<InventoryBalance, Guid>>(),
            Substitute.For<IRepository<ProductBatch, Guid>>(),
            Substitute.For<IRepository<Bin, Guid>>(),
            Substitute.For<IRepository<Warehouse, Guid>>(),
            Substitute.For<IRepository<Product, Guid>>(),
            Substitute.For<IRepository<PurchaseOrder, Guid>>(),
            Substitute.For<IRepository<PurchaseOrderLine, Guid>>(),
            Substitute.For<IRepository<SalesOrder, Guid>>(),
            Substitute.For<IRepository<SalesOrderLine, Guid>>(),
            Substitute.For<WarehouseManager>(
                Substitute.For<IRepository<Warehouse, Guid>>(),
                Substitute.For<IRepository<Zone, Guid>>(),
                Substitute.For<IRepository<Bin, Guid>>(),
                Substitute.For<IRepository<InventoryBalance, Guid>>(),
                _mockDocumentManager
            ),
            Substitute.For<InventoryBalanceManager>(
                Substitute.For<IRepository<InventoryBalance, Guid>>(),
                Substitute.For<IRepository<InventoryReservation, Guid>>(),
                Substitute.For<IRepository<InventoryTransaction, Guid>>()
            ),
            _mockDocumentManager,
            _mockUnitConversionManager
        );

        // Create PurchaseOrderManager
        _purchaseOrderManager = new PurchaseOrderManager(
            _mockOrderRepo,
            _mockSupplierRepo,
            _mockProductRepo,
            _mockWarehouseRepo,
            _mockSupplierProductRepo,
            _mockConditionRepo,
            _mockTicketManager,
            _mockDocumentManager,
            _mockUnitConversionManager
        );
        IAbpLazyServiceProvider lazyServiceProvider = Substitute.For<IAbpLazyServiceProvider>();
        lazyServiceProvider.LazyGetRequiredService(typeof(IGuidGenerator)).Returns(SimpleGuidGenerator.Instance);
        _purchaseOrderManager.LazyServiceProvider = lazyServiceProvider;

        // Seed basic test data
        _supplierId = Guid.NewGuid();
        _warehouseId = Guid.NewGuid();
        _productId = Guid.NewGuid();
        _unitId = Guid.NewGuid();

        _supplier = new Supplier(_supplierId, "SUP-001", "Test Supplier", null, null, null, null, null, null, null, null, null, null, 100000000m, 30);
        _warehouse = new Warehouse(_warehouseId, "WH-001", "Test Warehouse", null, null, null, null);
        _product = new Medicine(
            _productId,
            Guid.NewGuid(), // categoryId
            Guid.NewGuid(), // manufacturerId
            "MED-001",
            "Paracetamol 500mg",
            _unitId, // baseUnitId
            Guid.NewGuid(), // dosageFormId
            "VD-11111-20",
            UsageRoute.Oral,
            StorageCondition.Normal,
            isPrescriptionDrug: false
        );
        _product.Approve();

        // Setup mock default returns
        _mockSupplierRepo.GetAsync(_supplierId).Returns(Task.FromResult(_supplier));
        _mockWarehouseRepo.GetAsync(_warehouseId).Returns(Task.FromResult(_warehouse));
        _mockProductRepo.GetAsync(_productId).Returns(Task.FromResult<Product>(_product));

        _mockSupplierRepo.AnyAsync(Arg.Any<Expression<Func<Supplier, bool>>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        _mockWarehouseRepo.AnyAsync(Arg.Any<Expression<Func<Warehouse, bool>>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
        _mockProductRepo.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
    }

    [Fact]
    public async Task AddLineAsync_ValidSupplierProductCondition_ShouldAddLineWithCorrectPriceAndFactor()
    {
        // Arrange
        var order = new PurchaseOrder(Guid.NewGuid(), "PO-00001", _supplierId, _warehouseId, DateTime.Now, null, null, null, null);

        // Cấu hình mock để kiểm tra sản phẩm và đơn vị có cấu hình điều kiện
        _mockConditionRepo.AnyAsync(Arg.Any<Expression<Func<SupplierProductCondition, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        decimal expectedUnitPrice = 1500m;
        int expectedConversionFactor = 10;
        decimal quantity = 100m;
        decimal taxRate = 0.05m;

        // Act
        await _purchaseOrderManager.AddLineAsync(
            order,
            _productId,
            _unitId,
            expectedConversionFactor,
            quantity,
            expectedUnitPrice,
            taxRate
        );

        // Assert
        order.Lines.Count.ShouldBe(1);
        PurchaseOrderLine addedLine = order.Lines.First();
        addedLine.ProductId.ShouldBe(_productId);
        addedLine.UnitId.ShouldBe(_unitId);
        addedLine.ConversionFactor.ShouldBe(expectedConversionFactor);
        addedLine.Quantity.ShouldBe(quantity);
        addedLine.UnitPrice.ShouldBe(expectedUnitPrice);
        addedLine.TaxRate.ShouldBe(taxRate);
    }

    [Fact]
    public async Task AddLineAsync_NoSupplierProductCondition_ShouldThrowUserFriendlyException()
    {
        // Arrange
        var order = new PurchaseOrder(Guid.NewGuid(), "PO-00001", _supplierId, _warehouseId, DateTime.Now, null, null, null, null);

        // Mock AnyAsync trả về false -> Không cấu hình bảng giá cho đơn vị này
        _mockConditionRepo.AnyAsync(Arg.Any<Expression<Func<SupplierProductCondition, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act & Assert
        UserFriendlyException exception = await Should.ThrowAsync<UserFriendlyException>(async () =>
        {
            await _purchaseOrderManager.AddLineAsync(
                order,
                _productId,
                _unitId,
                1,
                100m,
                1000m,
                0m
            );
        });

        exception.Message.ShouldContain("chưa được cấu hình bảng giá");
        order.Lines.ShouldBeEmpty();
    }

    [Fact]
    public async Task CreateOrdersFromRequisitionAsync_ValidConditions_ShouldCreatePurchaseOrdersWithCorrectPricesAndConversionFactors()
    {
        // Arrange
        var prId = Guid.NewGuid();
        var prLineId = Guid.NewGuid();
        var requisition = new PurchaseRequisition(prId, "PR-00001", _warehouseId, DateTime.Now, DateTime.Now.AddDays(5), "Test PR");

        requisition.AddLine(prLineId, _productId, _unitId, 100m, "Need immediately");
        requisition.SendToApprove();
        requisition.Approve(); // Chuyển trạng thái sang Approved để được phép tạo PO

        var supplierProductId = Guid.NewGuid();
        var supplierProduct = new SupplierProduct(supplierProductId, _supplierId, _productId, _unitId, 5, true, "SP-CODE-001");

        var condition = new SupplierProductCondition(Guid.NewGuid(), supplierProductId, _unitId, 12, 2000m, 10m, 0m, 0m);

        // Mock repository tra cứu SupplierProduct và Condition qua GetQueryableAsync
        _mockSupplierProductRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<SupplierProduct> { supplierProduct }.AsQueryable()));

        // Mock repository tra cứu SupplierProductCondition qua GetQueryableAsync
        _mockConditionRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<SupplierProductCondition> { condition }.AsQueryable()));

        // Mock AsyncExecuter cho repository để các extension method như FirstOrDefaultAsync hoạt động chính xác
        IAsyncQueryableExecuter mockAsyncExecuter = Substitute.For<IAsyncQueryableExecuter>();
        _mockSupplierProductRepo.AsyncExecuter.Returns(mockAsyncExecuter);
        _mockConditionRepo.AsyncExecuter.Returns(mockAsyncExecuter);

        mockAsyncExecuter.FirstOrDefaultAsync(
            Arg.Any<IQueryable<SupplierProduct>>(),
            Arg.Any<Expression<Func<SupplierProduct, bool>>>(),
            Arg.Any<CancellationToken>()
        )
        .Returns(Task.FromResult<SupplierProduct?>(supplierProduct));

        mockAsyncExecuter.ToListAsync(
            Arg.Any<IQueryable<SupplierProductCondition>>(),
            Arg.Any<CancellationToken>()
        )
        .Returns(Task.FromResult(new List<SupplierProductCondition> { condition }));

        // Mock DocumentManager phát sinh mã PO
        _mockDocumentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypePurchaseOrder).Returns(Task.FromResult("PO-Generated-001"));

        var allocations = new List<(Guid RequisitionLineId, Guid SupplierId, Guid WarehouseId, decimal Quantity)>
        {
            (prLineId, _supplierId, _warehouseId, 50m)
        };

        // Act
        List<PurchaseOrder> createdOrders = await _purchaseOrderManager.CreateOrdersFromRequisitionAsync(
            requisition,
            allocations,
            DateTime.Now,
            "PO from PR"
        );

        // Assert
        createdOrders.ShouldNotBeNull();
        createdOrders.Count.ShouldBe(1);

        PurchaseOrder po = createdOrders.First();
        po.Code.ShouldBe("PO-Generated-001");
        po.SupplierId.ShouldBe(_supplierId);
        po.WarehouseId.ShouldBe(_warehouseId);
        po.PurchaseRequisitionId.ShouldBe(prId);

        po.Lines.Count.ShouldBe(1);
        PurchaseOrderLine poLine = po.Lines.First();
        poLine.ProductId.ShouldBe(_productId);
        poLine.UnitId.ShouldBe(_unitId);
        poLine.UnitPrice.ShouldBe(condition.StandardPrice); // 2000m từ bảng con Condition
        poLine.ConversionFactor.ShouldBe(condition.ConversionFactor); // 12 từ bảng con Condition
        poLine.Quantity.ShouldBe(50m);

        // PR Line OrderedQuantity phải tăng thêm 50
        PurchaseRequisitionLine prLine = requisition.Lines.First();
        prLine.OrderedQuantity.ShouldBe(50m);
    }

    [Theory]
    [InlineData(5, 1000)]   // Lượng 5 < MOQ thấp nhất (10) -> Áp dụng MOQ thấp nhất (10) với giá 1000
    [InlineData(10, 1000)]  // Lượng 10 = MOQ 10 -> Áp dụng MOQ 10 với giá 1000
    [InlineData(25, 1000)]  // MOQ 10 < Lượng 25 < MOQ 50 -> Áp dụng MOQ 10 với giá 1000
    [InlineData(50, 900)]   // Lượng 50 = MOQ 50 -> Áp dụng MOQ 50 với giá 900
    [InlineData(75, 900)]   // MOQ 50 < Lượng 75 < MOQ 100 -> Áp dụng MOQ 50 với giá 900
    [InlineData(100, 800)]  // Lượng 100 = MOQ 100 -> Áp dụng MOQ 100 với giá 800
    [InlineData(150, 800)]  // Lượng 150 > MOQ 100 -> Áp dụng MOQ 100 với giá 800
    public async Task CreateOrdersFromRequisitionAsync_TieredPricing_ShouldSelectCorrectTieredPriceBasedOnQuantity(
        decimal quantity,
        decimal expectedPrice)
    {
        // Arrange
        var prId = Guid.NewGuid();
        var prLineId = Guid.NewGuid();
        var requisition = new PurchaseRequisition(prId, "PR-00001", _warehouseId, DateTime.Now, DateTime.Now.AddDays(5), "Test PR");

        requisition.AddLine(prLineId, _productId, _unitId, quantity, "Need immediately");
        requisition.SendToApprove();
        requisition.Approve();

        var supplierProductId = Guid.NewGuid();
        var supplierProduct = new SupplierProduct(supplierProductId, _supplierId, _productId, _unitId, 5, true, "SP-CODE-001");

        // Tạo 3 mốc MOQ phân cấp
        var cond1 = new SupplierProductCondition(Guid.NewGuid(), supplierProductId, _unitId, 1, 1000m, 10m);
        var cond2 = new SupplierProductCondition(Guid.NewGuid(), supplierProductId, _unitId, 1, 900m, 50m);
        var cond3 = new SupplierProductCondition(Guid.NewGuid(), supplierProductId, _unitId, 1, 800m, 100m);

        supplierProduct.AddCondition(cond1);
        supplierProduct.AddCondition(cond2);
        supplierProduct.AddCondition(cond3);

        // Mock repository tra cứu SupplierProduct và Condition qua GetQueryableAsync
        _mockSupplierProductRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<SupplierProduct> { supplierProduct }.AsQueryable()));

        _mockConditionRepo.GetQueryableAsync()
            .Returns(Task.FromResult(new List<SupplierProductCondition> { cond1, cond2, cond3 }.AsQueryable()));

        // Mock AsyncExecuter
        IAsyncQueryableExecuter mockAsyncExecuter = Substitute.For<IAsyncQueryableExecuter>();
        _mockSupplierProductRepo.AsyncExecuter.Returns(mockAsyncExecuter);
        _mockConditionRepo.AsyncExecuter.Returns(mockAsyncExecuter);

        mockAsyncExecuter.FirstOrDefaultAsync(
            Arg.Any<IQueryable<SupplierProduct>>(),
            Arg.Any<Expression<Func<SupplierProduct, bool>>>(),
            Arg.Any<CancellationToken>()
        )
        .Returns(Task.FromResult<SupplierProduct?>(supplierProduct));

        mockAsyncExecuter.ToListAsync(
            Arg.Any<IQueryable<SupplierProductCondition>>(),
            Arg.Any<CancellationToken>()
        )
        .Returns(Task.FromResult(new List<SupplierProductCondition> { cond1, cond2, cond3 }));

        // Mock DocumentManager phát sinh mã PO
        _mockDocumentManager.GenerateAsync(SupplyCoreERPConsts.DocumentTypePurchaseOrder).Returns(Task.FromResult("PO-Generated-001"));

        var allocations = new List<(Guid RequisitionLineId, Guid SupplierId, Guid WarehouseId, decimal Quantity)>
        {
            (prLineId, _supplierId, _warehouseId, quantity)
        };

        // Act
        List<PurchaseOrder> createdOrders = await _purchaseOrderManager.CreateOrdersFromRequisitionAsync(
            requisition,
            allocations,
            DateTime.Now,
            "PO from PR"
        );

        // Assert
        createdOrders.ShouldNotBeNull();
        createdOrders.Count.ShouldBe(1);

        PurchaseOrder po = createdOrders.First();
        po.Lines.Count.ShouldBe(1);
        PurchaseOrderLine poLine = po.Lines.First();
        poLine.UnitPrice.ShouldBe(expectedPrice);
        poLine.Quantity.ShouldBe(quantity);
    }
}
