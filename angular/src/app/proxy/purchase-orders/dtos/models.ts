import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PurchaseOrderStatus } from '../../enums/orders/purchase-order-status.enum';

export interface AddPurchaseOrderDetailDto {
  productId: string;
  unitId: string;
  conversionFactor: number;
  quantity: number;
  unitPrice: number;
  taxRate?: number;
}

export interface CreatePurchaseOrderDto {
  supplierId: string;
  warehouseId: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  dueDate?: string;
  note?: string;
}

export interface GetPurchaseOrderListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  supplierId?: string;
  warehouseId?: string;
  status?: PurchaseOrderStatus;
}

export interface PurchaseOrderDetailDto extends FullAuditedEntityDto<string> {
  productId?: string;
  productCode?: string;
  productName?: string;
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  quantity?: number;
  baseQuantity?: number;
  receivedQuantity?: number;
  unitPrice?: number;
  taxRate?: number;
  totalPrice?: number;
  taxAmount?: number;
  finalPrice?: number;
}

export interface PurchaseOrderDto extends FullAuditedEntityDto<string> {
  code?: string;
  supplierId?: string;
  supplierName?: string;
  warehouseId?: string;
  warehouseName?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  dueDate?: string;
  status?: PurchaseOrderStatus;
  subTotal?: number;
  taxAmount?: number;
  totalAmount?: number;
  note?: string;
  details?: PurchaseOrderDetailDto[];
}

export interface UpdatePurchaseOrderDetailDto {
  quantity: number;
  unitPrice: number;
  taxRate?: number;
}

export interface UpdatePurchaseOrderDto {
  warehouseId: string;
  expectedDeliveryDate?: string;
  dueDate?: string;
  note?: string;
}
