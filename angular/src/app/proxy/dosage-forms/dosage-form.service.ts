import type { CreateUpdateDosageFormDto, DosageFormDto, GetDosageFormListDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DosageFormService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateDosageFormDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DosageFormDto>({
      method: 'POST',
      url: '/api/app/dosage-form',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/dosage-form/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DosageFormDto>({
      method: 'GET',
      url: `/api/app/dosage-form/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetDosageFormListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<DosageFormDto>>({
      method: 'GET',
      url: '/api/app/dosage-form',
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateDosageFormDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, DosageFormDto>({
      method: 'PUT',
      url: `/api/app/dosage-form/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}