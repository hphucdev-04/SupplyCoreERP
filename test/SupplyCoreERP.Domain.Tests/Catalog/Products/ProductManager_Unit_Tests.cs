using System;
using SupplyCoreERP;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using SupplyCoreERP.Sales.Orders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SupplyCoreERP.Catalog.Products;

public class ProductManager_Unit_Tests
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IRepository<PurchaseOrderLine, Guid> _poLineRepo;
    private readonly IRepository<SalesOrderLine, Guid> _soLineRepo;
    private readonly IRepository<PurchaseRequisitionLine, Guid> _prLineRepo;
    private readonly ProductManager _productManager;

    public ProductManager_Unit_Tests()
    {
        _productRepository = Substitute.For<IRepository<Product, Guid>>();
        _balanceRepo = Substitute.For<IRepository<InventoryBalance, Guid>>();
        _ticketLineRepo = Substitute.For<IRepository<InventoryTicketLine, Guid>>();
        _poLineRepo = Substitute.For<IRepository<PurchaseOrderLine, Guid>>();
        _soLineRepo = Substitute.For<IRepository<SalesOrderLine, Guid>>();
        _prLineRepo = Substitute.For<IRepository<PurchaseRequisitionLine, Guid>>();

        _productManager = new ProductManager(
            _productRepository, _balanceRepo, _ticketLineRepo, _poLineRepo, _soLineRepo, _prLineRepo
        );
    }
    [QATest(scenario: "Ném ngoại lệ business ngoại lệ khi check mã code trùng lặp.", feature: "SupplierProduct", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_CheckCode_Duplicate()
    {
        // Arrange
        _productRepository.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(true);

        // Act & Assert
        BusinessException ex = await Assert.ThrowsAsync<BusinessException>(async () =>
        {
            await _productManager.CheckCodeAsync("MED-001");
        });
        ex.Code.ShouldBe("SupplyCoreERP:DuplicateProductCode");
    }
    [QATest(scenario: "Not Ném ngoại lệ khi check mã code is unique.", feature: "SupplierProduct", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Not_Throw_When_CheckCode_Is_Unique()
    {
        // Arrange
        _productRepository.AnyAsync(Arg.Any<Expression<Func<Product, bool>>>()).Returns(false);

        // Act & Assert
        await _productManager.CheckCodeAsync("MED-001");
        await Task.CompletedTask;
    }
    [QATest(scenario: "Return true for has transactions khi balance tồn tại.", feature: "SupplierProduct", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Return_True_For_HasTransactions_When_Balance_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _balanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>()).Returns(true);

        // Act
        var result = await _productManager.HasTransactionsAsync(productId);

        // Assert
        result.ShouldBeTrue();
    }
    [QATest(scenario: "Return true for has transactions khi ticket line tồn tại.", feature: "SupplierProduct", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Return_True_For_HasTransactions_When_TicketLine_Exists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _balanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>()).Returns(false);
        _ticketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>()).Returns(true);

        // Act
        var result = await _productManager.HasTransactionsAsync(productId);

        // Assert
        result.ShouldBeTrue();
    }
    [QATest(scenario: "Return false for has transactions khi no transactions exist.", feature: "SupplierProduct", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Return_False_For_HasTransactions_When_No_Transactions_Exist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _balanceRepo.AnyAsync(Arg.Any<Expression<Func<InventoryBalance, bool>>>()).Returns(false);
        _ticketLineRepo.AnyAsync(Arg.Any<Expression<Func<InventoryTicketLine, bool>>>()).Returns(false);
        _poLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseOrderLine, bool>>>()).Returns(false);
        _soLineRepo.AnyAsync(Arg.Any<Expression<Func<SalesOrderLine, bool>>>()).Returns(false);
        _prLineRepo.AnyAsync(Arg.Any<Expression<Func<PurchaseRequisitionLine, bool>>>()).Returns(false);

        // Act
        var result = await _productManager.HasTransactionsAsync(productId);

        // Assert
        result.ShouldBeFalse();
    }
}
