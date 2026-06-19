import type { DashboardBatchLookupDto, DashboardBatchQAStatusDto, DashboardBatchTraceDto, DashboardCategoryDistributionDto, DashboardDebtOverviewDto, DashboardExpiredBatchDto, DashboardFilterInput, DashboardFinancialTrendDto, DashboardInventoryTicketStatusDto, DashboardInventoryTransactionDto, DashboardOverviewDto, DashboardPartnerDebtDto, DashboardPhysicalMovementTrendDto, DashboardProcurementStatusDto, DashboardSalesStatusDto, DashboardWarehouseCapacityDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getAlreadyExpiredBatches = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardExpiredBatchDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/already-expired-batches',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getBatchLookup = (filter: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardBatchLookupDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/batch-lookup',
      params: { filter },
    },
    { apiName: this.apiName,...config });
  

  getBatchQAStatusDistribution = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardBatchQAStatusDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/batch-qAStatus-distribution',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getBatchTraceDetails = (batchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardBatchTraceDto>({
      method: 'GET',
      url: `/api/app/dashboard/batch-trace-details/${batchId}`,
    },
    { apiName: this.apiName,...config });
  

  getDebtOverview = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardDebtOverviewDto>({
      method: 'GET',
      url: '/api/app/dashboard/debt-overview',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getFinancialTrends = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardFinancialTrendDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/financial-trends',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getInventoryTicketStatusDistribution = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardInventoryTicketStatusDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/inventory-ticket-status-distribution',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getInventoryTransactionDistribution = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardInventoryTransactionDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/inventory-transaction-distribution',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getMedicineCategoryDistribution = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardCategoryDistributionDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/medicine-category-distribution',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getNearExpiryBatches = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardExpiredBatchDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/near-expiry-batches',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getOverview = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardOverviewDto>({
      method: 'GET',
      url: '/api/app/dashboard/overview',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getPhysicalMovementTrends = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardPhysicalMovementTrendDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/physical-movement-trends',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getProcurementStatusDistribution = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardProcurementStatusDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/procurement-status-distribution',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getSalesStatusDistribution = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardSalesStatusDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/sales-status-distribution',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getTopCustomerDebts = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardPartnerDebtDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/top-customer-debts',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getTopSupplierDebts = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardPartnerDebtDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/top-supplier-debts',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
  

  getWarehouseCapacities = (input: DashboardFilterInput, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardWarehouseCapacityDto[]>({
      method: 'GET',
      url: '/api/app/dashboard/warehouse-capacities',
      params: { warehouseId: input.warehouseId, days: input.days, categoryId: input.categoryId },
    },
    { apiName: this.apiName,...config });
}