import type { CreateUpdateSupplierDto, CreateUpdateSupplierProductDto, GetSupplierListDto, GetSupplierMedicineListDto, GetSupplierProductListDto, SupplierDetailDto, SupplierDto, SupplierMedicineDto, SupplierProductDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class SupplierService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addProduct = (supplierId: string, input: CreateUpdateSupplierProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierProductDto>({
      method: 'POST',
      url: `/api/app/supplier/product/${supplierId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateSupplierDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierDetailDto>({
      method: 'POST',
      url: '/api/app/supplier',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/supplier/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierDetailDto>({
      method: 'GET',
      url: `/api/app/supplier/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetSupplierListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SupplierDto>>({
      method: 'GET',
      url: '/api/app/supplier',
      params: { filter: input.filter, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getProductList = (supplierId: string, input: GetSupplierProductListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SupplierProductDto>>({
      method: 'GET',
      url: `/api/app/supplier/product-list/${supplierId}`,
      params: { filter: input.filter, isPreferred: input.isPreferred, isActive: input.isActive, minPrice: input.minPrice, maxPrice: input.maxPrice, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getSupplierList = (productId: string, input: GetSupplierMedicineListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<SupplierMedicineDto>>({
      method: 'GET',
      url: `/api/app/supplier/supplier-list/${productId}`,
      params: { filter: input.filter, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  removeProduct = (supplierId: string, productId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: '/api/app/supplier/product',
      params: { supplierId, productId },
    },
    { apiName: this.apiName,...config });
  

  toggleActive = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/supplier/${id}/toggle-active`,
    },
    { apiName: this.apiName,...config });
  

  toggleProductActive = (supplierId: string, productId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/supplier/toggle-product-active',
      params: { supplierId, productId },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateSupplierDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierDetailDto>({
      method: 'PUT',
      url: `/api/app/supplier/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateProduct = (supplierId: string, productId: string, input: CreateUpdateSupplierProductDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, SupplierProductDto>({
      method: 'PUT',
      url: '/api/app/supplier/product',
      params: { supplierId, productId },
      body: input,
    },
    { apiName: this.apiName,...config });
}