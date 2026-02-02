import { mapEnumToOptions } from '@abp/ng.core';

export enum StorageCondition {
  Normal = 1,
  Cool = 2,
  Cold = 3,
  Frozen = 4,
}

export const storageConditionOptions = mapEnumToOptions(StorageCondition);
