import type { RecallLevel } from '../../enums/orders/recall-level.enum';
import type { AuditedEntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { SalesRecallStatus } from '../../enums/orders/sales-recall-status.enum';
import type { TicketType } from '../../enums/warehouses/ticket-type.enum';
import type { ApprovalStatus } from '../../enums/warehouses/approval-status.enum';

export interface AddSalesRecallLineDto {
  customerId: string;
  salesOrderId: string;
  unitId: string;
  conversionFactor: number;
  quantity: number;
  originalUnitPrice: number;
  taxRate: number;
}

export interface CreateSalesRecallDto {
  recallDecisionNumber: string;
  productId: string;
  productBatchId?: string;
  warehouseId: string;
  recallDate: string;
  level: RecallLevel;
  note?: string;
}

export interface CustomerRecallTraceDto {
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  salesOrderId?: string;
  salesOrderCode?: string;
  salesOrderDate?: string;
  productBatchId?: string;
  batchNumber?: string;
  quantity?: number;
  unitName?: string;
}

export interface GetSalesRecallListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  customerId?: string;
  warehouseId?: string;
  status?: SalesRecallStatus;
}

export interface SalesRecallDto extends FullAuditedEntityDto<string> {
  code?: string;
  recallDecisionNumber?: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  productBatchId?: string;
  batchNumber?: string;
  warehouseId?: string;
  warehouseName?: string;
  warehouseCode?: string;
  recallDate?: string;
  level?: RecallLevel;
  deadline?: string;
  status?: SalesRecallStatus;
  totalAmount?: number;
  note?: string;
  isOverdue?: boolean;
  lines?: SalesRecallLineDto[];
  relatedTickets?: SalesRecallRelatedTicketDto[];
}

export interface SalesRecallLineDto extends AuditedEntityDto<string> {
  customerId?: string;
  customerCode?: string;
  customerName?: string;
  salesOrderId?: string;
  salesOrderCode?: string;
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  quantity?: number;
  baseQuantity?: number;
  originalUnitPrice?: number;
  taxRate?: number;
  totalPrice?: number;
  taxAmount?: number;
  finalPrice?: number;
}

export interface SalesRecallRelatedTicketDto {
  id?: string;
  ticketNumber?: string;
  type?: TicketType;
  status?: ApprovalStatus;
  creationTime?: string;
}

export interface UpdateSalesRecallDto {
  warehouseId: string;
  recallDate: string;
  level: RecallLevel;
  recallDecisionNumber: string;
  note?: string;
}

export interface UpdateSalesRecallLineDto {
  quantity: number;
}
