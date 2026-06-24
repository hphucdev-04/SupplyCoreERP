import type { CreationAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { InventoryTransactionType } from '../../enums/warehouses/inventory-transaction-type.enum';

export interface GetInventoryTransactionListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  warehouseId?: string;
  productId?: string;
  productBatchId?: string;
  binId?: string;
  referenceDocumentId?: string;
  transactionType?: InventoryTransactionType;
  fromDate?: string;
  toDate?: string;
}

export interface InventoryTransactionDto extends CreationAuditedEntityDto<string> {
  warehouseId?: string;
  warehouseName?: string;
  productId?: string;
  productName?: string;
  productCode?: string;
  productBatchId?: string;
  batchNumber?: string;
  binId?: string;
  binCode?: string;
  transactionType?: InventoryTransactionType;
  quantity?: number;
  balanceAfterTransaction?: number;
  referenceDocumentId?: string;
  referenceDocumentNumber?: string;
  note?: string;
  partnerId?: string;
  partnerName?: string;
  sourceDocumentId?: string;
  sourceDocumentNumber?: string;
}
