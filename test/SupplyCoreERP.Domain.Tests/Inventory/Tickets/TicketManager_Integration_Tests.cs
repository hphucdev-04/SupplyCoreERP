using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Medicines;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.SeedData;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace SupplyCoreERP.Inventory.Tickets;

public abstract class TicketManager_Integration_Tests<TStartupModule> : SupplyCoreERPDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ITicketManager _ticketManager;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepository;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepository;
    private readonly IRepository<InventoryTicketDetail, Guid> _ticketDetailRepository;
    private readonly IRepository<ProductBatch, Guid> _batchRepository;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepository;
    private readonly IRepository<Zone, Guid> _zoneRepository;
    private readonly IRepository<Bin, Guid> _binRepository;

    protected TicketManager_Integration_Tests()
    {
        _ticketManager = GetRequiredService<ITicketManager>();
        _ticketRepository = GetRequiredService<IRepository<InventoryTicket, Guid>>();
        _ticketLineRepository = GetRequiredService<IRepository<InventoryTicketLine, Guid>>();
        _ticketDetailRepository = GetRequiredService<IRepository<InventoryTicketDetail, Guid>>();
        _batchRepository = GetRequiredService<IRepository<ProductBatch, Guid>>();
        _balanceRepository = GetRequiredService<IRepository<InventoryBalance, Guid>>();
        _zoneRepository = GetRequiredService<IRepository<Zone, Guid>>();
        _binRepository = GetRequiredService<IRepository<Bin, Guid>>();
    }

    [QATest(scenario: "Phân bổ xuất kho FEFO ưu tiên ngày hết hạn trước và fallback qua ngày sản xuất cũ hơn khi ngày hết hạn trùng nhau.", feature: "FEFO", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Allocate_Stock_Using_FEFO_With_ManufacturingDate_Fallback()
    {
        Guid lineId = Guid.Empty;
        Guid batchCId = Guid.NewGuid();
        Guid batchBId = Guid.NewGuid();
        Guid batchAId = Guid.NewGuid();
        Guid binId = Guid.NewGuid();

        // UOW 1: Thiet lap du lieu, tao phieu xuat va phan bo FEFO
        await WithUnitOfWorkAsync(async () =>
        {
            // 1. Arrange: Thiết lập cấu trúc kho (Zone, Bin)
            Guid zoneId = Guid.NewGuid();
            Zone zone = new(
                zoneId,
                TestDataConsts.WarehouseMainId,
                "ZONE-STOR",
                "Khu lưu trữ chính",
                ZoneType.Storage,
                StorageCondition.Normal,
                "#FFFFFF",
                0, 0, 10, 10, 0
            );
            await _zoneRepository.InsertAsync(zone, autoSave: true);

            Bin bin = new(
                binId,
                TestDataConsts.WarehouseMainId,
                zoneId,
                "BIN-A1",
                0, 0, 100, 100, 0,
                10, 10
            );
            await _binRepository.InsertAsync(bin, autoSave: true);

            // 2. Tạo 3 Lô hàng của Paracetamol
            // Lô C: Hết hạn trước tiên (2026-10-31)
            ProductBatch batchC = new(batchCId, "LOTC", TestDataConsts.MedicineParacetamolId, "LOT-C", DateTime.Parse("2023-07-24"), DateTime.Parse("2026-10-31"), TestDataConsts.SupplierAId);
            batchC.ApproveQA();
            await _batchRepository.InsertAsync(batchC, autoSave: true);

            // Lô B: Hết hạn sau (2026-12-31), Ngày sản xuất cũ hơn (2023-05-24)
            ProductBatch batchB = new(batchBId, "LOTB", TestDataConsts.MedicineParacetamolId, "LOT-B", DateTime.Parse("2023-05-24"), DateTime.Parse("2026-12-31"), TestDataConsts.SupplierAId);
            batchB.ApproveQA();
            await _batchRepository.InsertAsync(batchB, autoSave: true);

            // Lô A: Hết hạn sau (2026-12-31), Ngày sản xuất mới hơn (2023-06-24)
            ProductBatch batchA = new(batchAId, "LOTA", TestDataConsts.MedicineParacetamolId, "LOT-A", DateTime.Parse("2023-06-24"), DateTime.Parse("2026-12-31"), TestDataConsts.SupplierAId);
            batchA.ApproveQA();
            await _batchRepository.InsertAsync(batchA, autoSave: true);

            // 3. Tạo số dư tồn kho (InventoryBalance) cho 3 lô hàng này
            // Lô C: 5 hộp trong kho
            InventoryBalance balC = new(Guid.NewGuid(), TestDataConsts.WarehouseMainId, TestDataConsts.MedicineParacetamolId, batchCId);
            balC.AddStock(binId, 5m, Guid.NewGuid());
            await _balanceRepository.InsertAsync(balC, autoSave: true);

            // Lô B: 10 hộp trong kho
            InventoryBalance balB = new(Guid.NewGuid(), TestDataConsts.WarehouseMainId, TestDataConsts.MedicineParacetamolId, batchBId);
            balB.AddStock(binId, 10m, Guid.NewGuid());
            await _balanceRepository.InsertAsync(balB, autoSave: true);

            // Lô A: 10 hộp trong kho
            InventoryBalance balA = new(Guid.NewGuid(), TestDataConsts.WarehouseMainId, TestDataConsts.MedicineParacetamolId, batchAId);
            balA.AddStock(binId, 10m, Guid.NewGuid());
            await _balanceRepository.InsertAsync(balA, autoSave: true);

            // 4. Tạo phiếu xuất kho (GoodsIssue) và dòng hàng yêu cầu xuất 12 hộp
            InventoryTicket ticket = await _ticketManager.CreateTicketAsync(
                TicketType.GoodsIssue,
                TestDataConsts.WarehouseMainId,
                Guid.NewGuid(),
                "REF-DOC-001",
                "Phieu xuat kho test FEFO"
            );
            await _ticketRepository.InsertAsync(ticket, autoSave: true);

            InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(
                ticket,
                TestDataConsts.MedicineParacetamolId,
                null,
                12m, // Yêu cầu xuất 12
                TestDataConsts.UnitBoxId,
                1
            );
            await _ticketLineRepository.InsertAsync(line, autoSave: true);
            lineId = line.Id;

            // 5. Act: Thực thi thuật toán phân bổ FEFO
            await _ticketManager.AllocateFEFOForLineAsync(ticket, line);
        });

        // UOW 2: Assert ket qua khi database da ghi nhan day du
        await WithUnitOfWorkAsync(async () =>
        {
            List<InventoryTicketDetail> details = await _ticketDetailRepository.GetListAsync(x => x.TicketLineId == lineId);
            details.Count.ShouldBe(2); // Phải phân bổ vào 2 lô

            // Lô C (hết hạn trước tiên) phải được lấy sạch 5 hộp
            InventoryTicketDetail? detailC = details.FirstOrDefault(x => x.ProductBatchId == batchCId);
            detailC.ShouldNotBeNull();
            detailC.Quantity.ShouldBe(5m);
            detailC.BinId.ShouldBe(binId);

            // Lô B (cùng hạn với Lô A nhưng sản xuất trước) phải được lấy 7 hộp còn thiếu
            InventoryTicketDetail? detailB = details.FirstOrDefault(x => x.ProductBatchId == batchBId);
            detailB.ShouldNotBeNull();
            detailB.Quantity.ShouldBe(7m);
            detailB.BinId.ShouldBe(binId);

            // Lô A (ngày sản xuất mới hơn) không được đụng tới
            details.Any(x => x.ProductBatchId == batchAId).ShouldBeFalse();
        });
    }

    [QATest(scenario: "Ném ngoại lệ khi tổng tồn kho không đủ số lượng yêu cầu xuất FEFO.", feature: "FEFO", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_Exception_When_Insufficient_Stock()
    {
        Guid binId = Guid.NewGuid();
        Guid batchId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            // Thiết lập cấu trúc kho
            Guid zoneId = Guid.NewGuid();
            Zone zone = new(zoneId, TestDataConsts.WarehouseMainId, "ZONE-ERR1", "Khu loi 1", ZoneType.Storage, StorageCondition.Normal, "#FFFFFF", 0, 0, 10, 10, 0);
            await _zoneRepository.InsertAsync(zone, autoSave: true);

            Bin bin = new(binId, TestDataConsts.WarehouseMainId, zoneId, "BIN-ERR1", 0, 0, 100, 100, 0, 10, 10);
            await _binRepository.InsertAsync(bin, autoSave: true);

            // Tạo 1 Lô hàng hợp lệ (Duyệt QA và chưa hết hạn)
            ProductBatch batch = new(batchId, "LOTERR1", TestDataConsts.MedicineParacetamolId, "LOT-ERR1", DateTime.Parse("2023-01-24"), DateTime.Parse("2028-12-31"), TestDataConsts.SupplierAId);
            batch.ApproveQA();
            await _batchRepository.InsertAsync(batch, autoSave: true);

            // Chỉ có 5 hộp tồn kho
            InventoryBalance balance = new(Guid.NewGuid(), TestDataConsts.WarehouseMainId, TestDataConsts.MedicineParacetamolId, batchId);
            balance.AddStock(binId, 5m, Guid.NewGuid());
            await _balanceRepository.InsertAsync(balance, autoSave: true);

            // Tạo phiếu xuất yêu cầu 10 hộp
            InventoryTicket ticket = await _ticketManager.CreateTicketAsync(TicketType.GoodsIssue, TestDataConsts.WarehouseMainId, Guid.NewGuid(), "REF-DOC-ERR1", "Phieu loi thieu hang");
            await _ticketRepository.InsertAsync(ticket, autoSave: true);

            InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(ticket, TestDataConsts.MedicineParacetamolId, null, 10m, TestDataConsts.UnitBoxId, 1);
            await _ticketLineRepository.InsertAsync(line, autoSave: true);

            // Thực thi và kiểm tra ném ngoại lệ
            BusinessException exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _ticketManager.AllocateFEFOForLineAsync(ticket, line);
            });

            exception.Code.ShouldBe("SupplyCoreERP:InsufficientStock");
            exception.Message.ShouldContain("không đủ số lượng");
        });
    }

    [QATest(scenario: "Ném ngoại lệ khi tồn kho thô đủ nhưng lô hàng chưa được duyệt QA hoặc đã hết hạn.", feature: "FEFO", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Throw_Exception_When_Stock_Exists_But_Not_Approved_Or_Expired()
    {
        Guid binId = Guid.NewGuid();
        Guid batchDraftId = Guid.NewGuid();
        Guid batchExpiredId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            // Thiết lập cấu trúc kho
            Guid zoneId = Guid.NewGuid();
            Zone zone = new(zoneId, TestDataConsts.WarehouseMainId, "ZONE-ERR2", "Khu loi 2", ZoneType.Storage, StorageCondition.Normal, "#FFFFFF", 0, 0, 10, 10, 0);
            await _zoneRepository.InsertAsync(zone, autoSave: true);

            Bin bin = new(binId, TestDataConsts.WarehouseMainId, zoneId, "BIN-ERR2", 0, 0, 100, 100, 0, 10, 10);
            await _binRepository.InsertAsync(bin, autoSave: true);

            // Lô 1: Chưa duyệt QA (vẫn ở trạng thái Draft), tồn kho 5 hộp
            ProductBatch batchDraft = new(batchDraftId, "LOTDRAFT", TestDataConsts.MedicineParacetamolId, "LOT-DRAFT", DateTime.Parse("2023-01-01"), DateTime.Parse("2028-12-31"), TestDataConsts.SupplierAId);
            // Không gọi ApproveQA
            await _batchRepository.InsertAsync(batchDraft, autoSave: true);

            InventoryBalance balDraft = new(Guid.NewGuid(), TestDataConsts.WarehouseMainId, TestDataConsts.MedicineParacetamolId, batchDraftId);
            balDraft.AddStock(binId, 5m, Guid.NewGuid());
            await _balanceRepository.InsertAsync(balDraft, autoSave: true);

            // Lô 2: Đã duyệt QA nhưng hết hạn (ExpiryDate < hiện tại), tồn kho 5 hộp
            ProductBatch batchExpired = new(batchExpiredId, "LOTEXP", TestDataConsts.MedicineParacetamolId, "LOT-EXP", DateTime.Parse("2020-01-01"), DateTime.Parse("2024-01-01"), TestDataConsts.SupplierAId);
            batchExpired.ApproveQA();
            await _batchRepository.InsertAsync(batchExpired, autoSave: true);

            InventoryBalance balExpired = new(Guid.NewGuid(), TestDataConsts.WarehouseMainId, TestDataConsts.MedicineParacetamolId, batchExpiredId);
            balExpired.AddStock(binId, 5m, Guid.NewGuid());
            await _balanceRepository.InsertAsync(balExpired, autoSave: true);

            // Tổng tồn kho thô là 10 (Draft: 5, Expired: 5)
            // Tạo phiếu xuất yêu cầu 10 hộp
            InventoryTicket ticket = await _ticketManager.CreateTicketAsync(TicketType.GoodsIssue, TestDataConsts.WarehouseMainId, Guid.NewGuid(), "REF-DOC-ERR2", "Phieu loi khong duyet hoac het han");
            await _ticketRepository.InsertAsync(ticket, autoSave: true);

            InventoryTicketLine line = await _ticketManager.CreateTicketLineAsync(ticket, TestDataConsts.MedicineParacetamolId, null, 10m, TestDataConsts.UnitBoxId, 1);
            await _ticketLineRepository.InsertAsync(line, autoSave: true);

            // Thực thi và kiểm tra ném ngoại lệ
            BusinessException exception = await Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _ticketManager.AllocateFEFOForLineAsync(ticket, line);
            });

            exception.Code.ShouldBe("SupplyCoreERP:InsufficientStock");
            exception.Message.ShouldContain("chưa được Duyệt QA hoặc đã hết hạn sử dụng");
        });
    }
}
