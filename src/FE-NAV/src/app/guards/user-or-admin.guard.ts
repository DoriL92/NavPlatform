import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { filter, map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const userOrAdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If still loading, wait for authentication to complete
  if (authService.isLoading) {
    return authService.authState$.pipe(
      filter(authState => !authState.isLoading),
      map(authState => {
        if (authState.isAuthenticated) {
          return true; // Allow both regular users and admin users
        } else {
          router.navigate(['/login']);
          return false;
        }
      }),
      take(1)
    );
  }

  if (authService.isAuthenticated) {
    return true; // Allow both regular users and admin users
  }

  router.navigate(['/login']);
  return false;
};
