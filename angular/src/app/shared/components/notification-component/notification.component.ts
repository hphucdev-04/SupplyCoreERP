import { ChangeDetectionStrategy, Component, HostListener, OnDestroy, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { OAuthService } from 'angular-oauth2-oidc';
import { NotificationService } from '../../../proxy/notifications/notification.service';
import { NotificationHubService } from '../../services/signalR/notification.hub.service';
import { NotificationSeverity } from '../../../proxy/enums/notificaitons/notification-severity.enum';
import type { NotificationDto } from '../../../proxy/notifications/dtos/models';
import { SharedModule } from '../../shared.module';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [SharedModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.scss',
})
export class NotificationComponent implements OnInit, OnDestroy {
  items = signal<NotificationDto[]>([]);
  loading = signal(false);
  open = signal(false);
  unread = computed(() => this.items().filter(n => !n.isRead).length);

  private sub = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private hubService: NotificationHubService,
    private oauthService: OAuthService,
  ) {}

  ngOnInit(): void {
    if (this.oauthService.hasValidAccessToken()) {
      this.loadInitial();
    }

    this.sub.add(
      this.hubService.received$.subscribe(dto => {
        this.items.update(list => {
          if (list.some(x => x.id === dto.id)) return list;
          return [dto, ...list];
        });
      })
    );
  }

  private loadInitial(): void {
    this.loading.set(true);
    this.notificationService.getList({ maxResultCount: 20, skipCount: 0 }).subscribe({
      next: result => this.items.set(result.items),
      complete: () => this.loading.set(false),
    });
  }

  toggle(): void { this.open.update(v => !v); }

  markRead(n: NotificationDto): void {
    if (n.isRead) return;
    this.notificationService.markRead(n.id).subscribe(() => {
      this.items.update(list => list.map(x => x.id === n.id ? { ...x, isRead: true } : x));
    });
  }

  markAllRead(): void {
    const ids = this.items().filter(n => !n.isRead).map(n => n.id);
    if (!ids.length) return;
    this.notificationService.markAllRead(ids).subscribe(() => {
      this.items.update(list => list.map(x => ({ ...x, isRead: true })));
    });
  }

  delete(n: NotificationDto, event: MouseEvent): void {
    event.stopPropagation();
    this.notificationService.markDelete(n.id).subscribe(() => {
      this.items.update(list => list.filter(x => x.id !== n.id));
    });
  }

  deleteAll(): void {
    const ids = this.items().map(n => n.id);
    if (!ids.length) return;
    this.notificationService.markAllDelete(ids).subscribe(() => {
      this.items.set([]);
    });
  }

  sevClass(s: NotificationSeverity): string {
    return (['info', 'success', 'warning', 'error'] as const)[s] ?? 'info';
  }

  sevIcon(s: NotificationSeverity): string {
    const icons: Record<number, string> = {
      [NotificationSeverity.Info]: 'M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10zm0-6v-4m0-4h.01',
      [NotificationSeverity.Success]: 'M22 11.08V12a10 10 0 1 1-5.93-9.14M22 4 12 14.01l-3-3',
      [NotificationSeverity.Warning]: 'M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0zM12 9v4m0 4h.01',
      [NotificationSeverity.Error]: 'M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10zm4-14-8 8m0-8 8 8',
    };
    return icons[s] ?? icons[NotificationSeverity.Info];
  }

  @HostListener('document:click')
  onOutsideClick(): void { this.open.set(false); }

  ngOnDestroy(): void { this.sub.unsubscribe(); }
}