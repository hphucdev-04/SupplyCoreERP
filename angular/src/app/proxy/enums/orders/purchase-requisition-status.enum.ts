import { mapEnumToOptions } from '@abp/ng.core';

export enum PurchaseRequisitionStatus {
  Draft = 1,
  PendingApproval = 2,
  Approved = 3,
  Rejected = 4,
  PartialOrdered = 5,
  Ordered = 6,
  Canceled = 7,
}

export const purchaseRequisitionStatusOptions = mapEnumToOptions(PurchaseRequisitionStatus);
