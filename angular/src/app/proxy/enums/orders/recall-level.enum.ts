import { mapEnumToOptions } from '@abp/ng.core';

export enum RecallLevel {
  Level1 = 1,
  Level2 = 2,
  Level3 = 3,
}

export const recallLevelOptions = mapEnumToOptions(RecallLevel);
