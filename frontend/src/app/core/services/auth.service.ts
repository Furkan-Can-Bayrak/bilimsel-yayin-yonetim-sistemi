import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthSession, LoginRequest, LoginResponse } from '../models/auth.model';
import { Permissions } from '../auth/permissions';

const SESSION_KEY = 'byys_session';
const LEGACY_TOKEN_KEY = 'blog_access_token';
const LEGACY_USER_KEY = 'blog_username';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly sessionSignal = signal<AuthSession | null>(this.readSession());

  readonly session = this.sessionSignal.asReadonly();
  readonly isLoggedIn = computed(() => this.isSessionActive(this.sessionSignal()));
  readonly displayName = computed(() => {
    const session = this.sessionSignal();
    return session?.fullName || session?.email || null;
  });
  readonly permissions = computed(() => this.sessionSignal()?.permissions ?? []);

  getToken(): string | null {
    const session = this.sessionSignal();
    return this.isSessionActive(session) ? session!.accessToken : null;
  }

  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }

  hasAnyPermission(...permissions: string[]): boolean {
    return permissions.some((permission) => this.hasPermission(permission));
  }

  login(body: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, body)
      .pipe(tap((res) => this.persist(res)));
  }

  /**
   * Girişten sonra kullanıcının ilk görebileceği yönetim ekranı.
   * İzni yoksa public listeye düşer.
   */
  pathAfterLogin(): string {
    if (this.hasPermission(Permissions.Manuscripts.ViewAll) ||
        this.hasPermission(Permissions.Manuscripts.Create)) {
      return '/admin';
    }
    if (this.hasPermission(Permissions.Notifications.View)) {
      return '/admin/notifications';
    }

    return '/';
  }

  logout(): void {
    this.clearStorage();
    this.sessionSignal.set(null);
    void this.router.navigateByUrl('/admin/login');
  }

  private persist(res: LoginResponse): void {
    const session: AuthSession = {
      accessToken: res.accessToken,
      expiresAtUtc: res.expiresAtUtc,
      userId: res.userId,
      email: res.email,
      fullName: res.fullName,
      roles: res.roles ?? [],
      permissions: res.permissions ?? [],
    };

    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
    localStorage.removeItem(LEGACY_TOKEN_KEY);
    localStorage.removeItem(LEGACY_USER_KEY);
    this.sessionSignal.set(session);
  }

  private readSession(): AuthSession | null {
    localStorage.removeItem(LEGACY_TOKEN_KEY);
    localStorage.removeItem(LEGACY_USER_KEY);

    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as AuthSession;
      if (!parsed.accessToken || !Array.isArray(parsed.permissions)) {
        this.clearStorage();
        return null;
      }

      if (!this.isSessionActive(parsed)) {
        this.clearStorage();
        return null;
      }

      return parsed;
    } catch {
      this.clearStorage();
      return null;
    }
  }

  private isSessionActive(session: AuthSession | null): boolean {
    if (!session?.accessToken) {
      return false;
    }

    const expires = Date.parse(session.expiresAtUtc);
    return Number.isNaN(expires) || expires > Date.now();
  }

  private clearStorage(): void {
    localStorage.removeItem(SESSION_KEY);
    localStorage.removeItem(LEGACY_TOKEN_KEY);
    localStorage.removeItem(LEGACY_USER_KEY);
  }
}
