import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AppNotification } from '../../../core/models/notification.model';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-admin-notifications',
  imports: [DatePipe, RouterLink],
  templateUrl: './admin-notifications.html',
  styleUrl: './admin-notifications.css',
})
export class AdminNotifications implements OnInit {
  private readonly api = inject(NotificationService);

  readonly items = signal<AppNotification[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly busyId = signal<number | null>(null);

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api.getAll().subscribe({
      next: (data) => {
        this.items.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Bildirimler yüklenemedi.');
        this.loading.set(false);
      },
    });
  }

  markRead(item: AppNotification): void {
    if (item.isRead) {
      return;
    }

    this.busyId.set(item.id);
    this.api.markRead(item.id).subscribe({
      next: () => {
        this.busyId.set(null);
        this.reload();
      },
      error: () => {
        this.busyId.set(null);
        this.error.set('Okundu işaretlenemedi.');
      },
    });
  }
}
