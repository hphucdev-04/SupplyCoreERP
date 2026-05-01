import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { OAuthService } from 'angular-oauth2-oidc';

export abstract class BaseHubService {
  protected hub: HubConnection | null = null;
  protected abstract readonly hubUrl: string;

  constructor(protected oauthService: OAuthService) {}

  connect(): void {
    if (this.hub?.state === 'Connected') return;

    this.hub = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.oauthService.getAccessToken(), // Gửi kèm accesstoken
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000]) // Tự động kết nối lại 
      .configureLogging(LogLevel.Warning)
      .build();

    this.registerEvents();

    this.hub.start().catch(err => console.error(`[SignalR] Kết nối thất bại tới ${this.hubUrl}:`, err));
  }

  disconnect(): void {
    if (this.hub) {
      this.hub.stop();
      this.hub = null;
    }
  }

  // Bắt buộc lớp con tự định nghĩa
  protected abstract registerEvents(): void;
}