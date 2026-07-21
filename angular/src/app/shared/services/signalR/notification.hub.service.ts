import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { BaseHubService } from './base-hub.service';
import { OAuthService } from 'angular-oauth2-oidc';
import type { NotificationDto } from '../../../proxy/notifications/dtos/models'; 

@Injectable({ providedIn: 'root' })
export class NotificationHubService extends BaseHubService {
  protected readonly hubUrl = ` https://rxlogistics.up.railway.app/hubs/notification`; // url hub
  readonly received$ = new Subject<NotificationDto>(); // Kênh phát sóng

  constructor(oauthService: OAuthService) {
    super(oauthService);
  }

  protected registerEvents(): void {
    this.hub?.on('ReceiveNotification', (dto: NotificationDto) => {
      this.received$.next(dto); // Bắn data ra cho Component hứng
    });
  }
}