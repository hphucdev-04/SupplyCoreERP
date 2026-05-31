import { Component } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { NotificationComponent } from './shared/components/notification-component/notification.component';
import { AiChatComponent } from './shared/components/ai-chat.component/ai-chat.component'; 

@Component({
  selector: 'app-root',
  standalone: true,
  // 2. Thêm AiChatComponent vào mảng imports này
  imports: [
    LoaderBarComponent, 
    DynamicLayoutComponent, 
    NotificationComponent, 
    AiChatComponent 
  ],
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
    
    <div id="notification" style="position: fixed; top: 0.1rem; right: 7rem; z-index: 1020;">
      <app-notification></app-notification>
    </div>

    <app-ai-chat></app-ai-chat>
  `,
})
export class AppComponent {}