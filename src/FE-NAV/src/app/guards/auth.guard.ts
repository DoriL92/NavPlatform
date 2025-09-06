import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { filter, map, take } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If still loading, wait for authentication to complete
  if (authService.isLoading) {
    return authService.authState$.pipe(
      filter(authState => !authState.isLoading),
      map(authState => {
        if (authState.isAuthenticated) {
          return true;
        } else {
          router.navigate(['/login']);
          return false;
        }
      }),
      take(1)
    );
  }

  if (authService.isAuthenticated) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};

export const loginGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If still loading, wait for authentication to complete
  if (authService.isLoading) {
    return authService.authState$.pipe(
      filter(authState => !authState.isLoading),
      map(authState => {
        if (authState.isAuthenticated) {
          // Redirect authenticated users to appropriate page
          if (authState.user?.roles?.includes('Admin')) {
            router.navigate(['/admin']);
          } else {
            router.navigate(['/listing']);
          }
          return false;
        }
        return true;
      }),
      take(1)
    );
  }

  if (authService.isAuthenticated) {
    // Redirect authenticated users to appropriate page
    if (authService.currentUser?.roles?.includes('Admin')) {
      router.navigate(['/admin']);
    } else {
      router.navigate(['/listing']);
    }
    return false;
  }

  return true;
};