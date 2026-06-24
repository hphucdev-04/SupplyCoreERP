import type { PurchaseReturnType } from '../../enums/orders/purchase-return-type.enum';
import type { AuditedEntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PurchaseReturnRequestStatus } from '../../enums/orders/purchase-return-request-status.enum';

export interface AddPurchaseReturnRequestLineDto {
  returnType: PurchaseReturnType;
  productId: string;
  unitId: string;
  conversionFactor: number;
  purchaseOrderId: string;
  purchaseOrderLineId: string;
  quantity: number;
  originalUnitPrice: number;
  depreciationRate: number;
  taxRate: number;
}

export interface CreatePurchaseReturnRequestDto {
  warehouseId: string;
  requestDate: string;
  note?: string;
}

export interface GetPurchaseReturnRequestListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  supplierId?: string;
  warehouseId?: string;
  status?: PurchaseReturnRequestStatus;
}

export interface PurchaseReturnRequestDto extends FullAuditedEntityDto<string> {
  code?: string;
  warehouseId?: string;
  warehouseName?: string;
  warehouseCode?: string;
  requestDate?: string;
  status?: PurchaseReturnRequestStatus;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  note?: string;
  lines?: PurchaseReturnRequestLineDto[];
  relatedTickets?: PurchaseReturnRequestRelatedTicketDto[];
}

export interface PurchaseReturnRequestLineDto extends AuditedEntityDto<string> {
  purchaseReturnRequestId?: string;
  productId?: string;
  productCode?: string;
  productName?: string;
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  purchaseOrderId?: string;
  purchaseOrderCode?: string;
  purchaseOrderLineId?: string;
  returnType?: PurchaseReturnType;
  supplierId?: string;
  supplierName?: string;
  supplierCode?: string;
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

export interface PurchaseReturnRequestRelatedTicketDto {
  id?: string;
  ticketNumber?: string;
  type?: number;
  status?: number;
  creationTime?: string;
}

export interface UpdatePurchaseReturnRequestDto {
  warehouseId: string;
  requestDate: string;
  note?: string;
}

export interface UpdatePurchaseReturnRequestLineDto {
  returnType: PurchaseReturnType;
  quantity: number;
  depreciationRate: number;
}
