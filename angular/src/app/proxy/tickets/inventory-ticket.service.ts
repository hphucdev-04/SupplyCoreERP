import type { AddTicketDetailDto, CreateInventoryTicketDto, GetInventoryTicketListDto, InventoryTicketDto, UpdateInventoryTicketDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class InventoryTicketService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  allocateFEFO = (id: string, productId: string, requiredBaseQuantity: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/inventory-ticket/${id}/allocate-fEFO/${productId}`,
      params: { requiredBaseQuantity },
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateInventoryTicketDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryTicketDto>({
      method: 'POST',
      url: '/api/app/inventory-ticket',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createTicketDetail = (ticketId: string, input: AddTicketDetailDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, InventoryTicketDto>({
      method: 'POST',
      url: `/api/app/inventory-ticket/ticket-detail/${ticketId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/inventory-ticket/${id}`,
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
  

  getList = (input: GetInventoryTicketListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<InventoryTicketDto>>({
      method: 'GET',
      url: '/api/app/inventory-ticket',
      params: { filter: input.filter, type: input.type, status: input.status, warehouseId: input.warehouseId, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, reason: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/inventory-ticket/${id}/reject`,
      params: { reason },
    },
    { apiName: this.apiName,...config });
  

  removeDetail = (ticketId: string, detailId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/inventory-ticket/detail',
      params: { ticketId, detailId },
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
  

  updateDetailQuantity = (detailId: string, actualQuantity: number, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/inventory-ticket/detail-quantity/${detailId}`,
      params: { actualQuantity },
    },
    { apiName: this.apiName,...config });
}