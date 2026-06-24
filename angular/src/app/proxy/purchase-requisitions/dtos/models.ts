import type { AuditedEntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { PurchaseRequisitionStatus } from '../../enums/orders/purchase-requisition-status.enum';
import type { PurchaseOrderStatus } from '../../enums/orders/purchase-order-status.enum';

export interface AddPurchaseRequisitionLineDto {
  productId: string;
  unitId: string;
  quantity: number;
  note?: string;
}

export interface ConvertToPurchaseOrderDto {
  allocations: PurchaseOrderAllocationDto[];
  orderDate?: string;
  note?: string;
}

export interface CreatePurchaseRequisitionDto {
  warehouseId: string;
  requestedDate: string;
  requiredDate?: string;
  note?: string;
}

export interface GetPurchaseRequisitionListDto extends PagedAndSortedResultRequestDto {
  filter?: string;
  status?: PurchaseRequisitionStatus;
}

export interface PurchaseOrderAllocationDto {
  requisitionLineId: string;
  supplierId: string;
  warehouseId: string;
  quantity: number;
}

export interface PurchaseRequisitionDto extends FullAuditedEntityDto<string> {
  code?: string;
  warehouseId?: string;
  warehouseName?: string;
  requestedDate?: string;
  requiredDate?: string;
  status?: PurchaseRequisitionStatus;
  note?: string;
  lines?: PurchaseRequisitionLineDto[];
  relatedOrders?: RelatedPurchaseOrderDto[];
}

export interface PurchaseRequisitionLineDto extends AuditedEntityDto<string> {
  productId?: string;
  productName?: string;
  productCode?: string;
  unitId?: string;
  unitName?: string;
  quantity?: number;
  orderedQuantity?: number;
  note?: string;
}

export interface RelatedPurchaseOrderDto {
  id?: string;
  code?: string;
  supplierName?: string;
  status?: PurchaseOrderStatus;
  totalAmount?: number;
  creationTime?: string;
}

export interface UpdatePurchaseRequisitionDto {
  warehouseId: string;
  requiredDate?: string;
  note?: string;
}

export interface UpdatePurchaseRequisitionLineDto {
  quantity: number;
  note?: string;
}
