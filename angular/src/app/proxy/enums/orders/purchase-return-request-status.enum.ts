import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseReturnRequestStatus {
  Draft = 1,
  PendingApproval = 2,
  Approved = 3,
  Rejected = 4,
  Processed = 5,
}

export const purchaseReturnRequestStatusOptions = mapEnumToOptions(PurchaseReturnRequestStatus);
