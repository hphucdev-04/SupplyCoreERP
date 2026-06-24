using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using SupplyCoreERP.SeedData;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Procurement.PurchaseOrders;

public abstract class PurchaseOrderManager_Integration_Tests<TStartupModule> : SupplyCoreERPDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IPurchaseOrderManager _purchaseOrderManager;
    private readonly IRepository<PurchaseOrder, Guid> _orderRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<PurchaseRequisition, Guid> _requisitionRepository;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepository;

    protected PurchaseOrderManager_Integration_Tests()
    {
        _purchaseOrderManager = GetRequiredService<IPurchaseOrderManager>();
        _orderRepository = GetRequiredService<IRepository<PurchaseOrder, Guid>>();
        _supplierRepository = GetRequiredService<IRepository<Supplier, Guid>>();
        _warehouseRepository = GetRequiredService<IRepository<Warehouse, Guid>>();
        _productRepository = GetRequiredService<IRepository<Product, Guid>>();
        _requisitionRepository = GetRequiredService<IRepository<PurchaseRequisition, Guid>>();
        _ticketRepository = GetRequiredService<IRepository<InventoryTicket, Guid>>();
    }

    [QATest(scenario: "Tạo mới PurchaseOrder thông qua Manager thành công.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_PurchaseOrder_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Act
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                DateTime.Now.AddDays(3),
                null,
                "Ghi chu PO test"
            );

            // Assert
            order.ShouldNotBeNull();
            order.Code.ShouldNotBeNullOrWhiteSpace();
            order.SupplierId.ShouldBe(TestDataConsts.SupplierAId);
            order.WarehouseId.ShouldBe(TestDataConsts.WarehouseMainId);
            order.Status.ShouldBe(PurchaseOrderStatus.Draft);

            // Do SupplierA co PaymentTermDays = 30, DueDate tu dong duoc tinh bang OrderDate + 30 days
            order.DueDate.ShouldNotBeNull();
            order.DueDate.Value.Date.ShouldBe(order.OrderDate.AddDays(30).Date);
        });
    }

    [QATest(scenario: "Ném ngoại lệ business khi tạo PurchaseOrder với nhà cung cấp bị khóa.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_BusinessException_When_Creating_With_Inactive_Supplier()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Supplier supplier = await _supplierRepository.GetAsync(TestDataConsts.SupplierAId);
            // Khoa tam thoi nha cung cap
            supplier.SetActive(false);
            await _supplierRepository.UpdateAsync(supplier, autoSave: true);

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _purchaseOrderManager.CreateOrderAsync(
                    TestDataConsts.SupplierAId,
                    TestDataConsts.WarehouseMainId,
                    DateTime.Now,
                    null,
                    null,
                    null
                );
            });
            ex.Code.ShouldBe("SupplyCoreERP:InactiveSupplier");
        });
    }

    [QATest(scenario: "Cập nhật thông tin đơn hàng thành công qua Manager.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Update_PurchaseOrder_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                "Note cu"
            );
            await _orderRepository.InsertAsync(order, autoSave: true);

            DateTime newDelivery = DateTime.Now.AddDays(7);
            DateTime newDue = DateTime.Now.AddDays(20);

            // Act
            await _purchaseOrderManager.UpdateOrderAsync(order, TestDataConsts.WarehouseMainId, newDelivery, newDue, "Note moi");

            // Assert
            PurchaseOrder updatedOrder = await _orderRepository.GetAsync(order.Id);
            updatedOrder.ExpectedDeliveryDate.ShouldBe(newDelivery);
            updatedOrder.DueDate.ShouldBe(newDue);
            updatedOrder.Note.ShouldBe("Note moi");
        });
    }

    [QATest(scenario: "Thêm dòng hàng thành công qua Manager.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Add_Line_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            await _orderRepository.InsertAsync(order, autoSave: true);

            // Act
            await _purchaseOrderManager.AddLineAsync(
                order,
                TestDataConsts.MedicineParacetamolId,
                TestDataConsts.UnitBoxId,
                1,
                100m,
                90000m, // Price khop voi MOQ = 100 trong SupplierTestDataSeedContributor
                0
            );

            // Assert
            order.Lines.Count.ShouldBe(1);
            PurchaseOrderLine line = order.Lines.First();
            line.ProductId.ShouldBe(TestDataConsts.MedicineParacetamolId);
            line.UnitId.ShouldBe(TestDataConsts.UnitBoxId);
            line.Quantity.ShouldBe(100m);
            line.UnitPrice.ShouldBe(90000m);
            order.TotalAmount.ShouldBe(9000000m); // 100 * 90k
        });
    }

    [QATest(scenario: "Ném ngoại lệ business khi thêm dòng hàng với sản phẩm chưa được cấu hình bảng giá.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_Exception_When_Product_Or_Price_Not_Configured()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            await _orderRepository.InsertAsync(order, autoSave: true);

            // Tao 1 product (Medicine) moi hoan toan khong co bang gia voi SupplierA
            Medicine unconfiguredProduct = new(
                Guid.NewGuid(),
                TestDataConsts.CategoryMedicineId,
                TestDataConsts.ManufacturerAId,
                "UNCONFIGURED-CODE",
                "San pham chua cau hinh gia",
                TestDataConsts.UnitBoxId,
                TestDataConsts.DosageTabletId,
                "SDK-UNCONFIGURED",
                Enums.Medicines.UsageRoute.Oral,
                Enums.Medicines.StorageCondition.Normal,
                false
            );
            unconfiguredProduct.Approve();
            await _productRepository.InsertAsync(unconfiguredProduct, autoSave: true);

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _purchaseOrderManager.AddLineAsync(
                    order,
                    unconfiguredProduct.Id,
                    TestDataConsts.UnitBoxId,
                    1,
                    10m,
                    50000m,
                    0
                );
            });
            ex.Code.ShouldBe("SupplyCoreERP:NoPriceConfigured");
        });
    }

    [QATest(scenario: "Ném ngoại lệ business khi thêm dòng hàng với hệ số quy đổi không hợp lệ.", feature: "PurchaseOrder", layer: "Domain", priority: "Medium")]
    [Fact]
    public async Task Should_Throw_Exception_When_ConversionFactor_Is_Invalid()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            await _orderRepository.InsertAsync(order, autoSave: true);

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                // Paracetamol co BaseUnitId la UnitBoxId, nen he so quy doi tuyet doi voi UnitBoxId phai la 1
                // Truyen vao conversionFactor = 10 (khong hop le)
                await _purchaseOrderManager.AddLineAsync(
                    order,
                    TestDataConsts.MedicineParacetamolId,
                    TestDataConsts.UnitBoxId,
                    10,
                    10m,
                    100000m,
                    0
                );
            });
            ex.Code.ShouldBe("SupplyCoreERP:InvalidConversionFactor");
        });
    }

    [QATest(scenario: "Duyệt PurchaseOrder thành công và sinh phiếu kho GoodsReceipt.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Approve_PurchaseOrder_Successfully()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            order.AddLine(Guid.NewGuid(), TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 1, 10m, 100000m, 0);
            await _orderRepository.InsertAsync(order, autoSave: true);

            // Gui duyet truoc
            await _purchaseOrderManager.SendToApproveAsync(order);

            // Act
            InventoryTicket ticket = await _purchaseOrderManager.ApproveAsync(order);

            // Assert
            order.Status.ShouldBe(PurchaseOrderStatus.Approved);
            ticket.ShouldNotBeNull();
            ticket.Type.ShouldBe(TicketType.GoodsReceipt);
            ticket.WarehouseId.ShouldBe(order.WarehouseId);
            ticket.ReferenceDocumentId.ShouldBe(order.Id);
            ticket.ReferenceDocumentNumber.ShouldBe(order.Code);
            ticket.Status.ShouldBe(ApprovalStatus.Draft);
        });
    }

    [QATest(scenario: "Ném ngoại lệ business khi duyệt đơn hàng vượt quá trần nợ của Supplier.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_Exception_When_Approving_Exceeds_DebtLimit()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            Supplier supplier = await _supplierRepository.GetAsync(TestDataConsts.SupplierAId);
            // Gioi han no cuc thap de don hang moi se vuot tran no
            supplier.SetDebtInfo(1000m, supplier.PaymentTermDays);
            await _supplierRepository.UpdateAsync(supplier, autoSave: true);

            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            // Don hang 60 trieu, vuot tran no (450M + 60M = 510M > 500M)
            order.AddLine(Guid.NewGuid(), TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 1, 600m, 100000m, 0);
            await _orderRepository.InsertAsync(order, autoSave: true);

            await _purchaseOrderManager.SendToApproveAsync(order);

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _purchaseOrderManager.ApproveAsync(order);
            });
            ex.Code.ShouldBe("SupplyCoreERP:ExceedsDebtLimit");
        });
    }

    [QATest(scenario: "Ném ngoại lệ business khi duyệt đơn hàng mới nhưng Supplier đang có khoản nợ quá hạn.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_Exception_When_Supplier_Has_Overdue_Orders()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            // 1. Tao mot don hang cu da hoan thanh nhung qua han thanh toan (DueDate < Today)
            PurchaseOrder overdueOrder = new(
                Guid.NewGuid(),
                "PO-OVERDUE-001",
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now.AddDays(-40),
                DateTime.Now.AddDays(-37),
                DateTime.Now.AddDays(-10), // Qua han 10 ngay
                "Don hang bi qua han no"
            );
            overdueOrder.AddLine(Guid.NewGuid(), TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 1, 10m, 100000m, 0);
            overdueOrder.SendToApprove();
            overdueOrder.Approve();
            overdueOrder.Complete();
            await _orderRepository.InsertAsync(overdueOrder, autoSave: true);

            // 2. Tao PO moi
            PurchaseOrder newOrder = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            newOrder.AddLine(Guid.NewGuid(), TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 1, 5m, 100000m, 0);
            await _orderRepository.InsertAsync(newOrder, autoSave: true);
            await _purchaseOrderManager.SendToApproveAsync(newOrder);

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _purchaseOrderManager.ApproveAsync(newOrder);
            });
            ex.Code.ShouldBe("SupplyCoreERP:HasOverdueOrders");
        });
    }

    [QATest(scenario: "Hoàn tất PurchaseOrder thành công, cập nhật công nợ cho Supplier.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Complete_PurchaseOrder_Successfully()
    {
        Guid orderId = Guid.Empty;
        decimal initialDebt = 0;

        // UOW 1: Tao PO, Gui duyet, Duyet PO de sinh Ticket, va thuc thi Ticket (khi chua nhan hang)
        await WithUnitOfWorkAsync(async () =>
        {
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            order.AddLine(Guid.NewGuid(), TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 1, 10m, 100000m, 0); // 1M
            await _orderRepository.InsertAsync(order, autoSave: true);

            await _purchaseOrderManager.SendToApproveAsync(order);
            InventoryTicket ticket = await _purchaseOrderManager.ApproveAsync(order);
            await _ticketRepository.InsertAsync(ticket, autoSave: true);

            // GIA LAP: 1. Phiếu nhập kho được duyệt (Status = Approved)
            ticket.RequestApprove();
            ticket.Execute(new List<InventoryTicketLine>());
            await _ticketRepository.UpdateAsync(ticket, autoSave: true);

            orderId = order.Id;
            Supplier supplier = await _supplierRepository.GetAsync(TestDataConsts.SupplierAId);
            initialDebt = supplier.CurrentDebt;
        }); // Ket thuc UOW 1: Event Handler chay va chuyen PO sang status Receiving (nhan hang)

        // UOW 2: Gia lap nhan du hang va hoan tat PO thu cong
        await WithUnitOfWorkAsync(async () =>
        {
            IQueryable<PurchaseOrder> query = await _orderRepository.WithDetailsAsync(o => o.Lines);
            PurchaseOrder order = await _orderRepository.AsyncExecuter.FirstOrDefaultAsync(query, o => o.Id == orderId);
            order.ShouldNotBeNull();
            order.Lines.First().AddReceivedQuantity(10m);
            await _orderRepository.UpdateAsync(order, autoSave: true);

            Supplier updatedSupplier = await _purchaseOrderManager.CompleteAsync(order);
            await _supplierRepository.UpdateAsync(updatedSupplier, autoSave: true);
        }); // Ket thuc UOW 2

        // UOW 3: Assert ket qua cuoi cung
        await WithUnitOfWorkAsync(async () =>
        {
            IQueryable<PurchaseOrder> query = await _orderRepository.WithDetailsAsync(o => o.Lines);
            PurchaseOrder order = await _orderRepository.AsyncExecuter.FirstOrDefaultAsync(query, o => o.Id == orderId);
            order.ShouldNotBeNull();
            Supplier supplier = await _supplierRepository.GetAsync(TestDataConsts.SupplierAId);

            order.Status.ShouldBe(PurchaseOrderStatus.Completed);
            supplier.CurrentDebt.ShouldBe(initialDebt + order.TotalAmount);
        });
    }

    [QATest(scenario: "Ném ngoại lệ business khi hoàn tất PurchaseOrder nhưng chưa nhận đủ hàng.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_Exception_When_Completing_With_Insufficient_Quantity()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            PurchaseOrder order = await _purchaseOrderManager.CreateOrderAsync(
                TestDataConsts.SupplierAId,
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                null,
                null,
                null
            );
            order.AddLine(Guid.NewGuid(), TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 1, 10m, 100000m, 0);
            await _orderRepository.InsertAsync(order, autoSave: true);

            await _purchaseOrderManager.SendToApproveAsync(order);
            InventoryTicket ticket = await _purchaseOrderManager.ApproveAsync(order);
            await _ticketRepository.InsertAsync(ticket, autoSave: true);

            // Gia lap phieu kho da duyet
            ticket.RequestApprove();
            ticket.Execute(new List<InventoryTicketLine>());
            await _ticketRepository.UpdateAsync(ticket, autoSave: true);

            // Gia lap nhan thieu (moi nhan 5, dat 10)
            order.Lines.First().AddReceivedQuantity(5m);
            await _orderRepository.UpdateAsync(order, autoSave: true);

            // Act & Assert
            BusinessException ex = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _purchaseOrderManager.CompleteAsync(order);
            });
            ex.Code.ShouldBe("SupplyCoreERP:InsufficientQuantity");
        });
    }

    [QATest(scenario: "Tạo tự động PurchaseOrders từ PurchaseRequisition dựa trên phân bổ và MOQ chính xác.", feature: "PurchaseOrder", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_Orders_From_Requisition_With_Correct_MOQ_Prices()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            // Arrange
            // 1. Tao Yeu cau mua hang (PR) va gui duyet
            PurchaseRequisition requisition = new(
                Guid.NewGuid(),
                "PR-TEST-001",
                TestDataConsts.WarehouseMainId,
                DateTime.Now,
                DateTime.Now.AddDays(5),
                "Yeu cau tu phong kham"
            );
            PurchaseRequisitionLine prLine = requisition.AddLine(Guid.NewGuid(), TestDataConsts.MedicineParacetamolId, TestDataConsts.UnitBoxId, 150m, "Dat cho kho");
            requisition.SendToApprove();
            requisition.Approve();
            await _requisitionRepository.InsertAsync(requisition, autoSave: true);

            // 2. Chuan bi thong tin allocations: dat 100m (Khop voi MOQ 100 cua SupplierA co don gia 90k)
            var allocations = new List<(Guid RequisitionLineId, Guid SupplierId, Guid WarehouseId, decimal Quantity)>
            {
                (prLine.Id, TestDataConsts.SupplierAId, TestDataConsts.WarehouseMainId, 100m)
            };

            // Act
            List<PurchaseOrder> orders = await _purchaseOrderManager.CreateOrdersFromRequisitionAsync(
                requisition,
                allocations,
                DateTime.Now,
                "Tạo từ PR"
            );

            // Assert
            orders.Count.ShouldBe(1);
            PurchaseOrder order = orders.First();
            order.SupplierId.ShouldBe(TestDataConsts.SupplierAId);
            order.WarehouseId.ShouldBe(TestDataConsts.WarehouseMainId);
            order.PurchaseRequisitionId.ShouldBe(requisition.Id);
            order.Lines.Count.ShouldBe(1);

            PurchaseOrderLine poLine = order.Lines.First();
            poLine.ProductId.ShouldBe(TestDataConsts.MedicineParacetamolId);
            poLine.Quantity.ShouldBe(100m);
            // Đơn giá áp dụng phải là 90,000đ (do MOQ >= 100, so voi MOQ 1 co gia 100,000đ)
            poLine.UnitPrice.ShouldBe(90000m);

            // Kiem tra OrderedQuantity cua PR duoc cap nhat
            prLine.OrderedQuantity.ShouldBe(100m);
            requisition.Status.ShouldBe(PurchaseRequisitionStatus.PartialOrdered);
        });
    }
}
