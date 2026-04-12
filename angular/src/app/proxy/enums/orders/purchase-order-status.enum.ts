import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseOrderStatus {
  Draft = 1,
  PendingApproval = 2,
  Approved = 3,
  Receiving = 4,
  Completed = 5,
  Canceled = 6,
}

export const purchaseOrderStatusOptions = mapEnumToOptions(PurchaseOrderStatus);
