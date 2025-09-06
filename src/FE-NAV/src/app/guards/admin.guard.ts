import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoading) {
    return true;
  }

  if (!authService.isAuthenticated) {
    router.navigate(['/login']);
    return false;
  }

  const user = authService.currentUser;
  if (user && user.roles && Array.isArray(user.roles) && user.roles.includes('Admin')) {
    return true;
  }

  router.navigate(['/listing']);
  return false;
}; 