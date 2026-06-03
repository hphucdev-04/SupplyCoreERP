import type { AddPurchaseReturnRequestLineDto, CreatePurchaseReturnRequestDto, GetPurchaseReturnRequestListDto, PurchaseReturnRequestDto, UpdatePurchaseReturnRequestDto, UpdatePurchaseReturnRequestLineDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PurchaseReturnRequestService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addLine = (requestId: string, input: AddPurchaseReturnRequestLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return-request/line/${requestId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  approveAndSplit = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return-request/${id}/approve-and-split`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreatePurchaseReturnRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseReturnRequestDto>({
      method: 'POST',
      url: '/api/app/purchase-return-request',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/purchase-return-request/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseReturnRequestDto>({
      method: 'GET',
      url: `/api/app/purchase-return-request/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetPurchaseReturnRequestListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchaseReturnRequestDto>>({
      method: 'GET',
      url: '/api/app/purchase-return-request',
      params: { filter: input.filter, supplierId: input.supplierId, warehouseId: input.warehouseId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return-request/${id}/reject`,
    },
    { apiName: this.apiName,...config });
  

  removeLine = (requestId: string, lineId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/purchase-return-request/line',
      params: { requestId, lineId },
    },
    { apiName: this.apiName,...config });
  

  sendToApprove = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return-request/${id}/send-to-approve`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdatePurchaseReturnRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseReturnRequestDto>({
      method: 'PUT',
      url: `/api/app/purchase-return-request/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (requestId: string, lineId: string, input: UpdatePurchaseReturnRequestLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/purchase-return-request/line',
      params: { requestId, lineId },
      body: input,
    },
    { apiName: this.apiName,...config });
}