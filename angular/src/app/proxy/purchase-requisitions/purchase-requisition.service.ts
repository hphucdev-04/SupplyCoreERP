import type { AddPurchaseRequisitionLineDto, ConvertToPurchaseOrderDto, CreatePurchaseRequisitionDto, GetPurchaseRequisitionListDto, PurchaseRequisitionDto, UpdatePurchaseRequisitionDto, UpdatePurchaseRequisitionLineDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PurchaseRequisitionService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addLine = (requisitionId: string, input: AddPurchaseRequisitionLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-requisition/line/${requisitionId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-requisition/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  convertToPurchaseOrder = (id: string, input: ConvertToPurchaseOrderDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-requisition/${id}/convert-to-purchase-order`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreatePurchaseRequisitionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>({
      method: 'POST',
      url: '/api/app/purchase-requisition',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/purchase-requisition/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>({
      method: 'GET',
      url: `/api/app/purchase-requisition/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetPurchaseRequisitionListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<PurchaseRequisitionDto>>({
      method: 'GET',
      url: '/api/app/purchase-requisition',
      params: { filter: input.filter, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-requisition/${id}/reject`,
    },
    { apiName: this.apiName,...config });
  

  removeLine = (requisitionId: string, lineId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/purchase-requisition/line',
      params: { requisitionId, lineId },
    },
    { apiName: this.apiName,...config });
  

  sendToApprove = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/purchase-requisition/${id}/send-to-approve`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdatePurchaseRequisitionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseRequisitionDto>({
      method: 'PUT',
      url: `/api/app/purchase-requisition/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateLine = (requisitionId: string, lineId: string, input: UpdatePurchaseRequisitionLineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: '/api/app/purchase-requisition/line',
      params: { requisitionId, lineId },
      body: input,
    },
    { apiName: this.apiName,...config });
}