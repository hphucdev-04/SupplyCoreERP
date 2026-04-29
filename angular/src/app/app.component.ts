import { Component, OnInit } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { NotificationHubService } from './shared/components/notification-component/notification.hub.service';
import { OAuthService } from 'angular-oauth2-oidc';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
  `,
  imports: [LoaderBarComponent, DynamicLayoutComponent],
})
export class AppComponent implements OnInit {
  constructor(
    private oauthService: OAuthService,
    private hubService: NotificationHubService,
  ) { }

  ngOnInit(): void {
    // Start SignalR ngay khi có token hợp lệ
    if (this.oauthService.hasValidAccessToken()) {
      console.log('[App] Đã có token, kết nối Hub ngay...');
      this.hubService.connect();
    }

    // Vẫn giữ cái này để xử lý khi người dùng Login/Logout/Refresh token mà không F5
    this.oauthService.events.subscribe(e => {
      if (e.type === 'token_received' || e.type === 'token_refreshed') {
        console.log('[App] Token mới nhận/refreshed, kết nối Hub...');
        this.hubService.connect();
      } else if (e.type === 'logout') {
        this.hubService.disconnect();
      }
    });
  }
}
