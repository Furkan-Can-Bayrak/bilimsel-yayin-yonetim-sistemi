import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/** Giriş yoksa login ekranına gönderir. */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isLoggedIn()) {
    return true;
  }

  return router.createUrlTree(['/admin/login']);
};

/**
 * Token yetmez: ilgili izin yoksa ana sayfaya döner.
 * Backend zaten 403 verir; bu guard menüden atlanan URL'leri de keser.
 */
export const permissionGuard = (permission: string): CanActivateFn => {
  return permissionGuardAny(permission);
};

export const permissionGuardAny = (...permissions: string[]): CanActivateFn => {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isLoggedIn()) {
      return router.createUrlTree(['/admin/login']);
    }

    if (!auth.hasAnyPermission(...permissions)) {
      return router.createUrlTree(['/']);
    }

    return true;
  };
};
