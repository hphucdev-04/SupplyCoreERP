import type { CreateUpdateProductBatchDto, GetProductBatchListDto, ProductBatchDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ProductBatchService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  approveQA = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/product-batch/${id}/approve-qA`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateProductBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductBatchDto>({
      method: 'POST',
      url: '/api/app/product-batch',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/product-batch/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductBatchDto>({
      method: 'GET',
      url: `/api/app/product-batch/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetProductBatchListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ProductBatchDto>>({
      method: 'GET',
      url: '/api/app/product-batch',
      params: { filter: input.filter, productId: input.productId, supplierId: input.supplierId, status: input.status, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  recall = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/product-batch/${id}/recall`,
    },
    { apiName: this.apiName,...config });
  

  rejectQA = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/product-batch/${id}/reject-qA`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateProductBatchDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductBatchDto>({
      method: 'PUT',
      url: `/api/app/product-batch/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}