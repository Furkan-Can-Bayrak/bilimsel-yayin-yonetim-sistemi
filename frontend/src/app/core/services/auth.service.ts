import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from '../models/auth.model';

const TOKEN_KEY = 'blog_access_token';
const USER_KEY = 'blog_username';

/**
 * AuthService = login/logout + token saklama.
 * Laravel'de session/cookie yerine burada JWT'yi localStorage'da tutuyoruz.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenSignal = signal<string | null>(this.readToken());
  private readonly usernameSignal = signal<string | null>(localStorage.getItem(USER_KEY));

  readonly isLoggedIn = computed(() => !!this.tokenSignal());
  readonly username = computed(() => this.usernameSignal());

  getToken(): string | null {
    return this.tokenSignal();
  }

  login(body: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, body)
      .pipe(
        tap((res) => {
          localStorage.setItem(TOKEN_KEY, res.accessToken);
          localStorage.setItem(USER_KEY, res.username);
          this.tokenSignal.set(res.accessToken);
          this.usernameSignal.set(res.username);
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.tokenSignal.set(null);
    this.usernameSignal.set(null);
    void this.router.navigateByUrl('/admin/login');
  }

  private readToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }
}
