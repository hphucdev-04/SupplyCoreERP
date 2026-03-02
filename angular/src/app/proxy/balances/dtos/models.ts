import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface GetInventoryBalanceListDto extends PagedAndSortedResultRequestDto {
  warehouseId?: string;
  binId?: string;
  productId?: string;
  batchNumber?: string;
  isNearExpiry?: boolean;
  hideZeroQuantity?: boolean;
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
  expiryDate?: string;
  quantity?: number;
  lockedQuantity?: number;
  availableQuantity?: number;
}
