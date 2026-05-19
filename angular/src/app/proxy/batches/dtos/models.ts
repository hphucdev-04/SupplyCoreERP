import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { BatchQAStatus } from '../../enums/warehouses/batch-qastatus.enum';

export interface CreateUpdateProductBatchDto {
  productId: string;
  batchNumber: string;
  manufacturingDate: string;
  expiryDate: string;
  supplierId?: string;
  medicineRegistrationId?: string;
}

export interface GetProductBatchListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  productId?: string;
  supplierId?: string;
  status?: BatchQAStatus;
}

export interface ProductBatchDto extends FullAuditedEntityDto<string> {
  code?: string;
  productId?: string;
  productName?: string;
  productCode?: string;
  batchNumber?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  supplierId?: string;
  supplierName?: string;
  status?: BatchQAStatus;
  registrationNumber?: string;
}
