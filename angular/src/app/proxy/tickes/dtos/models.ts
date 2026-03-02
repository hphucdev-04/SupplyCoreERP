import type { FullAuditedEntityDto } from '@abp/ng.core';
import type { TicketType } from '../../enums/warehouses/ticket-type.enum';
import type { ApprovalStatus } from '../../enums/warehouses/approval-status.enum';
import type { InventoryTicketDetailDto } from '../../tickets/dtos/models';

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
