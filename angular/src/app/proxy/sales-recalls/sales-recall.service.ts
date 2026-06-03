import type { AddSalesRecallLineDto, CreateSalesRecallDto, CustomerRecallTraceDto, GetSalesRecallListDto, SalesRecallDto, UpdateSalesRecallDto, UpdateSalesRecallLineDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SalesRecallService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addLine = (recallId: string, input: AddSalesRecallLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-recall/line/${recallId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-recall/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateSalesRecallDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesRecallDto>({
      method: 'POST',
      url: '/api/app/sales-recall',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/sales-recall/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesRecallDto>({
      method: 'GET',
      url: `/api/app/sales-recall/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetSalesRecallListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SalesRecallDto>>({
      method: 'GET',
      url: '/api/app/sales-recall',
      params: { filter: input.filter, customerId: input.customerId, warehouseId: input.warehouseId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-recall/${id}/reject`,
    },
    { apiName: this.apiName,...config });
  

  removeLine = (recallId: string, lineId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/sales-recall/line',
      params: { recallId, lineId },
    },
    { apiName: this.apiName,...config });
  

  sendToApprove = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/sales-recall/${id}/send-to-approve`,
    },
    { apiName: this.apiName,...config });
  

  traceCustomersByBatch = (productBatchId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerRecallTraceDto[]>({
      method: 'POST',
      url: `/api/app/sales-recall/trace-customers-by-batch/${productBatchId}`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateSalesRecallDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SalesRecallDto>({
      method: 'PUT',
      url: `/api/app/sales-recall/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (recallId: string, lineId: string, input: UpdateSalesRecallLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/sales-recall/line',
      params: { recallId, lineId },
      body: input,
    },
    { apiName: this.apiName,...config });
}