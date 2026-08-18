import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Token varsa Authorization ekler.
 * 401 (süresi dolmuş / security_version değişmiş token) oturumu kapatır.
 * 403'e dokunmaz: kullanıcı giriş yapmıştır, yalnızca o işlem yasaktır.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.getToken();

  const outgoing = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(outgoing).pipe(
    catchError((err: HttpErrorResponse) => {
      const isLogin = req.url.includes('/auth/login');
      if (err.status === 401 && !isLogin && auth.isLoggedIn()) {
        auth.logout();
      }

      return throwError(() => err);
    }),
  );
};
