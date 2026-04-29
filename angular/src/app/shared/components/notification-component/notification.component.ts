import {
  ChangeDetectionStrategy,
  Component,
  HostListener,
  OnDestroy,
  OnInit,
  signal,
  computed,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NotificationService } from '../../../proxy/notifications/notification.service';
import type { NotificationDto } from '../../../proxy/notifications/dtos/models';
import { NotificationSeverity } from '../../../proxy/enums/notificaitons/notification-severity.enum';
import { NotificationHubService } from './notification.hub.service';

@Component({
  selector: 'app-notification',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './notification.component.html',
  styleUrl: './notification.component.scss',
})
export class NotificationComponent implements OnInit, OnDestroy {
  items   = signal<NotificationDto[]>([]);
  loading = signal(false);
  open    = signal(false);
  unread  = computed(() => this.items().filter(n => !n.isRead).length);

  private sub = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private hubService: NotificationHubService,
  ) {}

  ngOnInit(): void {
    this.load();

    this.sub.add(
      this.hubService.received$.subscribe(dto => {
        this.items.update(list => [dto, ...list]);
      })
    );
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  toggle(): void {
    this.open.update(v => !v);
  }

  markRead(n: NotificationDto): void {
    if (n.isRead) return;
    this.notificationService.markRead(n.id).subscribe(() => {
      this.items.update(list =>
        list.map(x => (x.id === n.id ? { ...x, isRead: true } : x))
      );
    });
  }

  markAllRead(): void {
    const ids = this.items().filter(n => !n.isRead).map(n => n.id);
    if (!ids.length) return;
    this.notificationService.markAllRead(ids).subscribe(() => {
      this.items.update(list => list.map(x => ({ ...x, isRead: true })));
    });
  }

  sevClass(s: NotificationSeverity): string {
    return (['info', 'success', 'warning', 'error'] as const)[s] ?? 'info';
  }

  sevIcon(s: NotificationSeverity): string {
    const icons: Record<number, string> = {
      [NotificationSeverity.Info]:    'M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10zm0-6v-4m0-4h.01',
      [NotificationSeverity.Success]: 'M22 11.08V12a10 10 0 1 1-5.93-9.14M22 4 12 14.01l-3-3',
      [NotificationSeverity.Warning]: 'M10.29 3.86 1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0zM12 9v4m0 4h.01',
      [NotificationSeverity.Error]:   'M12 22c5.523 0 10-4.477 10-10S17.523 2 12 2 2 6.477 2 12s4.477 10 10 10zm4-14-8 8m0-8 8 8',
    };
    return icons[s] ?? icons[NotificationSeverity.Info];
  }

  private load(): void {
    this.loading.set(true);
    this.notificationService
      .getList({ maxResultCount: 20, skipCount: 0 })
      .subscribe({
        next: result => this.items.set(result.items),
        complete: () => this.loading.set(false),
      });
  }

  @HostListener('document:click')
  onOutsideClick(): void {
    this.open.set(false);
  }
}