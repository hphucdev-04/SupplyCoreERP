import { mapEnumToOptions } from '@abp/ng.core';

export enum StorageCondition {
  Normal = 0,
  Cool = 1,
  Cold = 2,
  Frozen = 3,
  Other = 4,
}

export const storageConditionOptions = mapEnumToOptions(StorageCondition);
