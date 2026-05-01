import type { GetMedicineListDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class MedicineExcelService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  getImportTemplate = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'GET',
      responseType: 'blob',
      url: '/api/app/medicine-excel/import-template',
    },
    { apiName: this.apiName,...config });
  

  getListAsExcelFile = (input: GetMedicineListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, Blob>({
      method: 'GET',
      responseType: 'blob',
      url: '/api/app/medicine-excel/as-excel-file',
      params: { filter: input.filter, categoryId: input.categoryId, manufacturerId: input.manufacturerId, status: input.status, isActive: input.isActive, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  importExcel = (file: FormData, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/medicine-excel/import-excel',
      body: file,
    },
    { apiName: this.apiName,...config });
}