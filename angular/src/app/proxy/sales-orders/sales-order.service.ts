import type { AddSalesOrderLineDto, CreateSalesOrderDto, GetSalesOrderListDto, SalesOrderDto, UpdateSalesOrderDto, UpdateSalesOrderLineDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SalesOrderService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addLine = (orderId: string, input: AddSalesOrderLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-order/line/${orderId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-order/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  complete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-order/${id}/complete`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateSalesOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesOrderDto>({
      method: 'POST',
      url: '/api/app/sales-order',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/sales-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesOrderDto>({
      method: 'GET',
      url: `/api/app/sales-order/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetSalesOrderListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SalesOrderDto>>({
      method: 'GET',
      url: '/api/app/sales-order',
      params: { filter: input.filter, customerId: input.customerId, warehouseId: input.warehouseId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  removeLine = (orderId: string, lineId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/sales-order/line',
      params: { orderId, lineId },
    },
    { apiName: this.apiName,...config });
  

  sendToApprove = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-order/${id}/send-to-approve`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateSalesOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesOrderDto>({
      method: 'PUT',
      url: `/api/app/sales-order/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (orderId: string, lineId: string, input: UpdateSalesOrderLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/sales-order/line',
      params: { orderId, lineId },
      body: input,
    },
    { apiName: this.apiName,...config });
}