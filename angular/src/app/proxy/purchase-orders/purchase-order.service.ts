import type { AddPurchaseOrderDetailDto, CreatePurchaseOrderDto, GetPurchaseOrderListDto, PurchaseOrderDto, UpdatePurchaseOrderDetailDto, UpdatePurchaseOrderDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PurchaseOrderService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addDetail = (orderId: string, input: AddPurchaseOrderDetailDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-order/detail/${orderId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-order/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  cancel = (id: string, reason: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-order/${id}/cancel`,
      params: { reason },
    },
    { apiName: this.apiName,...config });
  

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-order/${id}/complete`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreatePurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderDto>({
      method: 'POST',
      url: '/api/app/purchase-order',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/purchase-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderDto>({
      method: 'GET',
      url: `/api/app/purchase-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetPurchaseOrderListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchaseOrderDto>>({
      method: 'GET',
      url: '/api/app/purchase-order',
      params: { filter: input.filter, supplierId: input.supplierId, warehouseId: input.warehouseId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  removeDetail = (orderId: string, detailId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/purchase-order/detail',
      params: { orderId, detailId },
    },
    { apiName: this.apiName,...config });
  

  sendToApprove = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-order/${id}/send-to-approve`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdatePurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderDto>({
      method: 'PUT',
      url: `/api/app/purchase-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateDetail = (orderId: string, detailId: string, input: UpdatePurchaseOrderDetailDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/purchase-order/detail',
      params: { orderId, detailId },
      body: input,
    },
    { apiName: this.apiName,...config });
}