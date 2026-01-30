import type { ActiveIngredientDto, CreateUpdateActiveIngredientDto, GetActiveIngredientListDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ActiveIngredientService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateActiveIngredientDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ActiveIngredientDto>({
      method: 'POST',
      url: '/api/app/active-ingredient',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/active-ingredient/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ActiveIngredientDto>({
      method: 'GET',
      url: `/api/app/active-ingredient/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetActiveIngredientListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ActiveIngredientDto>>({
      method: 'GET',
      url: '/api/app/active-ingredient',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateActiveIngredientDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ActiveIngredientDto>({
      method: 'PUT',
      url: `/api/app/active-ingredient/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}