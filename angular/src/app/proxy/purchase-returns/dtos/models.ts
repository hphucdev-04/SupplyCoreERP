import type { PurchaseReturnType } from '../../enums/orders/purchase-return-type.enum';
import type { AuditedEntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PurchaseReturnStatus } from '../../enums/orders/purchase-return-status.enum';
import type { TicketType } from '../../enums/warehouses/ticket-type.enum';
import type { ApprovalStatus } from '../../enums/warehouses/approval-status.enum';

export interface AddPurchaseReturnLineDto {
  purchaseOrderLineId: string;
  productId: string;
  unitId: string;
  conversionFactor: number;
  quantity: number;
  originalUnitPrice: number;
  depreciationRate: number;
  taxRate: number;
}

export interface CreatePurchaseReturnDto {
  purchaseOrderId: string;
  supplierId: string;
  warehouseId: string;
  returnType: PurchaseReturnType;
  returnDate: string;
  note?: string;
}

export interface GetPurchaseReturnListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  supplierId?: string;
  warehouseId?: string;
  status?: PurchaseReturnStatus;
}

export interface PurchaseReturnDto extends FullAuditedEntityDto<string> {
  code?: string;
  purchaseOrderId?: string;
  purchaseOrderCode?: string;
  supplierId?: string;
  supplierName?: string;
  supplierCode?: string;
  warehouseId?: string;
  warehouseName?: string;
  warehouseCode?: string;
  returnDate?: string;
  returnType?: PurchaseReturnType;
  status?: PurchaseReturnStatus;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  note?: string;
  lines?: PurchaseReturnLineDto[];
  relatedTickets?: PurchaseReturnRelatedTicketDto[];
}

export interface PurchaseReturnLineDto extends AuditedEntityDto<string> {
  purchaseOrderLineId?: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  quantity?: number;
  baseQuantity?: number;
  originalUnitPrice?: number;
  depreciationRate?: number;
  returnUnitPrice?: number;
  taxRate?: number;
  totalPrice?: number;
  taxAmount?: number;
  finalPrice?: number;
}

export interface PurchaseReturnRelatedTicketDto {
  id?: string;
  ticketNumber?: string;
  type?: TicketType;
  status?: ApprovalStatus;
  creationTime?: string;
}

export interface UpdatePurchaseReturnDto {
  warehouseId: string;
  returnType: PurchaseReturnType;
  returnDate: string;
  note?: string;
}

export interface UpdatePurchaseReturnLineDto {
  quantity: number;
  depreciationRate: number;
}
