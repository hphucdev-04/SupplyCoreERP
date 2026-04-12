import { mapEnumToOptions } from '@abp/ng.core';

export enum SalesOrderStatus {
  Draft = 1,
  PendingApproval = 2,
  Approved = 3,
  Delivering = 4,
  Completed = 5,
  Canceled = 6,
}

export const salesOrderStatusOptions = mapEnumToOptions(SalesOrderStatus);
