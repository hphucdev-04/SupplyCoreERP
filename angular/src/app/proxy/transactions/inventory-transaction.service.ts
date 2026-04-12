import type { GetInventoryTransactionListDto, InventoryTransactionDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryTransactionService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryTransactionDto>({
      method: 'GET',
      url: `/api/app/inventory-transaction/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetInventoryTransactionListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryTransactionDto>>({
      method: 'GET',
      url: '/api/app/inventory-transaction',
      params: { filter: input.filter, warehouseId: input.warehouseId, productId: input.productId, productBatchId: input.productBatchId, binId: input.binId, referenceDocumentId: input.referenceDocumentId, transactionType: input.transactionType, fromDate: input.fromDate, toDate: input.toDate, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
}