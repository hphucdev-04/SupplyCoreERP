import type { GetNotificationListDto, NotificationDto } from './dtos/models';
import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import type { NotificationSeverity } from '../enums/notificaitons/notification-severity.enum';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private restService = inject(RestService);
  apiName = 'Default';
  

  createForPermission = (title: string, content: string, severity: NotificationSeverity, targetPermissions: string[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, NotificationDto>({
      method: 'POST',
      url: '/api/app/notification/for-permission',
      params: { title, content, severity },
      body: targetPermissions,
    },
    { apiName: this.apiName,...config });
  

  createGlobal = (title: string, content: string, severity: NotificationSeverity, config?: Partial<Rest.Config>) =>
    this.restService.request<any, NotificationDto>({
      method: 'POST',
      url: '/api/app/notification/global',
      params: { title, content, severity },
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetNotificationListDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<NotificationDto>>({
      method: 'GET',
      url: '/api/app/notification',
      params: { isRead: input.isRead, level: input.level, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  markAllRead = (ids: string[], config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/notification/mark-all-read',
      body: ids,
    },
    { apiName: this.apiName,...config });
  

  markRead = (notificationId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/notification/mark-read/${notificationId}`,
    },
    { apiName: this.apiName,...config });
}