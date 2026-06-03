import type { TicketType } from '../../enums/warehouses/ticket-type.enum';
import type { AuditedEntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ApprovalStatus } from '../../enums/warehouses/approval-status.enum';

export interface AddTicketDetailDto {
  productId: string;
  productBatchId: string;
  binId: string;
  unitId: string;
  conversionFactor: number;
  quantity: number;
}

export interface CreateInventoryTicketDto {
  type: TicketType;
  warehouseId: string;
  referenceDocumentId?: string;
  referenceDocumentNumber?: string;
  note?: string;
}

export interface GetInventoryTicketListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  type?: TicketType;
  status?: ApprovalStatus;
  warehouseId?: string;
  referenceDocumentId?: string;
}

export interface InventoryTicketDetailDto extends FullAuditedEntityDto<string> {
  productId?: string;
  productName?: string;
  productCode?: string;
  baseUnitName?: string;
  productBatchId?: string;
  batchNumber?: string;
  batchCode?: string;
  manufacturingDate?: string;
  expiryDate?: string;
  registrationNumber?: string;
  binId?: string;
  binCode?: string;
  unitId?: string;
  unitName?: string;
  quantity?: number;
  conversionFactor?: number;
  baseQuantity?: number;
}

export interface InventoryTicketDto extends FullAuditedEntityDto<string> {
  ticketNumber?: string;
  type?: TicketType;
  status?: ApprovalStatus;
  warehouseId?: string;
  warehouseName?: string;
  referenceDocumentId?: string;
  referenceDocumentNumber?: string;
  note?: string;
  lines?: InventoryTicketLineDto[];
}

export interface InventoryTicketLineDto extends AuditedEntityDto<string> {
  productId?: string;
  productCode?: string;
  productName?: string;
  baseUnitName?: string;
  referenceDocumentLineId?: string;
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  quantity?: number;
  baseQuantity?: number;
  details?: InventoryTicketDetailDto[];
}

export interface UpdateInventoryTicketDto {
  note?: string;
}
