import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { SalesOrderStatus } from '../../enums/orders/sales-order-status.enum';

export interface AddSalesOrderDetailDto {
  productId: string;
  unitId: string;
  conversionFactor: number;
  quantity: number;
  discountRate?: number;
  taxRate?: number;
}

export interface CreateSalesOrderDto {
  customerId: string;
  warehouseId: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  dueDate?: string;
  note?: string;
}

export interface GetSalesOrderListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  customerId?: string;
  warehouseId?: string;
  status?: SalesOrderStatus;
}

export interface SalesOrderDetailDto extends FullAuditedEntityDto<string> {
  productId?: string;
  productCode?: string;
  productName?: string;
  unitId?: string;
  unitName?: string;
  conversionFactor?: number;
  quantity?: number;
  baseQuantity?: number;
  deliveredQuantity?: number;
  unitPrice?: number;
  discountRate?: number;
  taxRate?: number;
  totalPrice?: number;
  discountAmount?: number;
  priceAfterDiscount?: number;
  taxAmount?: number;
  finalPrice?: number;
}

export interface SalesOrderDto extends FullAuditedEntityDto<string> {
  code?: string;
  customerId?: string;
  customerName?: string;
  warehouseId?: string;
  warehouseName?: string;
  orderDate?: string;
  expectedDeliveryDate?: string;
  dueDate?: string;
  status?: SalesOrderStatus;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  totalAmount?: number;
  note?: string;
  details?: SalesOrderDetailDto[];
}

export interface UpdateSalesOrderDetailDto {
  quantity: number;
  discountRate?: number;
  taxRate?: number;
}

export interface UpdateSalesOrderDto {
  warehouseId: string;
  expectedDeliveryDate?: string;
  dueDate?: string;
  note?: string;
}
