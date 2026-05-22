using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Inventories.Balances;
using SupplyCoreERP.Inventories.Tickets;
using SupplyCoreERP.Medicines;
using SupplyCoreERP.Orders.PO;
using SupplyCoreERP.Orders.PR;
using SupplyCoreERP.Sales.Orders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SupplyCoreERP.Products;

public class ProductManager_Unit_Tests
{
    private readonly IRepository<Product, Guid> _mockProductRepo;
    private readonly IRepository<InventoryBalance, Guid> _mockBalanceRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _mockTicketLineRepo;
    private readonly IRepository<PurchaseOrderLine, Guid> _mockPoLineRepo;
    private readonly IRepository<SalesOrderLine, Guid> _mockSoLineRepo;
    private readonly IRepository<PurchaseRequisitionLine, Guid> _mockPrLineRepo;
    private readonly ProductManager _productManager;
    private readonly Medicine _product;
    private readonly Guid _baseUnitId;
    private readonly Guid _newUnitId;

    public ProductManager_Unit_Tests()
    {
        // Mock dependencies
        _mockProductRepo = Substitute.For<IRepository<Product, Guid>>();
        _mockBalanceRepo = Substitute.For<IRepository<InventoryBalance, Guid>>();
        _mockTicketLineRepo = Substitute.For<IRepository<InventoryTicketLine, Guid>>();
        _mockPoLineRepo = Substitute.For<IRepository<PurchaseOrderLine, Guid>>();
        _mockSoLineRepo = Substitute.For<IRepository<SalesOrderLine, Guid>>();
        _mockPrLineRepo = Substitute.For<IRepository<PurchaseRequisitionLine, Guid>>();

        // Create Domain Service
        _productManager = new ProductManager(
            _mockProductRepo,
            _mockBalanceRepo,
            _mockTicketLineRepo,
            _mockPoLineRepo,
            _mockSoLineRepo,
            _mockPrLineRepo
        );

        _baseUnitId = Guid.NewGuid();
        _newUnitId = Guid.NewGuid();

        // Create a Medicine (subclass of Product) for testing
        _product = new Medicine(
            Guid.NewGuid(),
            Guid.NewGuid(), // categoryId
            Guid.NewGuid(), // manufacturerId
            "MED-999",
            "Test Medicine",
            _baseUnitId,
            Guid.NewGuid(), // dosageFormId
            "VD-99999-26",
            UsageRoute.Oral,
            StorageCondition.Normal,
            isPrescriptionDrug: false
        );
    }

    [Fact]
    public async Task ValidateBaseUnitChangeAsync_SameBaseUnit_ShouldReturnSilently()
    {
        // Act & Assert
        await _productManager.ValidateBaseUnitChangeAsync(_product, _baseUnitId);

        // Verify that no repository queries were made
        await _mockBalanceRepo.DidNotReceive().AnyAsync(
            Arg.Any<Expression<Func<InventoryBalance, bool>>>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task ValidateBaseUnitChangeAsync_NoTransactions_ShouldReturnSilently()
    {
        // Arrange
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockTicketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPoLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockSoLineRepo.AnyAsync(Arg.Any<Expression<Func<SalesOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPrLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseRequisitionLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act & Assert
        await _productManager.ValidateBaseUnitChangeAsync(_product, _newUnitId);
    }

    [Fact]
    public async Task ValidateBaseUnitChangeAsync_WithInventoryBalance_ShouldThrowCannotChangeBaseUnitWithTransactions()
    {
        // Arrange
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act & Assert
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _productManager.ValidateBaseUnitChangeAsync(_product, _newUnitId);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotChangeBaseUnitWithTransactions");
        exception.Message.ShouldContain("số dư tồn kho");
    }

    [Fact]
    public async Task ValidateBaseUnitChangeAsync_WithInventoryTicketLine_ShouldThrowCannotChangeBaseUnitWithTransactions()
    {
        // Arrange
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockTicketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act & Assert
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _productManager.ValidateBaseUnitChangeAsync(_product, _newUnitId);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotChangeBaseUnitWithTransactions");
        exception.Message.ShouldContain("phiếu kho");
    }

    [Fact]
    public async Task ValidateBaseUnitChangeAsync_WithPurchaseOrderLine_ShouldThrowCannotChangeBaseUnitWithTransactions()
    {
        // Arrange
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockTicketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPoLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act & Assert
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _productManager.ValidateBaseUnitChangeAsync(_product, _newUnitId);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotChangeBaseUnitWithTransactions");
        exception.Message.ShouldContain("dòng đơn mua hàng");
    }

    [Fact]
    public async Task ValidateBaseUnitChangeAsync_WithSalesOrderLine_ShouldThrowCannotChangeBaseUnitWithTransactions()
    {
        // Arrange
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockTicketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPoLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockSoLineRepo.AnyAsync(Arg.Any<Expression<Func<SalesOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act & Assert
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _productManager.ValidateBaseUnitChangeAsync(_product, _newUnitId);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotChangeBaseUnitWithTransactions");
        exception.Message.ShouldContain("dòng đơn bán hàng");
    }

    [Fact]
    public async Task ValidateBaseUnitChangeAsync_WithPurchaseRequisitionLine_ShouldThrowCannotChangeBaseUnitWithTransactions()
    {
        // Arrange
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockTicketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPoLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockSoLineRepo.AnyAsync(Arg.Any<Expression<Func<SalesOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPrLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseRequisitionLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act & Assert
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _productManager.ValidateBaseUnitChangeAsync(_product, _newUnitId);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotChangeBaseUnitWithTransactions");
        exception.Message.ShouldContain("yêu cầu mua hàng");
    }

    [Fact]
    public async Task ValidateUnitChangeAsync_NoTransactions_ShouldReturnSilently()
    {
        // Arrange
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockTicketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPoLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockSoLineRepo.AnyAsync(Arg.Any<Expression<Func<SalesOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPrLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseRequisitionLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act & Assert
        await _productManager.ValidateUnitChangeAsync(_product);
    }

    [Fact]
    public async Task ValidateUnitChangeAsync_WithTransactions_ShouldThrowCannotChangeUnitWithTransactions()
    {
        // Arrange (Chỉ cần mock 1 bảng phát sinh giao dịch bất kỳ, ví dụ: PurchaseOrderLine)
        _mockBalanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockTicketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPoLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true)); // Có giao dịch PO
        _mockSoLineRepo.AnyAsync(Arg.Any<Expression<Func<SalesOrderLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        _mockPrLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseRequisitionLine, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act & Assert
        BusinessException exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _productManager.ValidateUnitChangeAsync(_product);
        });

        exception.Code.ShouldBe("SupplyCoreERP:CannotChangeUnitWithTransactions");
        exception.Message.ShouldContain("đã phát sinh giao dịch lịch sử");
    }
}
