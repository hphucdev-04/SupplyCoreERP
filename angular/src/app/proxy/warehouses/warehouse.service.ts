import type { BinDto, CreateUpdateBinDto, CreateUpdateWarehouseDto, CreateUpdateZoneDto, GetWarehouseListDto, WarehouseDto, ZoneDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WarehouseService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/warehouse/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateWarehouseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseDto>({
      method: 'POST',
      url: '/api/app/warehouse',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createStorageBin = (input: CreateUpdateBinDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BinDto>({
      method: 'POST',
      url: '/api/app/warehouse/storage-bin',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  createZone = (input: CreateUpdateZoneDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ZoneDto>({
      method: 'POST',
      url: '/api/app/warehouse/zone',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/warehouse/${id}`,
    },
    { apiName: this.apiName,...config });
  

  deleteStorageBin = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/warehouse/${id}/storage-bin`,
    },
    { apiName: this.apiName,...config });
  

  deleteZone = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/warehouse/${id}/zone`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseDto>({
      method: 'GET',
      url: `/api/app/warehouse/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetWarehouseListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<WarehouseDto>>({
      method: 'GET',
      url: '/api/app/warehouse',
      params: { filter: input.filter, status: input.status, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getStorageBin = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BinDto>({
      method: 'GET',
      url: `/api/app/warehouse/${id}/storage-bin`,
    },
    { apiName: this.apiName,...config });
  

  getStorageBins = (warehouseId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BinDto[]>({
      method: 'GET',
      url: `/api/app/warehouse/storage-bins/${warehouseId}`,
    },
    { apiName: this.apiName,...config });
  

  getZone = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ZoneDto>({
      method: 'GET',
      url: `/api/app/warehouse/${id}/zone`,
    },
    { apiName: this.apiName,...config });
  

  getZones = (warehouseId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ZoneDto[]>({
      method: 'GET',
      url: `/api/app/warehouse/zones/${warehouseId}`,
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/warehouse/${id}/reject`,
    },
    { apiName: this.apiName,...config });
  

  toggleActive = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/warehouse/${id}/toggle-active`,
    },
    { apiName: this.apiName,...config });
  

  toggleBinBlock = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/warehouse/${id}/toggle-bin-block`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateWarehouseDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, WarehouseDto>({
      method: 'PUT',
      url: `/api/app/warehouse/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateStorageBin = (id: string, input: CreateUpdateBinDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BinDto>({
      method: 'PUT',
      url: `/api/app/warehouse/${id}/storage-bin`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateZone = (id: string, input: CreateUpdateZoneDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ZoneDto>({
      method: 'PUT',
      url: `/api/app/warehouse/${id}/zone`,
      body: input,
    },
    { apiName: this.apiName,...config });
}