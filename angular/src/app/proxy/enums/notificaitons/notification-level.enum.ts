import { mapEnumToOptions } from '@abp/ng.core';

export enum NotificationLevel {
  Global = 0,
  Permission = 1,
}

export const notificationLevelOptions = mapEnumToOptions(NotificationLevel);
