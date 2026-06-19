using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Dashboard.Dtos;
using SupplyCoreERP.Enums.Balances;
using SupplyCoreERP.Enums.Orders;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Batches;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Inventory.Transactions;
using SupplyCoreERP.Inventory.Warehouses;
using SupplyCoreERP.Partner.Customers;
using SupplyCoreERP.Partner.Suppliers;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseReturns;
using SupplyCoreERP.Sales.Orders;
using SupplyCoreERP.Sales.SalesRecalls;
using Volo.Abp.Domain.Repositories;

namespace SupplyCoreERP.Dashboard;

public class DashboardAppService : SupplyCore, IDashboardAppService
{
    private readonly IRepository<Warehouse, Guid> _warehouseRepository;
    private readonly IRepository<Medicine, Guid> _medicineRepository;
    private readonly IRepository<Bin, Guid> _binRepository;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepository;
    private readonly IRepository<InventoryTransaction, Guid> _transactionRepository;
    private readonly IRepository<ProductBatch, Guid> _batchRepository;
    private readonly IRepository<SalesOrder, Guid> _salesOrderRepository;
    private readonly IRepository<PurchaseOrder, Guid> _purchaseOrderRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Supplier, Guid> _supplierRepository;
    private readonly IRepository<InventoryTicket, Guid> _ticketRepository;
    private readonly IRepository<PurchaseReturn, Guid> _purchaseReturnRepository;
    private readonly IRepository<SalesRecall, Guid> _salesRecallRepository;
    private readonly IRepository<InventoryReservation, Guid> _reservationRepository;

    public DashboardAppService(
        IRepository<Warehouse, Guid> warehouseRepository,
        IRepository<Medicine, Guid> medicineRepository,
        IRepository<Bin, Guid> binRepository,
        IRepository<InventoryBalance, Guid> balanceRepository,
        IRepository<InventoryTransaction, Guid> transactionRepository,
        IRepository<ProductBatch, Guid> batchRepository,
        IRepository<SalesOrder, Guid> salesOrderRepository,
        IRepository<PurchaseOrder, Guid> purchaseOrderRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Supplier, Guid> supplierRepository,
        IRepository<InventoryTicket, Guid> ticketRepository,
        IRepository<PurchaseReturn, Guid> purchaseReturnRepository,
        IRepository<SalesRecall, Guid> salesRecallRepository,
        IRepository<InventoryReservation, Guid> reservationRepository)
    {
        _warehouseRepository = warehouseRepository;
        _medicineRepository = medicineRepository;
        _binRepository = binRepository;
        _balanceRepository = balanceRepository;
        _transactionRepository = transactionRepository;
        _batchRepository = batchRepository;
        _salesOrderRepository = salesOrderRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _customerRepository = customerRepository;
        _supplierRepository = supplierRepository;
        _ticketRepository = ticketRepository;
        _purchaseReturnRepository = purchaseReturnRepository;
        _salesRecallRepository = salesRecallRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilterInput input)
    {
        DashboardOverviewDto dto = new();

        // 1. Tổng số kho hoạt động
        dto.TotalWarehouses = (int)await _warehouseRepository.CountAsync(x => x.IsActive && (!input.WarehouseId.HasValue || x.Id == input.WarehouseId.Value));

        // 2. Tổng số thuốc hoạt động
        if (input.WarehouseId.HasValue)
        {
            IQueryable<InventoryBalance> balanceQuery = await _balanceRepository.GetQueryableAsync();
            dto.TotalMedicines = await balanceQuery
                .Where(x => x.WarehouseId == input.WarehouseId.Value && x.Quantity > 0 && (!input.CategoryId.HasValue || x.Product.CategoryId == input.CategoryId.Value))
                .Select(x => x.ProductId)
                .Distinct()
                .CountAsync();
        }
        else
        {
            dto.TotalMedicines = (int)await _medicineRepository.CountAsync(x => x.IsActive && (!input.CategoryId.HasValue || x.CategoryId == input.CategoryId.Value));
        }

        // 3. Tỉ lệ lấp đầy bình quan
        List<Bin> bins = await _binRepository.GetListAsync(x => !input.WarehouseId.HasValue || x.WarehouseId == input.WarehouseId.Value);
        IQueryable<InventoryBalance> balancesQuery = await _balanceRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }

        var binOccupiedVolumes = await balancesQuery
            .SelectMany(x => x.BinBalances.Where(bb => bb.Quantity > 0).Select(bb => new { bb.BinId, Volume = bb.Quantity * x.Product.BaseUnitVolume }))
            .GroupBy(x => x.BinId)
            .Select(g => new { BinId = g.Key, OccupiedVolume = g.Sum(x => x.Volume) })
            .ToListAsync();

        var binOccupiedDict = binOccupiedVolumes.ToDictionary(x => x.BinId, x => x.OccupiedVolume);

        List<decimal> binCapacityPercents = new();
        foreach (Bin bin in bins)
        {
            decimal maxVolSafe = bin.MaxVolume * 0.8m;
            if (maxVolSafe > 0)
            {
                decimal occupiedVol = binOccupiedDict.ContainsKey(bin.Id) ? binOccupiedDict[bin.Id] : 0m;
                decimal percent = (occupiedVol / maxVolSafe) * 100m;
                binCapacityPercents.Add(percent);
            }
        }
        dto.AverageCapacityPercent = binCapacityPercents.Any()
            ? Math.Round(binCapacityPercents.Average(), 2)
            : 0m;

        // 4. Lô thuốc cận hạn sử dụng (ngưỡng thời gian động theo filter Days, mặc định 90 ngày và tồn kho > 0)
        int thresholdDays = input.Days ?? 90;
        DateTime alertDate = DateTime.Now.Date.AddDays(thresholdDays);
        dto.ExpiredAlertCount = (int)await _balanceRepository.CountAsync(b =>
            b.ProductBatch != null &&
            b.ProductBatch.ExpiryDate.Date <= alertDate &&
            b.ProductBatch.ExpiryDate.Date >= DateTime.Now.Date &&
            b.Quantity > 0 &&
            (!input.WarehouseId.HasValue || b.WarehouseId == input.WarehouseId.Value) &&
            (!input.CategoryId.HasValue || b.Product.CategoryId == input.CategoryId.Value)
        );

        // Lọc ngày (nếu có) cho các chỉ số phát sinh trong kỳ
        DateTime? startDate = null;
        if (input.Days.HasValue)
        {
            startDate = DateTime.Now.Date.AddDays(-input.Days.Value + 1);
        }

        // 5. Tổng doanh thu SO (trừ Nháp và Hủy)
        IQueryable<SalesOrder> salesQuery = await _salesOrderRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (startDate.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.OrderDate >= startDate.Value);
        }
        if (input.CategoryId.HasValue)
        {
            dto.TotalRevenue = await salesQuery
                .Where(x => x.Status != SalesOrderStatus.Draft && x.Status != SalesOrderStatus.Canceled)
                .SelectMany(x => x.Lines)
                .Where(line => line.Product.CategoryId == input.CategoryId.Value)
                .SumAsync(line => line.Quantity * line.UnitPrice * (1 - line.DiscountRate / 100) * (1 + line.TaxRate / 100));
        }
        else
        {
            dto.TotalRevenue = await salesQuery
                .Where(x => x.Status != SalesOrderStatus.Draft && x.Status != SalesOrderStatus.Canceled)
                .SumAsync(x => x.TotalAmount);
        }

        // 6. Tổng chi phí PO (trừ Nháp và Hủy)
        IQueryable<PurchaseOrder> purchaseQuery = await _purchaseOrderRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            purchaseQuery = purchaseQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (startDate.HasValue)
        {
            purchaseQuery = purchaseQuery.Where(x => x.OrderDate >= startDate.Value);
        }
        if (input.CategoryId.HasValue)
        {
            dto.TotalProcurement = await purchaseQuery
                .Where(x => x.Status != PurchaseOrderStatus.Draft && x.Status != PurchaseOrderStatus.Canceled)
                .SelectMany(x => x.Lines)
                .Where(line => line.Product.CategoryId == input.CategoryId.Value)
                .SumAsync(line => line.Quantity * line.UnitPrice * (1 + line.TaxRate / 100));
        }
        else
        {
            dto.TotalProcurement = await purchaseQuery
                .Where(x => x.Status != PurchaseOrderStatus.Draft && x.Status != PurchaseOrderStatus.Canceled)
                .SumAsync(x => x.TotalAmount);
        }

        // 7. Tổng thu hồi bán hàng (trừ Nháp và Từ chối)
        IQueryable<SalesRecall> recallQuery = await _salesRecallRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            recallQuery = recallQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (startDate.HasValue)
        {
            recallQuery = recallQuery.Where(x => x.RecallDate >= startDate.Value);
        }
        if (input.CategoryId.HasValue)
        {
            recallQuery = recallQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }
        dto.TotalSalesRecall = await recallQuery
            .Where(x => x.Status != SalesRecallStatus.Draft && x.Status != SalesRecallStatus.Rejected)
            .SumAsync(x => x.TotalAmount);

        // 8. Tổng trả hàng NCC (trừ Nháp và Từ chối)
        IQueryable<PurchaseReturn> returnQuery = await _purchaseReturnRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            returnQuery = returnQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (startDate.HasValue)
        {
            returnQuery = returnQuery.Where(x => x.ReturnDate >= startDate.Value);
        }
        if (input.CategoryId.HasValue)
        {
            dto.TotalPurchaseReturn = await returnQuery
                .Where(x => x.Status != PurchaseReturnStatus.Draft && x.Status != PurchaseReturnStatus.Rejected)
                .SelectMany(x => x.Lines)
                .Where(line => line.Product.CategoryId == input.CategoryId.Value)
                .SumAsync(line => line.Quantity * (line.OriginalUnitPrice * (1 - line.DepreciationRate / 100)) * (1 + line.TaxRate / 100));
        }
        else
        {
            dto.TotalPurchaseReturn = await returnQuery
                .Where(x => x.Status != PurchaseReturnStatus.Draft && x.Status != PurchaseReturnStatus.Rejected)
                .SumAsync(x => x.TotalAmount);
        }

        // 9. Tổng thể tích giữ chỗ (Active)
        IQueryable<InventoryReservation> reservationQuery = await _reservationRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            reservationQuery = reservationQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            reservationQuery = reservationQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }
        dto.TotalReservedVolume = await reservationQuery
            .Where(x => x.Status == ReservationStatus.Active)
            .SumAsync(x => x.ReservedQuantity * x.Product.BaseUnitVolume);

        // 10. Tổng thể tích khả dụng thực tế
        IQueryable<InventoryBalance> occupiedQuery = await _balanceRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            occupiedQuery = occupiedQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            occupiedQuery = occupiedQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }
        decimal totalOccupied = await occupiedQuery.SumAsync(x => x.Quantity * x.Product.BaseUnitVolume);
        dto.TotalAvailableVolume = Math.Max(0m, totalOccupied - dto.TotalReservedVolume);

        return dto;
    }

    public async Task<List<DashboardFinancialTrendDto>> GetFinancialTrendsAsync(DashboardFilterInput input)
    {
        int daysCount = input.Days ?? 10;
        DateTime startDate = DateTime.Now.Date.AddDays(-daysCount + 1);

        IQueryable<SalesOrder> salesQuery = await _salesOrderRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var salesData = await salesQuery
            .Where(x => x.OrderDate >= startDate && x.Status != SalesOrderStatus.Draft && x.Status != SalesOrderStatus.Canceled)
            .SelectMany(x => x.Lines)
            .Where(line => !input.CategoryId.HasValue || line.Product.CategoryId == input.CategoryId.Value)
            .GroupBy(line => line.SalesOrder.OrderDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(line => line.Quantity * line.UnitPrice * (1 - line.DiscountRate / 100) * (1 + line.TaxRate / 100)) })
            .ToListAsync();

        IQueryable<PurchaseOrder> purchaseQuery = await _purchaseOrderRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            purchaseQuery = purchaseQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        var purchaseData = await purchaseQuery
            .Where(x => x.OrderDate >= startDate && x.Status != PurchaseOrderStatus.Draft && x.Status != PurchaseOrderStatus.Canceled)
            .SelectMany(x => x.Lines)
            .Where(line => !input.CategoryId.HasValue || line.Product.CategoryId == input.CategoryId.Value)
            .GroupBy(line => line.PurchaseOrder.OrderDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(line => line.Quantity * line.UnitPrice * (1 + line.TaxRate / 100)) })
            .ToListAsync();

        var salesDict = salesData.ToDictionary(x => x.Date, x => x.Amount);
        var purchaseDict = purchaseData.ToDictionary(x => x.Date, x => x.Amount);

        List<DashboardFinancialTrendDto> trends = new();

        for (int i = daysCount - 1; i >= 0; i--)
        {
            DateTime date = DateTime.Now.Date.AddDays(-i);
            string dateStr = date.ToString("dd/MM");

            decimal salesSum = salesDict.ContainsKey(date) ? salesDict[date] : 0m;
            decimal procurementSum = purchaseDict.ContainsKey(date) ? purchaseDict[date] : 0m;

            trends.Add(new DashboardFinancialTrendDto
            {
                Date = dateStr,
                SalesAmount = salesSum,
                ProcurementAmount = procurementSum
            });
        }

        return trends;
    }

    public async Task<List<DashboardSalesStatusDto>> GetSalesStatusDistributionAsync(DashboardFilterInput input)
    {
        IQueryable<SalesOrder> query = await _salesOrderRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.Days.HasValue)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-input.Days.Value + 1);
            query = query.Where(x => x.OrderDate >= startDate);
        }
        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.Lines.Any(line => line.Product.CategoryId == input.CategoryId.Value));
        }

        var rawGroups = await query
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int total = rawGroups.Sum(x => x.Count);
        if (total == 0)
        {
            return new List<DashboardSalesStatusDto>();
        }

        List<DashboardSalesStatusDto> groups = rawGroups
            .Select(x => new DashboardSalesStatusDto
            {
                StatusName = GetSalesStatusLabel(x.Status),
                Count = x.Count,
                Percentage = Math.Round((decimal)x.Count / total * 100m, 2)
            })
            .ToList();

        return groups;
    }

    public async Task<List<DashboardProcurementStatusDto>> GetProcurementStatusDistributionAsync(DashboardFilterInput input)
    {
        IQueryable<PurchaseOrder> query = await _purchaseOrderRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.Days.HasValue)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-input.Days.Value + 1);
            query = query.Where(x => x.OrderDate >= startDate);
        }
        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.Lines.Any(line => line.Product.CategoryId == input.CategoryId.Value));
        }

        var rawGroups = await query
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int total = rawGroups.Sum(x => x.Count);
        if (total == 0)
        {
            return new List<DashboardProcurementStatusDto>();
        }

        List<DashboardProcurementStatusDto> groups = rawGroups
            .Select(x => new DashboardProcurementStatusDto
            {
                StatusName = GetPurchaseStatusLabel(x.Status),
                Count = x.Count,
                Percentage = Math.Round((decimal)x.Count / total * 100m, 2)
            })
            .ToList();

        return groups;
    }

    public async Task<List<DashboardWarehouseCapacityDto>> GetWarehouseCapacitiesAsync(DashboardFilterInput input)
    {
        List<DashboardWarehouseCapacityDto> dto = new();

        IQueryable<Bin> binsQuery = await _binRepository.GetQueryableAsync();
        var binVolumes = await binsQuery
            .GroupBy(b => b.WarehouseId)
            .Select(g => new { WarehouseId = g.Key, MaxVolume = g.Sum(b => b.MaxVolume) })
            .ToListAsync();
        var binVolumeDict = binVolumes.ToDictionary(x => x.WarehouseId, x => x.MaxVolume);

        IQueryable<InventoryBalance> balancesQuery = await _balanceRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }
        var occupiedVolumes = await balancesQuery
            .GroupBy(b => b.WarehouseId)
            .Select(g => new { WarehouseId = g.Key, Volume = g.Sum(b => b.Quantity * b.Product.BaseUnitVolume) })
            .ToListAsync();
        var occupiedVolDict = occupiedVolumes.ToDictionary(x => x.WarehouseId, x => x.Volume);

        IQueryable<InventoryReservation> reservationQuery = await _reservationRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            reservationQuery = reservationQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            reservationQuery = reservationQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }
        var reservedVolumes = await reservationQuery
            .Where(x => x.Status == ReservationStatus.Active)
            .GroupBy(r => r.WarehouseId)
            .Select(g => new { WarehouseId = g.Key, Volume = g.Sum(r => r.ReservedQuantity * r.Product.BaseUnitVolume) })
            .ToListAsync();
        var reservedVolDict = reservedVolumes.ToDictionary(x => x.WarehouseId, x => x.Volume);

        List<Warehouse> warehouses = await _warehouseRepository.GetListAsync(x => x.IsActive && (!input.WarehouseId.HasValue || x.Id == input.WarehouseId.Value));
        foreach (Warehouse wh in warehouses)
        {
            decimal whMaxVol = binVolumeDict.ContainsKey(wh.Id) ? binVolumeDict[wh.Id] : 0m;
            decimal whSafeMaxVol = whMaxVol * 0.8m;

            decimal whOccupied = occupiedVolDict.ContainsKey(wh.Id) ? occupiedVolDict[wh.Id] : 0m;
            decimal whReserved = reservedVolDict.ContainsKey(wh.Id) ? reservedVolDict[wh.Id] : 0m;

            decimal whAvailable = Math.Max(0m, whOccupied - whReserved);

            decimal whCapacityPercent = whSafeMaxVol > 0
                ? Math.Round((whOccupied / whSafeMaxVol) * 100m, 2)
                : 0m;

            dto.Add(new DashboardWarehouseCapacityDto
            {
                WarehouseId = wh.Id,
                WarehouseName = wh.Name,
                OccupiedVolume = whOccupied,
                ReservedVolume = whReserved,
                AvailableVolume = whAvailable,
                SafeMaxVolume = whSafeMaxVol,
                CapacityPercent = whCapacityPercent
            });
        }

        return dto;
    }

    public async Task<List<DashboardInventoryTransactionDto>> GetInventoryTransactionDistributionAsync(DashboardFilterInput input)
    {
        IQueryable<InventoryTransaction> query = await _transactionRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.Days.HasValue)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-input.Days.Value + 1);
            query = query.Where(x => x.CreationTime >= startDate);
        }
        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }

        var rawGroups = await query
            .GroupBy(x => x.TransactionType)
            .Select(g => new
            {
                Type = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        int total = rawGroups.Sum(x => x.Count);
        if (total == 0)
        {
            return new List<DashboardInventoryTransactionDto>();
        }

        Dictionary<string, int> groupedTypes = new()
        {
            { "Xuất bán (SO)", 0 },
            { "Nhập mua (PO)", 0 },
            { "Trả hàng", 0 },
            { "Thu hồi", 0 },
            { "Chuyển kho", 0 },
            { "Hủy / Điều chỉnh", 0 }
        };

        foreach (var g in rawGroups)
        {
            if (g.Type == InventoryTransactionType.SaleDelivery)
            {
                groupedTypes["Xuất bán (SO)"] += g.Count;
            }
            else if (g.Type == InventoryTransactionType.PurchaseReceipt)
            {
                groupedTypes["Nhập mua (PO)"] += g.Count;
            }
            else if (g.Type == InventoryTransactionType.ReturnInward || g.Type == InventoryTransactionType.ReturnOutward)
            {
                groupedTypes["Trả hàng"] += g.Count;
            }
            else if (g.Type == InventoryTransactionType.RecallReceipt)
            {
                groupedTypes["Thu hồi"] += g.Count;
            }
            else if (g.Type == InventoryTransactionType.TransferIn || g.Type == InventoryTransactionType.TransferOut)
            {
                groupedTypes["Chuyển kho"] += g.Count;
            }
            else
            {
                groupedTypes["Hủy / Điều chỉnh"] += g.Count;
            }
        }

        return groupedTypes
            .Where(x => x.Value > 0)
            .Select(x => new DashboardInventoryTransactionDto
            {
                TransactionTypeName = x.Key,
                Count = x.Value,
                Percentage = Math.Round((decimal)x.Value / total * 100m, 2)
            })
            .ToList();
    }

    public async Task<List<DashboardCategoryDistributionDto>> GetMedicineCategoryDistributionAsync(DashboardFilterInput input)
    {
        List<DashboardCategoryDistributionDto> dto = new();

        IQueryable<InventoryBalance> balancesQuery = await _balanceRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }

        var rawGroups = await balancesQuery
            .Where(x => x.Product != null && x.Product.Category != null)
            .GroupBy(x => x.Product.Category.Name)
            .Select(g => new
            {
                CategoryName = g.Key,
                TotalQty = g.Sum(x => x.Quantity)
            })
            .ToListAsync();

        decimal totalSystemQty = rawGroups.Sum(x => x.TotalQty);
        if (totalSystemQty > 0)
        {
            foreach (var cat in rawGroups)
            {
                decimal percentage = Math.Round((cat.TotalQty / totalSystemQty) * 100m, 2);
                dto.Add(new DashboardCategoryDistributionDto
                {
                    CategoryName = cat.CategoryName,
                    TotalQuantity = cat.TotalQty,
                    Percentage = percentage
                });
            }
        }

        return dto;
    }

    public async Task<List<DashboardExpiredBatchDto>> GetNearExpiryBatchesAsync(DashboardFilterInput input)
    {
        List<DashboardExpiredBatchDto> dto = new();

        IQueryable<InventoryBalance> balancesQuery = await _balanceRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }

        int thresholdDays = input.Days ?? 90;
        DateTime today = DateTime.Now.Date;
        DateTime alertDate = today.AddDays(thresholdDays);

        var expiredBatches = await balancesQuery
            .Where(b => b.ProductBatch != null &&
                        b.ProductBatch.ExpiryDate.Date <= alertDate &&
                        b.ProductBatch.ExpiryDate.Date >= today &&
                        b.Quantity > 0)
            .GroupBy(b => new
            {
                MedicineName = b.Product.Name,
                BatchNumber = b.ProductBatch.BatchNumber,
                WarehouseName = b.Warehouse.Name,
                ExpiryDate = b.ProductBatch.ExpiryDate
            })
            .Select(g => new
            {
                MedicineName = g.Key.MedicineName,
                BatchNumber = g.Key.BatchNumber,
                WarehouseName = g.Key.WarehouseName,
                Quantity = g.Sum(x => x.Quantity),
                ExpiryDate = g.Key.ExpiryDate
            })
            .OrderBy(x => x.ExpiryDate)
            .ToListAsync();

        foreach (var item in expiredBatches)
        {
            int daysRemaining = (item.ExpiryDate.Date - today).Days;
            dto.Add(new DashboardExpiredBatchDto
            {
                MedicineName = item.MedicineName,
                BatchNumber = item.BatchNumber,
                WarehouseName = item.WarehouseName,
                Quantity = item.Quantity,
                ExpiryDate = item.ExpiryDate,
                DaysRemaining = daysRemaining
            });
        }

        return dto;
    }

    public async Task<List<DashboardExpiredBatchDto>> GetAlreadyExpiredBatchesAsync(DashboardFilterInput input)
    {
        List<DashboardExpiredBatchDto> dto = new();

        IQueryable<InventoryBalance> balancesQuery = await _balanceRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            balancesQuery = balancesQuery.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }

        DateTime today = DateTime.Now.Date;

        var expiredBatches = await balancesQuery
            .Where(b => b.ProductBatch != null &&
                        b.ProductBatch.ExpiryDate.Date < today &&
                        b.Quantity > 0)
            .GroupBy(b => new
            {
                MedicineName = b.Product.Name,
                BatchNumber = b.ProductBatch.BatchNumber,
                WarehouseName = b.Warehouse.Name,
                ExpiryDate = b.ProductBatch.ExpiryDate
            })
            .Select(g => new
            {
                MedicineName = g.Key.MedicineName,
                BatchNumber = g.Key.BatchNumber,
                WarehouseName = g.Key.WarehouseName,
                Quantity = g.Sum(x => x.Quantity),
                ExpiryDate = g.Key.ExpiryDate
            })
            .OrderBy(x => x.ExpiryDate)
            .ToListAsync();

        foreach (var item in expiredBatches)
        {
            int daysRemaining = (item.ExpiryDate.Date - today).Days;
            dto.Add(new DashboardExpiredBatchDto
            {
                MedicineName = item.MedicineName,
                BatchNumber = item.BatchNumber,
                WarehouseName = item.WarehouseName,
                Quantity = item.Quantity,
                ExpiryDate = item.ExpiryDate,
                DaysRemaining = daysRemaining
            });
        }

        return dto;
    }

    public async Task<DashboardDebtOverviewDto> GetDebtOverviewAsync(DashboardFilterInput input)
    {
        DashboardDebtOverviewDto dto = new();

        // 1. Phải thu lũy kế & Phải trả lũy kế tổng hệ thống
        IQueryable<Customer> customerQuery = await _customerRepository.GetQueryableAsync();
        IQueryable<Supplier> supplierQuery = await _supplierRepository.GetQueryableAsync();

        dto.TotalReceivableDebt = await customerQuery
            .Where(x => x.IsActive)
            .SumAsync(x => x.CurrentDebt);

        dto.TotalPayableDebt = await supplierQuery
            .Where(x => x.IsActive)
            .SumAsync(x => x.CurrentDebt);

        // Nếu có lọc Kho hàng và Ngày, ta tính thêm số liệu Công nợ phát sinh trong kỳ của Kho đó để bổ trợ ý nghĩa phân tích
        if (input.WarehouseId.HasValue || input.Days.HasValue)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-(input.Days ?? 30) + 1);

            // Doanh số bán hàng chưa thanh toán trong kỳ của kho (Đại diện cho nợ phải thu phát sinh trong kỳ)
            IQueryable<SalesOrder> salesQuery = await _salesOrderRepository.GetQueryableAsync();
            if (input.WarehouseId.HasValue) salesQuery = salesQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
            salesQuery = salesQuery.Where(x => x.OrderDate >= startDate && x.Status != SalesOrderStatus.Draft && x.Status != SalesOrderStatus.Canceled);
            // Giả lập tính toán: sum các đơn đã duyệt/giao nhưng chưa hoàn tất
            dto.TotalReceivableDebt = await salesQuery
                .Where(x => x.Status == SalesOrderStatus.Approved || x.Status == SalesOrderStatus.Delivering)
                .SumAsync(x => x.TotalAmount);

            // Chi phí mua hàng chưa thanh toán trong kỳ của kho (Đại diện cho nợ phải trả phát sinh trong kỳ)
            IQueryable<PurchaseOrder> purchaseQuery = await _purchaseOrderRepository.GetQueryableAsync();
            if (input.WarehouseId.HasValue) purchaseQuery = purchaseQuery.Where(x => x.WarehouseId == input.WarehouseId.Value);
            purchaseQuery = purchaseQuery.Where(x => x.OrderDate >= startDate && x.Status != PurchaseOrderStatus.Draft && x.Status != PurchaseOrderStatus.Canceled);
            dto.TotalPayableDebt = await purchaseQuery
                .Where(x => x.Status == PurchaseOrderStatus.Approved || x.Status == PurchaseOrderStatus.Receiving)
                .SumAsync(x => x.TotalAmount);
        }

        dto.TotalCustomers = (int)await _customerRepository.CountAsync(x => x.IsActive);
        dto.TotalSuppliers = (int)await _supplierRepository.CountAsync(x => x.IsActive);

        return dto;
    }

    public async Task<List<DashboardPartnerDebtDto>> GetTopCustomerDebtsAsync(DashboardFilterInput input)
    {
        IQueryable<Customer> customerQuery = await _customerRepository.GetQueryableAsync();

        List<DashboardPartnerDebtDto> topCustomers = await customerQuery
            .Where(x => x.IsActive && x.CurrentDebt > 0)
            .OrderByDescending(x => x.CurrentDebt)
            .Take(5)
            .Select(x => new DashboardPartnerDebtDto
            {
                PartnerCode = x.Code,
                PartnerName = x.Name,
                CurrentDebt = x.CurrentDebt
            })
            .ToListAsync();

        return topCustomers;
    }

    public async Task<List<DashboardPartnerDebtDto>> GetTopSupplierDebtsAsync(DashboardFilterInput input)
    {
        IQueryable<Supplier> supplierQuery = await _supplierRepository.GetQueryableAsync();

        List<DashboardPartnerDebtDto> topSuppliers = await supplierQuery
            .Where(x => x.IsActive && x.CurrentDebt > 0)
            .OrderByDescending(x => x.CurrentDebt)
            .Take(5)
            .Select(x => new DashboardPartnerDebtDto
            {
                PartnerCode = x.Code,
                PartnerName = x.Name,
                CurrentDebt = x.CurrentDebt
            })
            .ToListAsync();

        return topSuppliers;
    }

    public async Task<List<DashboardInventoryTicketStatusDto>> GetInventoryTicketStatusDistributionAsync(DashboardFilterInput input)
    {
        IQueryable<InventoryTicket> query = await _ticketRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.Days.HasValue)
        {
            DateTime startDate = DateTime.Now.Date.AddDays(-input.Days.Value + 1);
            query = query.Where(x => x.CreationTime >= startDate);
        }
        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.Lines.Any(line => line.Product.CategoryId == input.CategoryId.Value));
        }

        var rawGroups = await query
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int total = rawGroups.Sum(x => x.Count);
        if (total == 0)
        {
            return new List<DashboardInventoryTicketStatusDto>();
        }

        List<DashboardInventoryTicketStatusDto> groups = rawGroups
            .Select(x => new DashboardInventoryTicketStatusDto
            {
                StatusName = GetApprovalStatusLabel(x.Status),
                Count = x.Count,
                Percentage = Math.Round((decimal)x.Count / total * 100m, 2)
            })
            .ToList();

        return groups;
    }

    public async Task<List<DashboardBatchQAStatusDto>> GetBatchQAStatusDistributionAsync(DashboardFilterInput input)
    {
        IQueryable<ProductBatch> query = await _batchRepository.GetQueryableAsync();

        if (input.WarehouseId.HasValue)
        {
            IQueryable<Guid> batchIdsInWarehouse = (await _balanceRepository.GetQueryableAsync())
                .Where(x => x.WarehouseId == input.WarehouseId.Value && x.Quantity > 0 && x.ProductBatchId != Guid.Empty)
                .Select(x => x.ProductBatchId)
                .Distinct();

            query = query.Where(x => batchIdsInWarehouse.Contains(x.Id));
        }

        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }

        var rawGroups = await query
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        int total = rawGroups.Sum(x => x.Count);
        if (total == 0)
        {
            return new List<DashboardBatchQAStatusDto>();
        }

        List<DashboardBatchQAStatusDto> groups = rawGroups
            .Select(x => new DashboardBatchQAStatusDto
            {
                StatusName = GetBatchQAStatusLabel(x.Status),
                Count = x.Count,
                Percentage = Math.Round((decimal)x.Count / total * 100m, 2)
            })
            .ToList();

        return groups;
    }

    public async Task<List<DashboardPhysicalMovementTrendDto>> GetPhysicalMovementTrendsAsync(DashboardFilterInput input)
    {
        int daysCount = input.Days ?? 10;
        DateTime today = DateTime.Now.Date;
        DateTime startDate = today.AddDays(-daysCount + 1);

        IQueryable<InventoryTransaction> query = await _transactionRepository.GetQueryableAsync();
        if (input.WarehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == input.WarehouseId.Value);
        }
        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.Product.CategoryId == input.CategoryId.Value);
        }
        query = query.Where(x => x.CreationTime >= startDate);

        var txData = await query
            .GroupBy(x => new { Date = x.CreationTime.Date, Type = x.TransactionType })
            .Select(g => new
            {
                Date = g.Key.Date,
                Type = g.Key.Type,
                Volume = g.Sum(x => x.QuantityChanged * x.Product.BaseUnitVolume)
            })
            .ToListAsync();

        HashSet<InventoryTransactionType> inboundTypes = new()
        {
            InventoryTransactionType.PurchaseReceipt,
            InventoryTransactionType.ReturnInward,
            InventoryTransactionType.RecallReceipt,
            InventoryTransactionType.AdjustmentIn,
            InventoryTransactionType.TransferIn
        };

        var dailyData = txData
            .GroupBy(x => x.Date)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    InboundVolume = g.Where(x => inboundTypes.Contains(x.Type)).Sum(x => x.Volume),
                    OutboundVolume = g.Where(x => !inboundTypes.Contains(x.Type)).Sum(x => x.Volume)
                }
            );

        List<DashboardPhysicalMovementTrendDto> trends = new();

        for (int i = daysCount - 1; i >= 0; i--)
        {
            DateTime date = today.AddDays(-i);
            string dateStr = date.ToString("dd/MM");

            decimal inboundVol = 0m;
            decimal outboundVol = 0m;

            if (dailyData.TryGetValue(date, out var val))
            {
                inboundVol = val.InboundVolume;
                outboundVol = val.OutboundVolume;
            }

            trends.Add(new DashboardPhysicalMovementTrendDto
            {
                Date = dateStr,
                InboundVolume = inboundVol,
                OutboundVolume = outboundVol
            });
        }

        return trends;
    }

    #region Helpers
    private static string GetSalesStatusLabel(SalesOrderStatus status)
    {
        return status switch
        {
            SalesOrderStatus.Draft => "Nháp",
            SalesOrderStatus.PendingApproval => "Chờ duyệt",
            SalesOrderStatus.Approved => "Đã duyệt",
            SalesOrderStatus.Delivering => "Đang giao hàng",
            SalesOrderStatus.Completed => "Hoàn tất",
            SalesOrderStatus.Canceled => "Đã hủy",
            _ => status.ToString()
        };
    }

    private static string GetPurchaseStatusLabel(PurchaseOrderStatus status)
    {
        return status switch
        {
            PurchaseOrderStatus.Draft => "Nháp",
            PurchaseOrderStatus.PendingApproval => "Chờ duyệt",
            PurchaseOrderStatus.Approved => "Đã duyệt",
            PurchaseOrderStatus.Receiving => "Đang nhận hàng",
            PurchaseOrderStatus.Completed => "Hoàn tất",
            PurchaseOrderStatus.Canceled => "Đã hủy",
            _ => status.ToString()
        };
    }

    private static string GetApprovalStatusLabel(ApprovalStatus status)
    {
        return status switch
        {
            ApprovalStatus.Draft => "Nháp",
            ApprovalStatus.Pending => "Chờ duyệt",
            ApprovalStatus.Approved => "Đã duyệt",
            ApprovalStatus.Rejected => "Từ chối",
            _ => status.ToString()
        };
    }

    private static string GetBatchQAStatusLabel(BatchQAStatus status)
    {
        return status switch
        {
            BatchQAStatus.PendingQA => "Chờ duyệt QA",
            BatchQAStatus.Approved => "Đạt chuẩn",
            BatchQAStatus.Rejected => "Bị từ chối",
            BatchQAStatus.Recalled => "Đã thu hồi",
            BatchQAStatus.Expired => "Đã hết hạn",
            _ => status.ToString()
        };
    }

    public async Task<List<DashboardBatchLookupDto>> GetBatchLookupAsync(string? filter)
    {
        IQueryable<ProductBatch> query = await _batchRepository.WithDetailsAsync(x => x.Product);
        if (!string.IsNullOrWhiteSpace(filter))
        {
            string cleanFilter = filter.Trim().ToLower();
            query = query.Where(x => x.BatchNumber.ToLower().Contains(cleanFilter) || x.Product.Name.ToLower().Contains(cleanFilter));
        }

        List<ProductBatch> list = await query.Take(30).ToListAsync();

        return list.Select(x => new DashboardBatchLookupDto
        {
            Id = x.Id,
            BatchNumber = x.BatchNumber,
            MedicineName = x.Product?.Name ?? string.Empty
        }).ToList();
    }

    public async Task<DashboardBatchTraceDto> GetBatchTraceDetailsAsync(Guid batchId)
    {
        IQueryable<ProductBatch> batchQuery = await _batchRepository.WithDetailsAsync(x => x.Product, x => x.Supplier);
        ProductBatch? batch = await AsyncExecuter.FirstOrDefaultAsync(batchQuery.Where(x => x.Id == batchId));
        if (batch == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(ProductBatch), batchId);
        }

        DashboardBatchTraceDto dto = new()
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            MedicineCode = batch.Product?.Code ?? string.Empty,
            MedicineName = batch.Product?.Name ?? string.Empty,
            ManufacturingDate = batch.ManufacturingDate,
            ExpiryDate = batch.ExpiryDate,
            Status = GetBatchQAStatusLabel(batch.Status),
            SupplierName = batch.Supplier?.Name ?? "N/A"
        };

        // 1. Lấy tồn kho hiện tại (Balances)
        IQueryable<InventoryBalance> balanceQuery = await _balanceRepository.WithDetailsAsync(
            x => x.Warehouse,
            x => x.BinBalances
        );
        List<InventoryBalance> balances = await AsyncExecuter.ToListAsync(balanceQuery.Where(x => x.ProductBatchId == batchId && x.Quantity > 0));

        // Cần lấy mã Bin
        List<Bin> bins = await _binRepository.GetListAsync();

        foreach (InventoryBalance? bal in balances)
        {
            foreach (InventoryBinBalance bb in bal.BinBalances)
            {
                if (bb.Quantity > 0)
                {
                    Bin? bin = bins.FirstOrDefault(b => b.Id == bb.BinId);
                    dto.Balances.Add(new DashboardBatchTraceBalanceDto
                    {
                        WarehouseName = bal.Warehouse?.Name ?? "N/A",
                        BinCode = bin?.Code ?? "N/A",
                        Quantity = bb.Quantity
                    });
                }
            }
        }

        dto.TotalOnHand = balances.Sum(x => x.Quantity);

        // 2. Lấy số lượng giữ chỗ (Reservations)
        IQueryable<InventoryReservation> reservationQuery = await _reservationRepository.GetQueryableAsync();
        List<InventoryReservation> reservations = await AsyncExecuter.ToListAsync(reservationQuery.Where(x => x.ProductBatchId == batchId && x.Status == ReservationStatus.Active));
        dto.TotalReserved = reservations.Sum(x => x.ReservedQuantity);

        // 3. Lấy lịch sử giao dịch kho
        IQueryable<InventoryTransaction> transactionQuery = await _transactionRepository.GetQueryableAsync();
        List<InventoryTransaction> transactions = await AsyncExecuter.ToListAsync(transactionQuery.Where(x => x.ProductBatchId == batchId));

        foreach (InventoryTransaction? tx in transactions.OrderByDescending(x => x.CreationTime))
        {
            if (tx.TransactionType == InventoryTransactionType.PurchaseReceipt)
            {
                dto.Receipts.Add(new DashboardBatchTraceReceiptDto
                {
                    SupplierName = tx.PartnerName ?? "N/A",
                    TicketNumber = tx.ReferenceDocumentNumber ?? "N/A",
                    PoNumber = tx.SourceDocumentNumber ?? "N/A",
                    Date = tx.CreationTime,
                    Quantity = tx.QuantityChanged
                });
            }
            else if (tx.TransactionType == InventoryTransactionType.SaleDelivery)
            {
                dto.Deliveries.Add(new DashboardBatchTraceDeliveryDto
                {
                    CustomerName = tx.PartnerName ?? "N/A",
                    TicketNumber = tx.ReferenceDocumentNumber ?? "N/A",
                    SoNumber = tx.SourceDocumentNumber ?? "N/A",
                    Date = tx.CreationTime,
                    Quantity = tx.QuantityChanged
                });
            }
            else
            {
                dto.OtherTransactions.Add(new DashboardBatchTraceOtherDto
                {
                    TransactionType = GetTransactionTypeLabel(tx.TransactionType),
                    TicketNumber = tx.ReferenceDocumentNumber ?? "N/A",
                    Date = tx.CreationTime,
                    Quantity = tx.QuantityChanged,
                    Note = tx.Note ?? "N/A"
                });
            }
        }

        return dto;
    }

    private static string GetTransactionTypeLabel(InventoryTransactionType type)
    {
        return type switch
        {
            InventoryTransactionType.PurchaseReceipt => "Nhập mua hàng",
            InventoryTransactionType.SaleDelivery => "Xuất bán",
            InventoryTransactionType.ReturnInward => "Nhập trả từ khách",
            InventoryTransactionType.ReturnOutward => "Xuất trả cho NCC",
            InventoryTransactionType.RecallReceipt => "Nhập thu hồi",
            InventoryTransactionType.Disposal => "Xuất hủy",
            InventoryTransactionType.AdjustmentIn => "Điều chỉnh tăng",
            InventoryTransactionType.AdjustmentOut => "Điều chỉnh giảm",
            InventoryTransactionType.TransferIn => "Nhận chuyển kho",
            InventoryTransactionType.TransferOut => "Xuất chuyển kho",
            _ => type.ToString()
        };
    }
    #endregion
}
