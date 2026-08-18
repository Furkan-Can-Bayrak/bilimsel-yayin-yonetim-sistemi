import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AppNotification } from '../models/notification.model';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/notifications`;

  getAll(take = 50): Observable<AppNotification[]> {
    return this.http.get<AppNotification[]>(this.baseUrl, {
      params: { take: String(take) },
    });
  }

  markRead(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/read`, {});
  }
}
