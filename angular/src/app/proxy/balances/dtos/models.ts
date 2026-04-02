import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface GetInventoryBalanceListDto extends PagedAndSortedResultRequestDto {
  warehouseId?: string;
  binId?: string;
  productId?: string;
  batchNumber?: string;
  isNearExpiry?: boolean;
  hideZeroQuantity?: boolean;
}

export interface InventoryBalanceDetailDto extends InventoryBalanceDto {
  warehouseAddress?: string;
  cityName?: string;
  areaName?: string;
  productCode?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  supplierName?: string;
}

export interface InventoryBalanceDto extends FullAuditedEntityDto<string> {
  warehouseId?: string;
  warehouseName?: string;
  binId?: string;
  binCode?: string;
  productId?: string;
  productName?: string;
  productBatchId?: string;
  batchNumber?: string;
  quantity?: number;
  lockedQuantity?: number;
  availableQuantity?: number;
}
