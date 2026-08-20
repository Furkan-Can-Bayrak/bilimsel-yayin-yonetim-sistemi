import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppNotification } from '../models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/notifications`;

  private readonly unreadCountSignal = signal(0);
  readonly unreadCount = this.unreadCountSignal.asReadonly();

  getAll(take = 50): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(this.baseUrl, {
      params: { take: String(take) },
    });
  }

  markRead(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/read`, {}).pipe(
      tap(() => {
        this.unreadCountSignal.update((count) => Math.max(0, count - 1));
      }),
    );
  }

  /** Nav rozeti için okunmamış sayısını yeniler. */
  refreshUnreadCount(): void {
    this.getAll(50).subscribe({
      next: (items) => this.syncUnreadFrom(items),
      error: () => this.unreadCountSignal.set(0),
    });
  }

  syncUnreadFrom(items: AppNotification[]): void {
    this.unreadCountSignal.set(items.filter((item) => !item.isRead).length);
  }

  clearUnreadCount(): void {
    this.unreadCountSignal.set(0);
  }
}
