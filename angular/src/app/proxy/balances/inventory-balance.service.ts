import type { GetInventoryBalanceListDto, GetInventoryReservationListDto, InventoryBalanceDetailDto, InventoryBalanceDto, InventoryReservationDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryBalanceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryBalanceDetailDto>({
      method: 'GET',
      url: `/api/app/inventory-balance/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetInventoryBalanceListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryBalanceDto>>({
      method: 'GET',
      url: '/api/app/inventory-balance',
      params: { warehouseId: input.warehouseId, binId: input.binId, productId: input.productId, productBatchId: input.productBatchId, batchNumber: input.batchNumber, isNearExpiry: input.isNearExpiry, hideZeroQuantity: input.hideZeroQuantity, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getReservationList = (input: GetInventoryReservationListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryReservationDto>>({
      method: 'GET',
      url: '/api/app/inventory-balance/reservation-list',
      params: { referenceDocumentId: input.referenceDocumentId, referenceDocumentNumber: input.referenceDocumentNumber, warehouseId: input.warehouseId, binId: input.binId, productId: input.productId, productBatchId: input.productBatchId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}