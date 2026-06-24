using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SupplyCoreERP.Dashboard.Dtos;
using Volo.Abp.Application.Services;

namespace SupplyCoreERP.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardOverviewDto> GetOverviewAsync(DashboardFilterInput input);

    Task<List<DashboardFinancialTrendDto>> GetFinancialTrendsAsync(DashboardFilterInput input);

    Task<List<DashboardSalesStatusDto>> GetSalesStatusDistributionAsync(DashboardFilterInput input);

    Task<List<DashboardProcurementStatusDto>> GetProcurementStatusDistributionAsync(DashboardFilterInput input);

    Task<List<DashboardWarehouseCapacityDto>> GetWarehouseCapacitiesAsync(DashboardFilterInput input);

    Task<List<DashboardInventoryTransactionDto>> GetInventoryTransactionDistributionAsync(DashboardFilterInput input);

    Task<List<DashboardCategoryDistributionDto>> GetMedicineCategoryDistributionAsync(DashboardFilterInput input);

    Task<List<DashboardExpiredBatchDto>> GetNearExpiryBatchesAsync(DashboardFilterInput input);

    Task<List<DashboardExpiredBatchDto>> GetAlreadyExpiredBatchesAsync(DashboardFilterInput input);

    Task<DashboardDebtOverviewDto> GetDebtOverviewAsync(DashboardFilterInput input);

    Task<List<DashboardPartnerDebtDto>> GetTopCustomerDebtsAsync(DashboardFilterInput input);

    Task<List<DashboardPartnerDebtDto>> GetTopSupplierDebtsAsync(DashboardFilterInput input);

    Task<List<DashboardInventoryTicketStatusDto>> GetInventoryTicketStatusDistributionAsync(DashboardFilterInput input);

    Task<List<DashboardBatchQAStatusDto>> GetBatchQAStatusDistributionAsync(DashboardFilterInput input);

    Task<List<DashboardPhysicalMovementTrendDto>> GetPhysicalMovementTrendsAsync(DashboardFilterInput input);

    Task<List<DashboardBatchLookupDto>> GetBatchLookupAsync(string? filter);

    Task<DashboardBatchTraceDto> GetBatchTraceDetailsAsync(Guid batchId);
}

