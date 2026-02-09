import type { CreateUpdateMedicineDto, CreateUpdateMedicineIngredientDto, CreateUpdateMedicineUnitDto, GetMedicineListDto, MedicineDetailDto, MedicineDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MedicineService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  addIngredient = (id: string, input: CreateUpdateMedicineIngredientDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/medicine/${id}/ingredient`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  addUnit = (id: string, input: CreateUpdateMedicineUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/medicine/${id}/unit`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  approve = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/medicine/${id}/approve`,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateMedicineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MedicineDetailDto>({
      method: 'POST',
      url: '/api/app/medicine',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/medicine/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MedicineDetailDto>({
      method: 'GET',
      url: `/api/app/medicine/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetMedicineListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<MedicineDto>>({
      method: 'GET',
      url: '/api/app/medicine',
      params: { filter: input.filter, categoryId: input.categoryId, manufacturerId: input.manufacturerId, status: input.status, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  getListAsExcelFile = (input: GetMedicineListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'GET',
      responseType: 'blob',
      url: '/api/app/medicine/as-excel-file',
      params: { filter: input.filter, categoryId: input.categoryId, manufacturerId: input.manufacturerId, status: input.status, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  reject = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/medicine/${id}/reject`,
    },
    { apiName: this.apiName,...config });
  

  removeIngredient = (id: string, activeIngredientId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/medicine/${id}/ingredient/${activeIngredientId}`,
    },
    { apiName: this.apiName,...config });
  

  removeUnit = (id: string, unitId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/medicine/${id}/unit/${unitId}`,
    },
    { apiName: this.apiName,...config });
  

  toggleActive = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/medicine/${id}/toggle-active`,
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateMedicineDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, MedicineDetailDto>({
      method: 'PUT',
      url: `/api/app/medicine/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  updateUnit = (id: string, unitId: string, input: CreateUpdateMedicineUnitDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'PUT',
      url: `/api/app/medicine/${id}/unit/${unitId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
}