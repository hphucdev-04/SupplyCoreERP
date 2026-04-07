import { mapEnumToOptions } from '@abp/ng.core';

export enum ApprovalStatus {
  Draft = 0,
  Pending = 1,
  Approved = 2,
  Rejected = 3,
}

export const approvalStatusOptions = mapEnumToOptions(ApprovalStatus);
