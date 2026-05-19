import type { CreationAuditedEntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ReservationStatus } from '../../enums/balances/reservation-status.enum';

export interface GetInventoryBalanceListDto extends PagedAndSortedResultRequestDto {
  warehouseId?: string;
  binId?: string;
  productId?: string;
  productBatchId?: string;
  batchNumber?: string;
  isNearExpiry?: boolean;
  hideZeroQuantity?: boolean;
}

export interface GetInventoryReservationListDto extends PagedAndSortedResultRequestDto {
  referenceDocumentId?: string;
  referenceDocumentNumber?: string;
  warehouseId?: string;
  binId?: string;
  productId?: string;
  productBatchId?: string;
  status?: ReservationStatus;
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
  baseUnitName?: string;
  quantity?: number;
  lockedQuantity?: number;
  availableQuantity?: number;
}

export interface InventoryReservationDto extends CreationAuditedEntityDto<string> {
  referenceDocumentId?: string;
  referenceDocumentNumber?: string;
  warehouseId?: string;
  warehouseName?: string;
  binId?: string;
  binCode?: string;
  productId?: string;
  productBatchId?: string;
  reservedQuantity?: number;
  status?: ReservationStatus;
}
