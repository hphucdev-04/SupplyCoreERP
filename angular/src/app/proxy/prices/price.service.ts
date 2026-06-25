import type { CreateUpdateProductPriceDto, PriceListDto, ProductCostReferenceDto, ProductPriceDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PriceService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  create = (input: CreateUpdateProductPriceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductPriceDto>({
      method: 'POST',
      url: '/api/app/price',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/price/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getByProduct = (productId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductPriceDto[]>({
      method: 'GET',
      url: `/api/app/price/by-product/${productId}`,
    },
    { apiName: this.apiName,...config });
  

  getCostReference = (productId: string, unitId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductCostReferenceDto>({
      method: 'GET',
      url: '/api/app/price/cost-reference',
      params: { productId, unitId },
    },
    { apiName: this.apiName,...config });
  

  getPriceLists = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PriceListDto[]>({
      method: 'GET',
      url: '/api/app/price/price-lists',
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateProductPriceDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ProductPriceDto>({
      method: 'PUT',
      url: `/api/app/price/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}