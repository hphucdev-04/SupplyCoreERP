import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { NotificationComponent } from './shared/components/notification-component/notification.component';
import { AgentChatComponent } from './shared/components/agent-chat.component/agent-chat.component'; 
import { DrawerComponent } from './shared/components/drawer-component/drawer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    LoaderBarComponent, 
    DynamicLayoutComponent, 
    NotificationComponent, 
    AgentChatComponent,
    DrawerComponent
  ],
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
    
    <div id="notification" style="position: fixed; top: 0.1rem; right: 7rem; z-index: 1020; display: flex; gap: 10px; align-items: center;">
      <button class="btn-ask-ai" (click)="isChatOpen = true" style="display: flex; align-items: center; gap: 8px; padding: 6px 12px; background: linear-gradient(135deg, #0ea5e9 0%, #2563eb 100%); color: white; border: none; border-radius: 6px; font-size: 13px; font-weight: 600; cursor: pointer; box-shadow: 0 2px 8px rgba(37, 99, 235, 0.2); transition: all 0.2s;">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="flex-shrink: 0;"><path d="m12 3-1.912 5.813a2 2 0 0 1-1.275 1.275L3 12l5.813 1.912a2 2 0 0 1 1.275 1.275L12 21l1.912-5.813a2 2 0 0 1 1.275-1.275L21 12l-5.813-1.912a2 2 0 0 1-1.275-1.275L12 3Z"/></svg>
        <span>Ask AI</span>
      </button>
      <app-notification></app-notification>
    </div>

    <app-drawer 
      [isOpen]="isChatOpen" 
      [title]="'Trợ lý AI'" 
      [width]="'lg'" 
      [showFooter]="false" 
      (close)="isChatOpen = false">
      <app-agent-chat *ngIf="isChatOpen"></app-agent-chat>
    </app-drawer>
  `,
})
export class AppComponent {
  isChatOpen = false;
}