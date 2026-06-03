import type { AddPurchaseReturnLineDto, CreatePurchaseReturnDto, GetPurchaseReturnListDto, PurchaseReturnDto, UpdatePurchaseReturnDto, UpdatePurchaseReturnLineDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PurchaseReturnService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addLine = (returnId: string, input: AddPurchaseReturnLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return/line/${returnId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreatePurchaseReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseReturnDto>({
      method: 'POST',
      url: '/api/app/purchase-return',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/purchase-return/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseReturnDto>({
      method: 'GET',
      url: `/api/app/purchase-return/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetPurchaseReturnListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchaseReturnDto>>({
      method: 'GET',
      url: '/api/app/purchase-return',
      params: { filter: input.filter, supplierId: input.supplierId, warehouseId: input.warehouseId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return/${id}/reject`,
    },
    { apiName: this.apiName,...config });
  

  removeLine = (returnId: string, lineId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/purchase-return/line',
      params: { returnId, lineId },
    },
    { apiName: this.apiName,...config });
  

  sendToApprove = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-return/${id}/send-to-approve`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdatePurchaseReturnDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseReturnDto>({
      method: 'PUT',
      url: `/api/app/purchase-return/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (returnId: string, lineId: string, input: UpdatePurchaseReturnLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/purchase-return/line',
      params: { returnId, lineId },
      body: input,
    },
    { apiName: this.apiName,...config });
}