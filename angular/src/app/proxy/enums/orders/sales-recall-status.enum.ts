import { mapEnumToOptions } from '@abp/ng.core';

export enum SalesRecallStatus {
  Draft = 1,
  PendingApproval = 2,
  Approved = 3,
  Recalling = 4,
  Completed = 5,
  Rejected = 6,
}

export const salesRecallStatusOptions = mapEnumToOptions(SalesRecallStatus);
