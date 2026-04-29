import type { EntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { NotificationLevel } from '../../enums/notificaitons/notification-level.enum';
import type { NotificationSeverity } from '../../enums/notificaitons/notification-severity.enum';

export interface GetNotificationListDto extends PagedAndSortedResultRequestDto {
  isRead?: boolean;
  level?: NotificationLevel;
}

export interface NotificationDto extends EntityDto<string> {
  title?: string;
  content?: string;
  severity?: NotificationSeverity;
  level?: NotificationLevel;
  targetPermissions?: string[];
  creationTime?: string;
  isRead?: boolean;
}
