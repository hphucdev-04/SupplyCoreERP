import { mapEnumToOptions } from '@abp/ng.core';

export enum BatchQAStatus {
  PendingQA = 0,
  Approved = 1,
  Rejected = 2,
  Recalled = 3,
  Expired = 4,
}

export const batchQAStatusOptions = mapEnumToOptions(BatchQAStatus);
