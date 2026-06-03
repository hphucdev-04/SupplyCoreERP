import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseReturnStatus {
  Draft = 1,
  PendingApproval = 2,
  Approved = 3,
  Returning = 4,
  Completed = 5,
  Rejected = 6,
}

export const purchaseReturnStatusOptions = mapEnumToOptions(PurchaseReturnStatus);
