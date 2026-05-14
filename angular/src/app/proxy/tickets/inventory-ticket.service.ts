import type { AddTicketDetailDto, CreateInventoryTicketDto, GetInventoryTicketListDto, InventoryTicketDto, UpdateInventoryTicketDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { PurchaseOrderLineDto } from '../purchase-orders/dtos/models';

@Injectable({
  providedIn: 'root',
})
export class InventoryTicketService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addDetail = (id: string, input: AddTicketDetailDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/inventory-ticket/${id}/detail`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  addLineFromPurchaseOrder = (id: string, poLineId: string, quantity: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/inventory-ticket/${id}/line-from-purchase-order/${poLineId}`,
      params: { quantity },
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateInventoryTicketDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryTicketDto>({
      method: 'POST',
      url: '/api/app/inventory-ticket',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/inventory-ticket/${id}`,
    },
    { apiName: this.apiName,...config });
  

  deleteDetail = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/inventory-ticket/${id}/detail`,
    },
    { apiName: this.apiName,...config });
  

  deleteLine = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/inventory-ticket/${id}/line`,
    },
    { apiName: this.apiName,...config });
  

  execute = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/inventory-ticket/${id}/execute`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryTicketDto>({
      method: 'GET',
      url: `/api/app/inventory-ticket/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getLinesFromPurchaseOrder = (poId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PurchaseOrderLineDto[]>({
      method: 'GET',
      url: `/api/app/inventory-ticket/lines-from-purchase-order/${poId}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetInventoryTicketListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryTicketDto>>({
      method: 'GET',
      url: '/api/app/inventory-ticket',
      params: { filter: input.filter, type: input.type, status: input.status, warehouseId: input.warehouseId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getRelatedTicketsByPurchaseOrder = (poId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryTicketDto[]>({
      method: 'GET',
      url: `/api/app/inventory-ticket/related-tickets-by-purchase-order/${poId}`,
    },
    { apiName: this.apiName,...config });
  

  sendToApprove = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/inventory-ticket/${id}/send-to-approve`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: UpdateInventoryTicketDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryTicketDto>({
      method: 'PUT',
      url: `/api/app/inventory-ticket/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}