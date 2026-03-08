import type { TicketType } from '../../enums/warehouses/ticket-type.enum';
import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { ApprovalStatus } from '../../enums/warehouses/approval-status.enum';

export interface AddTicketDetailDto {
  productId: string;
  productBatchId: string;
  binId: string;
  quantity: number;
}

export interface CreateInventoryTicketDto {
  type: TicketType;
  warehouseId: string;
  referenceDocumentId?: string;
  note?: string;
}

export interface GetInventoryTicketListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  type?: TicketType;
  status?: ApprovalStatus;
  warehouseId?: string;
}

export interface InventoryTicketDetailDto extends FullAuditedEntityDto<string> {
  ticketId?: string;
  productId?: string;
  productName?: string;
  productBatchId?: string;
  batchNumber?: string;
  binId?: string;
  binCode?: string;
  quantity?: number;
}

export interface InventoryTicketDto extends FullAuditedEntityDto<string> {
  ticketNumber?: string;
  type?: TicketType;
  status?: ApprovalStatus;
  warehouseId?: string;
  warehouseName?: string;
  referenceDocumentId?: string;
  note?: string;
  details?: InventoryTicketDetailDto[];
}

export interface UpdateInventoryTicketDto {
  note?: string;
}
