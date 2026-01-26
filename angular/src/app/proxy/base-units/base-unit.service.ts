import type { BaseUnitDto, CreateUpdateBaseUnitDto, GetBaseUnitListDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class BaseUnitService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateBaseUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BaseUnitDto>({
      method: 'POST',
      url: '/api/app/base-unit',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/base-unit/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BaseUnitDto>({
      method: 'GET',
      url: `/api/app/base-unit/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetBaseUnitListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<BaseUnitDto>>({
      method: 'GET',
      url: '/api/app/base-unit',
      params: { filter: input.filter },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateBaseUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, BaseUnitDto>({
      method: 'PUT',
      url: `/api/app/base-unit/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}