import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/manuscript.model';
import { AppNotification } from '../models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/notifications`;

  private readonly unreadCountSignal = signal(0);
  readonly unreadCount = this.unreadCountSignal.asReadonly();

  getPage(page = 1, pageSize = 10): Observable<PagedResult<AppNotification>> {
    return this.http.get<PagedResult<AppNotification>>(this.baseUrl, {
      params: {
        page: String(page),
        pageSize: String(pageSize),
      },
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
    this.http.get<{ count: number }>(`${this.baseUrl}/unread-count`).subscribe({
      next: (res) => this.unreadCountSignal.set(res.count),
      error: () => this.unreadCountSignal.set(0),
    });
  }

  clearUnreadCount(): void {
    this.unreadCountSignal.set(0);
  }
}
