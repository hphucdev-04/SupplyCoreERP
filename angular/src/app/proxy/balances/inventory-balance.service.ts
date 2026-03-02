import type { GetInventoryBalanceListDto, InventoryBalanceDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryBalanceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getList = (input: GetInventoryBalanceListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryBalanceDto>>({
      method: 'GET',
      url: '/api/app/inventory-balance',
      params: { warehouseId: input.warehouseId, binId: input.binId, productId: input.productId, batchNumber: input.batchNumber, isNearExpiry: input.isNearExpiry, hideZeroQuantity: input.hideZeroQuantity, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}