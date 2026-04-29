import { Injectable, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { OAuthService } from 'angular-oauth2-oidc';
import { EnvironmentService } from '@abp/ng.core'; // 1. Thêm import này
import type { NotificationDto } from '../../../proxy/notifications/dtos/models';

@Injectable({ providedIn: 'root' })
export class NotificationHubService implements OnDestroy {
  private hub: HubConnection | null = null;

  readonly received$ = new Subject<NotificationDto>();

  constructor(
    private oauthService: OAuthService,
    private envService: EnvironmentService // 2. Inject EnvironmentService vào đây
  ) {}

  connect(): void {
    if (this.hub) return;

    const hubUrl = `https://localhost:44367/hubs/notification`; 

    this.hub = new HubConnectionBuilder()
      .withUrl(hubUrl, { // 4. Truyền hubUrl tuyệt đối vào đây
        accessTokenFactory: () => this.oauthService.getAccessToken(),
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.hub.on('ReceiveNotification', (dto: NotificationDto) =>
      this.received$.next(dto)
    );

    this.hub.start().catch(e => console.error('[Hub] start failed', e));
  }

  disconnect(): void {
    this.hub?.stop();
    this.hub = null;
  }

  ngOnDestroy(): void {
    this.disconnect();
  }
}