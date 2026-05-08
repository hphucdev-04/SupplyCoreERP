import { Component } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { NotificationComponent } from './shared/components/notification-component/notification.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LoaderBarComponent, DynamicLayoutComponent, NotificationComponent],
  template: `
    <abp-loader-bar />
    <abp-dynamic-layout />
    <div id="notification" style="position: fixed; top: 0.1rem; right: 7rem; z-index: 1020;">
      <app-notification></app-notification>
    </div>
  `,
})
export class AppComponent {}