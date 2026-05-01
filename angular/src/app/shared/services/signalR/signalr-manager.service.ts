import { Injectable } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { NotificationHubService } from './notification.hub.service';

@Injectable({ providedIn: 'root' })
export class SignalRManager {
  private hubs = [this.notificationHub];
  // thêm hub mới ở đây

  constructor(
    private oauthService: OAuthService,
    private notificationHub: NotificationHubService
  ) {}

  init(): void {
    // Connect khi có sẵn token
    if (this.oauthService.hasValidAccessToken()) {
      this.hubs.forEach(h => h.connect());
    }

    // Theo giõi login/logout
    this.oauthService.events.subscribe(e => {
      if (e.type === 'token_received' || e.type === 'token_refreshed') {
        this.hubs.forEach(h => h.connect());
      } else if (e.type === 'logout') {
        this.hubs.forEach(h => h.disconnect());
      }
    });
  }
}