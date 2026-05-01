import type { CreateUpdateCustomerDto, CustomerDetailDto, CustomerDto, CustomerSummaryDto, GetCustomerListDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CustomerService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDetailDto>({
      method: 'POST',
      url: '/api/app/customer',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/customer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDetailDto>({
      method: 'GET',
      url: `/api/app/customer/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetCustomerListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<CustomerDto>>({
      method: 'GET',
      url: '/api/app/customer',
      params: { filter: input.filter, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSummary = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerSummaryDto>({
      method: 'GET',
      url: '/api/app/customer/summary',
    },
    { apiName: this.apiName,...config });
  

  toggleActive = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/customer/${id}/toggle-active`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateCustomerDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CustomerDetailDto>({
      method: 'PUT',
      url: `/api/app/customer/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}